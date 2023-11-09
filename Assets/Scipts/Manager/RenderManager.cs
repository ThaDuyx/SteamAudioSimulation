using System;
using System.Collections.Generic;
using SteamAudio;
using UnityEngine;

public enum RenderMethod
{
   OneByOne,
   AllAtOnce
}
public class RenderManager : MonoBehaviour
{
    // Singleton object
    public static RenderManager Instance { get; private set; }

    private AudioListener receiver;
    private List<Speaker> speakers;
    private Recorder recorder;
    private Room room;
    private Timer timer;
    private Logger logger;
    private Calculator calculator;
    
    // Integer used for tracking which speaker should play during the OneByOne render method
    private int activeSpeaker = 0;
    // Integer used for selecting speakers and configuring their parameters
    private int selectedSpeaker = 0;

    public int SpeakerCount { get { return speakers.Count; } }
    public string ActiveSpeakerName { get { return speakers[activeSpeaker].Name; } }
    public bool IsLastSpeaker { get { return activeSpeaker == speakers.Count - 1; } }
    public int SampleRate { get { return UnityEngine.AudioSettings.outputSampleRate; } }
    public bool IsRendering { get; private set; }
    public float SimulationLength { get { return 6.0f; } private set { SimulationLength = value; } }
    public bool IsTiming { get { return timer.IsActive(); } }
    public string TimeLeft { get { return timer.GetTimeLeft().ToString(); } }                                   
    public string TimeLeftOfRender { get { return timer.GetTimeLeftOfSimulation().ToString(); } }
    public int SOFACount { get { return SteamAudioManager.Singleton.hrtfNames.Length; }}
    public string[] SOFANames { get { return SteamAudioManager.Singleton.hrtfNames; } }
    public string ActiveSOFAName { get { return SteamAudioManager.Singleton.hrtfNames[SteamAudioManager.Singleton.currentHRTF]; } }
    public bool IsLastSOFA { get { return SteamAudioManager.Singleton.currentHRTF == SteamAudioManager.Singleton.hrtfNames.Length - 1; } }
    
    // Should be modified for specific needs - TODO: Change to dynamic folder structure
    public static string folderPath = "/Users/duyx/Code/Jabra/python/renders";
    public string recordingPath;
    
    public static string defaultClipName = "sweep_48kHz";

    // Basic Unity MonoBehaviour method - Lifecycle process
    private void Awake()
    {
        if (Instance != null && Instance != this)
        { 
            Destroy(this); 
        }
        else 
        { 
            Instance = this; 
        }

        // Initialise at the Awake lifecycle in order to have it ready for the view to read
        SetContent();
    }

    // Basic Unity MonoBehaviour method - Essentially a start-up function / Constructor of the class
    void Start()
    {
        recorder = new Recorder(outputSampleRate: SampleRate);
        logger = new Logger();
        timer = gameObject.AddComponent<Timer>();
    }

    private void SetContent()
    {
        // Find Receiver in scene
        receiver = FindObjectOfType<AudioListener>();

        // Fetching audio objects and pairing them in a speaker model
        AudioSource[] audioSources = FindObjectsOfType<AudioSource>();
        speakers = new List<Speaker>();
        
        calculator = new Calculator();
        
        foreach (var audioSource in audioSources)
        {
            // Initialising speaker list
            SteamAudioSource steamSource = audioSource.gameObject.GetComponent<SteamAudioSource>();
            Speaker speaker = new(audioSource, steamSource);
            speaker.audioSource.clip = Resources.Load<AudioClip>("Audio/" + defaultClipName);
            speakers.Add(speaker);

            // Calculating geometry
            speaker.DistanceToReceiver = calculator.CalculateDistanceToReceiver(receiver.transform, audioSource.transform);
            speaker.Azimuth = calculator.CalculateAzimuth(receiver.transform, audioSource.transform);
            speaker.Elevation = calculator.CalculateElevation(receiver.transform, audioSource.transform);
        }
        
        // Sorting speakers after name
        speakers.Sort((speaker1, speaker2) => speaker1.Name.CompareTo(speaker2.Name));

        // Try to load a persisted room or else fetch a default one.
        room = DataManager.Instance.LoadRoomData(amountOfSpeakers: speakers.Count);

        speakers[selectedSpeaker].audioSource.volume = room.sources[selectedSpeaker].volume;
        speakers[selectedSpeaker].steamAudioSource.directMixLevel = room.sources[selectedSpeaker].directMixLevel;
        speakers[selectedSpeaker].steamAudioSource.reflectionsMixLevel = room.sources[selectedSpeaker].reflectionMixLevel;
        speakers[selectedSpeaker].audioSource.clip = Resources.Load<AudioClip>("Audio/" + room.sources[selectedSpeaker].audioClip);
        speakers[selectedSpeaker].steamAudioSource.applyHRTFToReflections = room.sources[selectedSpeaker].applyHRTFToReflections == 1;
    }

