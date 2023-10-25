using System;
using System.Collections.Generic;
using SteamAudio;
using UnityEngine;

public enum RenderMethod
{
   LoneSpeaker,
   AllSpeakers
}
public class SimulationManager : MonoBehaviour
{
    // Singleton object
    public static SimulationManager Instance { get; private set; }

    [SerializeField] private AudioListener mainListener;
    private AudioSource[] audioSources;
    private SteamAudioManager steamAudioManager;
    private SteamAudioSource[] steamAudioSources;
    private List<Speaker> speakers;
    private Recorder recorder;
    private Timer timer;
    private Logger logger;
    private Calculator calculator;
    private int activeSpeaker = 0;
    
    public string folderPath = "/Users/duyx/Code/Jabra/python/renders";
    public bool IsRendering { get; private set;}
    public int SampleRate { get { return UnityEngine.AudioSettings.outputSampleRate; } }
    public int SpeakerCount { get { return speakers.Count; } }
    public float SimulationLength { get { return 8.0f; } private set { value = SimulationLength; } }

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
        SteamAudioManager[] steamAudioManagers = FindObjectsOfType<SteamAudioManager>();
        steamAudioManager = steamAudioManagers[0];

        steamAudioSources = FindObjectsOfType<SteamAudioSource>();
        audioSources = FindObjectsOfType<AudioSource>();
        
        speakers = new List<Speaker>();
        calculator = new Calculator();
        
        foreach (var audioSource in audioSources)
        {
            var steamSource = audioSource.gameObject.GetComponent<SteamAudioSource>();
            Speaker speaker = new(audioSource, steamSource);
            speakers.Add(speaker);

            speaker.DistanceToReceiver = calculator.CalculateDistanceToReceiver(mainListener.transform, audioSource.transform);
            speaker.Azimuth = calculator.CalculateAzimuth(mainListener.transform, audioSource.transform);
            speaker.Elevation = calculator.CalculateElevation(mainListener.transform, audioSource.transform);
        }

        speakers.Sort((speaker1, speaker2) => speaker1.Name.CompareTo(speaker2.Name));        
    }

    // Start rendering process
    public void StartRender(RenderMethod method)
    {
        IsRendering = true;
        SetRecordingPath();
        UpdateHRTF();
        
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
                UpdateHRTF();               // Moves to next HRTF
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
        steamAudioManager.currentHRTF = 0;
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

    public string CurrentHRTFName()
    {
        return steamAudioManager.hrtfNames[steamAudioManager.currentHRTF]; // SOFA file name
    }

    public string CurrentSpeakerName()
    {
        return speakers[activeSpeaker].Name;
    }

    // Called when we want to move to the next HRTF in our list and return back to the first one when we are at the last
    private void UpdateHRTF()
    {
        if (IsLastHRTF())
        { 
            steamAudioManager.currentHRTF = 0; 
        }
        else 
        { 
            steamAudioManager.currentHRTF++; 
        }
    }

    // Returning true if the rendering process has reached the end of the HRTF array and concludes the render.
    public bool IsLastHRTF()
    {
        return steamAudioManager.currentHRTF == steamAudioManager.hrtfNames.Length - 1;
    }

    // This method retrieves the amount of custom HRTF's in the scene
    public int AmountOfHRTFs()
    {
        // -1 since the first HRTF in the manager is always the system default one which we do not care about.
        return steamAudioManager.hrtfNames.Length - 1;
    }
    
    // Called when we want to begin a new render
    private void ResetAudioSources()
    {
        for (int i = 0; i < audioSources.Length; i++)
        {
            audioSources[i].Stop();
            audioSources[i].time = 0.0f;
            audioSources[i].Play();
        }
    }

    public bool IsLastSpeaker()
    {
        return activeSpeaker == speakers.Count - 1;
    }

    private void UpdateSpeakerAndPlay()
    {
        activeSpeaker++;
        PlayAudio();
    }

    // Used for visualising the time on the view
    public string TimeLeft()
    {
        return timer.GetTimeLeft().ToString();
    }

    public string TimeLeftOfSimulation()
    {
        return timer.GetTimeLeftOfSimulation().ToString();
    }

    public bool IsTiming()
    {
        return timer.IsActive();
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
        return steamAudioSources[0].applyHRTFToReflections;
    }

    public void SetHRTFReflectionStatus(bool value)
    {
        steamAudioSources[0].applyHRTFToReflections = value;
    }

    public void PlayAudio()
    {
        audioSources[activeSpeaker].Play();
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
        audioSources[activeSpeaker].time = 0.0f;
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
}