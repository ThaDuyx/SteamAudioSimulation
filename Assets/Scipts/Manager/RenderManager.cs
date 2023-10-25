using System;
using System.Collections.Generic;
using SteamAudio;
using UnityEngine;

public enum RenderMethod
{
   LoneSpeaker,
   AllSpeakers
}
public class RenderManager : MonoBehaviour
{
    // Singleton object
    public static RenderManager Instance { get; private set; }

    [SerializeField] private AudioListener mainListener;
    private AudioSource[] audioSources;
    private List<Speaker> speakers;
    private Recorder recorder;
    private Timer timer;
    private Logger logger;
    private Calculator calculator;
    private int activeSpeaker = 0;
    
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
        recorder = new Recorder(UnityEngine.AudioSettings.outputSampleRate);
        logger = new Logger();
        timer = gameObject.AddComponent<Timer>();
        audioSources = FindObjectsOfType<AudioSource>();
    }

    private void SetContent()
    {
        audioSources = FindObjectsOfType<AudioSource>();   
        speakers = new List<Speaker>();
        calculator = new Calculator();
        
        foreach (var audioSource in audioSources)
        {
            SteamAudioSource steamSource = audioSource.gameObject.GetComponent<SteamAudioSource>();
            Speaker speaker = new(audioSource, steamSource);
            speakers.Add(speaker);

            speaker.DistanceToReceiver = calculator.CalculateDistanceToReceiver(mainListener.transform, audioSource.transform);
            speaker.Azimuth = calculator.CalculateAzimuth(mainListener.transform, audioSource.transform);
            speaker.Elevation = calculator.CalculateElevation(mainListener.transform, audioSource.transform);
        }

        speakers.Sort((speaker1, speaker2) => speaker1.Name.CompareTo(speaker2.Name));        
    }

    // - Render Methods
    // Start rendering process
    public void StartRender(RenderMethod method)
    {
        IsRendering = true;
        SetRecordingPath();
        UpdateSOFA();
        
        switch(method)
        {
            case RenderMethod.AllSpeakers:
                ResetAudioSources();
                break;
            case RenderMethod.LoneSpeaker:
                PlayAudio();
                break;
            default:
                break;
        }

        recorder.ToggleRecording();
        timer.Begin(SimulationLength);
    }

    // Updates the state to continue rendering with the next HRTF in the list
    public void ContinueRender(RenderMethod method)
    {
        recorder.ToggleRecording();         // Stop previous recording

        switch(method)
        {
            case RenderMethod.AllSpeakers:
                UpdateSOFA();               // Moves to next HRTF
                ResetAudioSources();        
                break;

            case RenderMethod.LoneSpeaker:
                UpdateSpeakerAndPlay();     // Moves to next speaker            
                break;

            default:
                break;
        }

        recorder.ToggleRecording();         // Start new recording
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
    private void ResetAudioSources()
    {
        foreach (Speaker speaker in speakers)
        {
            speaker.audioSource.Stop();
            speaker.audioSource.Play();
        }
    }

    private void UpdateSpeakerAndPlay()
    {
        activeSpeaker++;
        PlayAudio();
    }

    public void PlayAudio()
    {
        speakers[activeSpeaker].audioSource.Play();
    }

    public void PlayAllAudio()
    {
        foreach (var audioSource in audioSources)
        {
            audioSource.Play();
        }
    }

    public void StopAudio()
    {
        audioSources[activeSpeaker].Stop();
    }

    public void ToggleAudio()
    {
        if (audioSources[activeSpeaker].isPlaying) 
        { 
            StopAudio(); 
        }
        else 
        { 
            PlayAudio(); 
        }
    }

    private void SetRecordingPath()
    {
        string timeStamp = DateTime.Now.ToString("ddMM-yy_HHmmss");
        folderPath += timeStamp + "/";
        System.IO.Directory.CreateDirectory(folderPath);
    }

       public int GetRealTimeBounces() 
    {
        return SteamAudioSettings.Singleton.realTimeBounces;
    }

    public void SetRealTimeBounces(float value)
    {
        // Converting to integer
        SteamAudioSettings.Singleton.realTimeBounces = (int)value;
    }

    public bool GetHRTFReflectionStatus()
    {
        return speakers[activeSpeaker].steamAudioSource.applyHRTFToReflections;
    }

    public void SetHRTFReflectionStatus(bool value)
    {
        speakers[activeSpeaker].steamAudioSource.applyHRTFToReflections = value;
    }
}