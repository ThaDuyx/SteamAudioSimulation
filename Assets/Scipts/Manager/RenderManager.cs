using System;
using System.Collections.Generic;
using System.IO;
using SteamAudio;
using UnityEngine;

public enum RenderMethod
{
   AllAtOnce,       // Render all sources together in a room with every individual SOFA files
   RenderRooms,     // Render all sources together in a room with every individual SOFA files, and different parameters
   RenderUser       // Render the near field audio source
}
public class RenderManager : MonoBehaviour
{
    public static RenderManager Instance { get; private set; } // singleton
    private readonly List <IRenderObserver> observers = new();
    
    public SourceViewModel sourceVM;
    public DataViewModel dataVM;
    private AudioListener receiver;
    private Recorder recorder;
    private Timer timer;
    public RenderMethod SelectedRenderMethod { get; private set; }

    public int SampleRate { get { return UnityEngine.AudioSettings.outputSampleRate; } }
    public bool IsRendering { get; private set; }
    public float SimulationLength { get { return 6.0f; } private set { SimulationLength = value; } }
    public bool IsTiming { get { return timer.IsActive(); } }
    public string TimeLeft { get { return timer.GetTimeLeft().ToString(); } }                                   
    public string TimeLeftOfRender { get { return timer.GetTimeLeftOfSimulation().ToString(); } }
    public string SelectorIndex { get { return sourceVM.GetSelectorIndex(); }}
    public bool IsLastUserIndex = false;

    // Integer used for tracking which speaker should play during the OneByOne render method & for selecting speakers and configuring their parameters
    private readonly int selectedUserIndex = 0;
    public bool didStartUpComplete = false;

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