    // - Render Methods
    // Start rendering process
    public void StartRender(RenderMethod renderMethod, int sofaIndex)
    {
        IsRendering = true;
        
        SetRecordingPath();                                             // updates the output for recordings
        
        switch(renderMethod)
        {
            case RenderMethod.AllAtOnce:
                UpdateSOFA();                                           // increments array of SOFA files to our
                RewindAndPlayAudioSources();                            // Resets and play every audio source in the current scene
                break;

            case RenderMethod.OneByOne:
                SteamAudioManager.Singleton.currentHRTF = sofaIndex;    // Selects which SOFA file should be used in the render
                PlayAudio();                                            // Plays the first speaker
                break;

            default:
                break;
        }

        recorder.ToggleRecording();
        timer.Begin(SimulationLength, renderMethod);
    }

    // Updates the state to continue rendering with the next HRTF in the list
    public void ContinueRender(RenderMethod renderMethod)
    {
        recorder.ToggleRecording();             // Stop previous recording

        switch(renderMethod)
        {
            case RenderMethod.AllAtOnce:
                UpdateSOFA();                   // Moves to next HRTF
                RewindAndPlayAudioSources();        
                break;

            case RenderMethod.OneByOne:
                SelectNextSpeaker();            // Moves to next speaker  

                PlayAudio();
                break;

            default:
                break;
        }

        recorder.ToggleRecording();             // Start new recording
        timer.Begin(SimulationLength, renderMethod);          // Start new timer
    }

    // Stop rendering process
    public void StopRender(RenderMethod renderMethod)
    {
        IsRendering = false;
        timer.Stop();
        recorder.StopRecording();
        
        logger.LogTitle();
        foreach (var speaker in speakers)
        {
            logger.Log(speaker: speaker);
        }

        switch (renderMethod)
        {
            case RenderMethod.AllAtOnce:
                SteamAudioManager.Singleton.currentHRTF = 0;       
                break;

            case RenderMethod.OneByOne:
                activeSpeaker = 0;
                break;
        }
    }

    // Used by the AudioCapturer class in conjunction with OnAudioFilterRead() which is a MonoBehavior class that needs an AudioSource.
    // This method binds the Recorder class together with the Audio.
    public void TransmitData(float[] data)
    {
        if (recorder != null && recorder.IsRecording() && IsRendering) {
            recorder.ConvertAndWrite(data);
        }
    }

    // - Audio Methods
    // Called when we want to move to the next HRTF in our list and return back to the first one when we are at the last
    private void UpdateSOFA()
    {
        if (IsLastSOFA)
        { 
            SteamAudioManager.Singleton.currentHRTF = 0;
        }
        else 
        { 
            SteamAudioManager.Singleton.currentHRTF++;
        }
    }
    
    // Called when we want to begin a new render
    private void RewindAndPlayAudioSources()
    {
        foreach (Speaker speaker in speakers)
        {
            speaker.audioSource.Stop();
            speaker.audioSource.Play();
        }
    }

    private void SelectNextSpeaker()
    {
        activeSpeaker++;
    }

    public void PlayAudio()
    {
        speakers[activeSpeaker].audioSource.Play();
    }

    public void PlayAllAudio()
    {
        foreach (Speaker speaker in speakers)
        {
            speaker.audioSource.Play();
            Debug.Log(speaker.Name + " playing");
        }
    }

    public void StopAudio()
    {
        speakers[selectedSpeaker].audioSource.Stop();
    }

