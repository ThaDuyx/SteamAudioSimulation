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

    [SerializeField] private AudioListener mainListener;
    private List<Speaker> speakers;
    private Recorder recorder;
    private Timer timer;
    private Logger logger;
    private Calculator calculator;
    private int activeSpeaker = 0;
    private Room persistedRoom;
    
    public int selectedSpeaker = 0;

    public int SpeakerCount { get { return speakers.Count; } }
    public string ActiveSpeakerName { get { return speakers[activeSpeaker].Name; } }
    public bool IsLastSpeaker { get { return activeSpeaker == speakers.Count - 1; } }
    public int SampleRate { get { return UnityEngine.AudioSettings.outputSampleRate; } }
    public bool IsRendering { get; private set;}
    public float SimulationLength { get { return 6.0f; } private set { value = SimulationLength; } }
    public bool IsTiming { get { return timer.IsActive(); } }
    public string TimeLeft { get { return timer.GetTimeLeft().ToString(); } }                                   
    public string TimeLeftOfRender { get { return timer.GetTimeLeftOfSimulation().ToString(); } }
    public string ActiveSOFAName { get { return SteamAudioManager.Singleton.hrtfNames[SteamAudioManager.Singleton.currentHRTF]; } }
    public bool IsLastSOFA { get { return SteamAudioManager.Singleton.currentHRTF == SteamAudioManager.Singleton.hrtfNames.Length - 1; } }
    
    // Should be modified for specific needs - TODO: Change to dynamic folder structure
    public string folderPath = "/Users/duyx/Code/Jabra/python/renders";

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
        // Fetching audio objects and pairing them in a speaker model
        AudioSource[] audioSources = FindObjectsOfType<AudioSource>();   
        speakers = new List<Speaker>();
        
        calculator = new Calculator();
        
        foreach (var audioSource in audioSources)
        {
            // Initialising speaker list
            SteamAudioSource steamSource = audioSource.gameObject.GetComponent<SteamAudioSource>();
            Speaker speaker = new(audioSource, steamSource);
            speakers.Add(speaker);

            // Calculating geometry
            speaker.DistanceToReceiver = calculator.CalculateDistanceToReceiver(mainListener.transform, audioSource.transform);
            speaker.Azimuth = calculator.CalculateAzimuth(mainListener.transform, audioSource.transform);
            speaker.Elevation = calculator.CalculateElevation(mainListener.transform, audioSource.transform);
        }
        
        // Sorting speakers after name
        speakers.Sort((speaker1, speaker2) => speaker1.Name.CompareTo(speaker2.Name));

        // Try to load a persisted room or else fetch a default one.
        persistedRoom = DataManager.Instance.LoadRoomData(amountOfSpeakers: speakers.Count);

        speakers[selectedSpeaker].audioSource.volume = persistedRoom.sources[selectedSpeaker].volume;
        speakers[selectedSpeaker].steamAudioSource.directMixLevel = persistedRoom.sources[selectedSpeaker].directMixLevel;
        speakers[selectedSpeaker].steamAudioSource.reflectionsMixLevel = persistedRoom.sources[selectedSpeaker].reflectionMixLevel;
    }

    // - Render Methods
    // Start rendering process
    public void StartRender(RenderMethod renderMethod)
    {
        IsRendering = true;
        
        SetRecordingPath();
        
        UpdateSOFA();
        
        switch(renderMethod)
        {
            case RenderMethod.AllAtOnce:
                RewindAndPlayAudioSources();
                break;

            case RenderMethod.OneByOne:
                PlayAudio();
                break;

            default:
                break;
        }

        recorder.ToggleRecording();
        timer.Begin(SimulationLength);
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
        timer.Begin(SimulationLength);
    }

    // Stop rendering process
    public void StopRender()
    {
        IsRendering = false;
        SteamAudioManager.Singleton.currentHRTF = 0;
        timer.Stop();
        recorder.StopRecording();
        
        logger.LogTitle();
        foreach (var speaker in speakers)
        {
            logger.Log(speaker: speaker);
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

    public void PersistRoom()
    {
        persistedRoom.sources[selectedSpeaker].volume = Volume;
        persistedRoom.sources[selectedSpeaker].directMixLevel = DirectMixLevel;
        persistedRoom.sources[selectedSpeaker].reflectionMixLevel = ReflectionMixLevel;
        
        DataManager.Instance.SaveRoomData(room: persistedRoom);
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

    public void SetSelectedSpeacker(int index)
    {
        selectedSpeaker = index;

        speakers[selectedSpeaker].audioSource.volume = persistedRoom.sources[selectedSpeaker].volume;
        speakers[selectedSpeaker].steamAudioSource.directMixLevel = persistedRoom.sources[selectedSpeaker].directMixLevel;
        speakers[selectedSpeaker].steamAudioSource.reflectionsMixLevel = persistedRoom.sources[selectedSpeaker].reflectionMixLevel;
    }

    private void SetRecordingPath()
    {
        string timeStamp = DateTime.Now.ToString("ddMM-yy_HHmmss");
        folderPath += timeStamp + "/";
        System.IO.Directory.CreateDirectory(folderPath);
    }
}