        SetContent(); // initialise at the Awake lifecycle in order to have it ready for the view to read
    }

    // Basic Unity MonoBehaviour method - Essentially a start-up function / Constructor of the class
    void Start()
    {
        recorder = new Recorder(outputSampleRate: SampleRate);
        timer = gameObject.AddComponent<Timer>();
        dataVM = new();
        SelectedRenderMethod = (RenderMethod)Enum.Parse(typeof(RenderMethod), SettingsManager.Instance.settings.selectedRenderMethod);
    }

    private void SetContent()
    {
        // find Receiver in scene
        receiver = FindObjectOfType<AudioListener>();
        Dimensions.defaultLocation = receiver.transform.localPosition;
        
        // initialising speaker list & near field object
        List<Speaker> farFieldSpeakers = FindFarFieldSpeakers();
        Speaker nearFieldSpeaker = FindNearFieldSpeaker();
        sourceVM = new(speakers: farFieldSpeakers, nearFieldSource: nearFieldSpeaker);

        Paths.SetupDirectories();
    }

    public void Callback()
    {
        Timer.OnTimerEnded += HandleTimerCallback;
    }

    // - Render Methods
    public void SetupRender()
    {
        if (SelectedRenderMethod == RenderMethod.RenderRooms)
        {   
            dataVM.CreateNewRoomFolder();
        }   
    }

    public void ToggleRender()
    {    
        if (IsRendering)
        {
            StopRender();
        }
        else
        {
            StartRender();
            Callback();
        }
    }
    
    // Start processing the first render
    public void StartRender()
    {
        IsRendering = true;
        dataVM.SetRecordingPath(renderMethod: SelectedRenderMethod);             // updates the output for recordings

        switch(SelectedRenderMethod)
        {
            case RenderMethod.RenderRooms:
                receiver.transform.localPosition = Calculator.CalculateNewPosition(); 
                UpdateSOFA();                                                    // increments array of SOFA files to our
                sourceVM.PlayAudio();                                            // play the first speaker
                break;
                
            case RenderMethod.RenderUser:
                IsLastUserIndex = false;                                    
                // fetch indices of config sofa files that match the randomly chosen user index
                List<int> userIndicies = SteamAudioManager.Singleton.GetUserSOFAIndices(selectedUserIndex);  
                SteamAudioManager.Singleton.currentHRTF = userIndicies[0];       // select the first config of the near field sofa files
                sourceVM.nearFieldSource.audioSource.Play();
                break;

            default: break;
        }

        recorder.ToggleRecording();
        timer.Begin(SimulationLength, SelectedRenderMethod);  
    }

    // Updates the state to continue rendering with the next HRTF in the list
    public void ContinueRender()
    {
        recorder.ToggleRecording();                                                 // stop previous recording

        switch(SelectedRenderMethod)
        {
            case RenderMethod.RenderRooms:
                UpdateSOFA();                                                       // moves to next HRTF
                sourceVM.PlayAudio();                                               // play
                break;

            case RenderMethod.RenderUser:
                // fetch indices of config sofa files that match the randomly chosen user index
                List<int> userIndicies = SteamAudioManager.Singleton.GetUserSOFAIndices(selectedUserIndex);

                SteamAudioManager.Singleton.currentHRTF = userIndicies[1];          // select the second config of the near field sofa files
                IsLastUserIndex = true;                                             // change flag notifiying that this is the final config file
                sourceVM.nearFieldSource.audioSource.Play();                        // play near field source
                break;

            default: break;
        }

        recorder.ToggleRecording();                                                 // start new recording
        timer.Begin(SimulationLength, SelectedRenderMethod);                        // start new timer
    }

    // Stop rendering process
    public void StopRender()
    {
        IsRendering = false;
        timer.Stop();
        recorder.StopRecording();
        Logger.LogTitle();

        switch (SelectedRenderMethod)
        {
            case RenderMethod.AllAtOnce: case RenderMethod.RenderRooms:
                CalculateGeometry();
                SteamAudioManager.Singleton.currentHRTF = 0;       
                Logger.Log(speaker: sourceVM.Speaker);
                break;

            case RenderMethod.RenderUser:
                SteamAudioManager.Singleton.currentHRTF = 0;
                Logger.Log(speaker: sourceVM.nearFieldSource);
                break;
            
            default: break;
        }
    }

    private void HandleTimerCallback()
    {
        switch (SelectedRenderMethod)
        {
            case RenderMethod.RenderRooms:
            {
                if (!SteamAudioManager.Singleton.IsLastSOFA())
                {
                    ContinueRender();
                }
                else
                {
                    StopRender();

                    if (!sourceVM.IsLastRoom)
                    {
                        sourceVM.UpdateSelectedSpeaker();
                        dataVM.UpdateRenderPath();
                        receiver.transform.localPosition = Dimensions.defaultLocation;
                        StartRender();
                    }
                    else if (sourceVM.IsLastRoom)
                    {
                        SelectedRenderMethod = RenderMethod.RenderUser;
                        StartRender();
                    }
                }
                break;
            }

            case RenderMethod.RenderUser:
            {
                if (!IsLastUserIndex)
                {
                    ContinueRender();
                } 
                else if (IsLastUserIndex)
                {
                    StopRender();
                }
                break;
            }

            case RenderMethod.AllAtOnce:
            {
                if (!SteamAudioManager.Singleton.IsLastSOFA())
                {
                    ContinueRender();        
                }
                else if (SteamAudioManager.Singleton.IsLastSOFA())
                {
                    StopRender();
                }
                break;
            }

            default: break;
        }

        NotifyObservers();
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
        SteamAudioManager.Singleton.currentHRTF++;
    }

    public void ToggleStartUp()
    {
        didStartUpComplete = !didStartUpComplete;
    }

    public void SetRenderMethod(RenderMethod renderMethod)
    {
        SelectedRenderMethod = renderMethod;
        SettingsManager.Instance.settings.selectedRenderMethod = renderMethod.ToString();
        SettingsManager.Instance.Save();
    }

    private void CalculateGeometry()
    {
        // Calculating geometry
        sourceVM.Speaker.DistanceToReceiver = Calculator.CalculateDistanceToReceiver(receiver.transform, sourceVM.Speaker.audioSource.transform);
        sourceVM.Speaker.Azimuth = Calculator.CalculateAzimuth(receiver.transform, sourceVM.Speaker.audioSource.transform);
        sourceVM.Speaker.Elevation = Calculator.CalculateElevation(receiver.transform, sourceVM.Speaker.audioSource.transform);
    }

    public void AddObserver(IRenderObserver observer)
    {
        if (!observers.Contains(observer))
        {
            observers.Add(observer);
        }
    }

    public void RemoveObserver(IRenderObserver observer)
    {
        observers.Remove(observer);
    }

    private void NotifyObservers()
    {
        foreach (var observer in observers)
        {
            observer.OnNotify();
        }
    }

    public List<Speaker> FindFarFieldSpeakers() {
        // Fetch audio objects in scene
        AudioSource[] audioSources = FindObjectsOfType<AudioSource>();
        List<Speaker> foundSpeakers = new();

        // iterate source and pair them in a speaker model
        foreach (var audioSource in audioSources)
        {
            if (audioSource.CompareTag("nearField")) { continue; }

            // retrieve the steamAudioSource associated with the respective audioSource object
            SteamAudioSource steamSource = audioSource.gameObject.GetComponent<SteamAudioSource>();
            Speaker speaker = new(audioSource, steamSource);
            // load default clip
            speaker.audioSource.clip = Resources.Load<AudioClip>("Audio/" + Paths.defaultClipName);
            foundSpeakers.Add(speaker);
        }
        
        // sort speakers by name
        foundSpeakers.Sort((speaker1, speaker2) => speaker1.Name.CompareTo(speaker2.Name));
        return foundSpeakers;
    }

    public Speaker FindNearFieldSpeaker()
    {
        GameObject gameObject = GameObject.FindGameObjectWithTag("nearField");
        AudioSource nearFieldAudioSource = gameObject.GetComponent<AudioSource>();
        SteamAudioSource nearFieldSteamSource = gameObject.GetComponent<SteamAudioSource>();
        Speaker nearFieldSpeaker = new(source: nearFieldAudioSource, steamSource: nearFieldSteamSource);
        
        return nearFieldSpeaker;   
    }
}