    public void StopAllAudio()
    {
        foreach (Speaker speaker in speakers)
        {
            speaker.audioSource.Stop();
        }
    }

    public void ToggleAudio()
    {
        if (speakers[selectedSpeaker].audioSource.isPlaying)
        {
            StopAudio();
        }
        else 
        {
            PlayAudio();
        }
    }

    public void ToggleAllAudio()
    {
        if (AnySpeakerPlaying()) 
        { 
            StopAllAudio(); 
        }
        else 
        { 
            PlayAllAudio();
        }
    }

    private bool AnySpeakerPlaying()
    {
        foreach (Speaker speaker in speakers)
        {
            if (speaker.audioSource.isPlaying)
            {
                return true;
            }
        }
        
        return false;
    }

    public int RealTimeBounces
    {
        get { return SteamAudioSettings.Singleton.realTimeBounces; }
        set { SteamAudioSettings.Singleton.realTimeBounces = value;}
    }

    public bool ApplyHRTFToReflections
    {
        get { return speakers[selectedSpeaker].steamAudioSource.applyHRTFToReflections; }
        set { speakers[selectedSpeaker].steamAudioSource.applyHRTFToReflections = value; }
    }
    public float Volume
    {
        get { return speakers[selectedSpeaker].audioSource.volume; }
        set { speakers[selectedSpeaker].audioSource.volume = value; }
    }

    public float DirectMixLevel
    {
        get { return speakers[selectedSpeaker].steamAudioSource.directMixLevel; }
        set { speakers[selectedSpeaker].steamAudioSource.directMixLevel = value; }
    }

    public float ReflectionMixLevel
    {
        get { return speakers[selectedSpeaker].steamAudioSource.reflectionsMixLevel; }
        set { speakers[selectedSpeaker].steamAudioSource.reflectionsMixLevel = value; }
    }

    public string AudioClip { 
        get 
        {
            return speakers[selectedSpeaker].audioSource.clip.name; 
        } 
        set 
        { 
            // Deletes the 4 last characters of the string meaning either '.wav' or '.mp3'. Unity does not use the file type when searching in the library.
            string audioClipWithoutFileType = value[..^4];

            // Replace the audio clip with the new one
            speakers[selectedSpeaker].audioSource.clip = Resources.Load<AudioClip>("Audio/" + audioClipWithoutFileType);
        }
    }

    public void PersistRoom()
    {
        room.sources[selectedSpeaker].volume = Volume;
        room.sources[selectedSpeaker].directMixLevel = DirectMixLevel;
        room.sources[selectedSpeaker].reflectionMixLevel = ReflectionMixLevel;
        room.sources[selectedSpeaker].audioClip = AudioClip;
        room.sources[selectedSpeaker].applyHRTFToReflections = ApplyHRTFToReflections ? 1 : 0;
        
        DataManager.Instance.SaveRoomData(room: room);
    }
    
    public string[] GetSpeakerNames()
    {
        string[] names = new string[speakers.Count];

        for (int i = 0; i < speakers.Count; i++)
        {
            names[i] = speakers[i].Name;
        }
        
        return names;
    }

    public void SetSelectedSpeaker(int index)
    {
        selectedSpeaker = index;

        speakers[selectedSpeaker].audioSource.volume = room.sources[selectedSpeaker].volume;
        speakers[selectedSpeaker].steamAudioSource.directMixLevel = room.sources[selectedSpeaker].directMixLevel;
        speakers[selectedSpeaker].steamAudioSource.reflectionsMixLevel = room.sources[selectedSpeaker].reflectionMixLevel;
        speakers[selectedSpeaker].audioSource.clip = Resources.Load<AudioClip>("Audio/" + room.sources[selectedSpeaker].audioClip);
        speakers[selectedSpeaker].steamAudioSource.applyHRTFToReflections = room.sources[selectedSpeaker].applyHRTFToReflections == 1;
    }

    private void SetRecordingPath()
    {
        string timeStamp = DateTime.Now.ToString("ddMM-yy_HHmmss");
        recordingPath = folderPath + timeStamp + "/";
        System.IO.Directory.CreateDirectory(recordingPath);
    }
}