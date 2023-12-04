using System;
using System.Collections.Generic;
using SteamAudio;
using UnityEngine;

public enum RenderMethod
{
   RenderRooms,     // Render all sources together in a room with every individual SOFA files, and different parameters
   RenderUser       // Render the near field audio source
}
public class RenderManager : MonoBehaviour
{
    public static RenderManager Instance { get; private set; } // singleton
    private readonly List <IRenderObserver> observers = new();

    public SourceViewModel sourceVM;
    public DataViewModel dataVM;
    private Recorder recorder;
    private Timer timer;
    public RenderMethod SelectedRenderMethod { get; private set; }

    public bool isLastUserIndex = false;
    private int renderCounter = 0, _amountOfRenders = 0;

    public bool IsRendering { get; private set; }
    public bool IsLastFarFieldRender { get { return renderCounter + 1 == AmountOfRenders; }}
    public float RenderDuration { get { return 6.0f; } private set { RenderDuration = value; } }
    public bool IsTiming { get { return timer.IsActive; } }
    public string CurrentTimeLeft { get { return timer.CurrentTime; } }                                   
    public string TotalTimeLeft { get { return timer.TotalTime; } }
    public string SelectorIndex { get { return sourceVM.GetSelectorIndex(); }}
    public int AmountOfRenders { get { return _amountOfRenders; } set { _amountOfRenders = value + 1; }} // + 1 because index can be 0

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

        SetContent();
    }

    private void SetContent()
    {
        // create behavior services
        recorder = new Recorder(outputSampleRate: UnityEngine.AudioSettings.outputSampleRate);
        timer = gameObject.AddComponent<Timer>();

        // find and save default location of our receiver in scene
        AudioListener receiver = FindObjectOfType<AudioListener>();
        Dimensions.defaultReceiverLocation = receiver.transform.localPosition;
        
        // initialise speaker list & near field object
        List<Speaker> farFieldSpeakers = FindFarFieldSpeakers();
        Speaker nearFieldSpeaker = FindNearFieldSpeaker();
        Dimensions.defaultSourceLocation = farFieldSpeakers[0].audioSource.transform.localPosition;
        
        // construct view models
        sourceVM = new(speakers: farFieldSpeakers, nearFieldSource: nearFieldSpeaker, receiver: receiver);
        dataVM = new();
        
        // fetch the previously selected render method on start up
        SelectedRenderMethod = (RenderMethod)Enum.Parse(typeof(RenderMethod), SettingsManager.Instance.settings.selectedRenderMethod);

        // create folder if they don't exist
        Paths.SetupDirectories();
    }

    // callback method issued when the timer ends
    public void HandleTimerCompletion()
    {
        Timer.OnTimerEnded += HandleRenderProgress;
    }

    // - Render Methods
    public void SetupRender()
    {
        if (AmountOfRenders == 0)
        {
            AmountOfRenders = 1;
        }
        
        if (SelectedRenderMethod == RenderMethod.RenderRooms)
        {
            dataVM.CreateRootRenderFolder();
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
            SetupRender();
            StartRender();
            HandleTimerCompletion();
        }
    }
    
    // Start processing the first render
    public void StartRender()
    {
        IsRendering = true;
        sourceVM.RandomiseReceiverPosition();

        switch(SelectedRenderMethod)
        {
            case RenderMethod.RenderRooms:
                dataVM.CreateFarFieldRenderFolder(renderCounter);
                sourceVM.RandomiseSourcePosition();
                
                SteamAudioManager.Singleton.currentHRTF++;
                sourceVM.PlayFarField();
                break;
                
            case RenderMethod.RenderUser:
                dataVM.CreateNearFieldRenderFolder();
                
                isLastUserIndex = false;                                    
                SteamAudioManager.Singleton.currentHRTF = dataVM.FetchUserSOFAIndex(index: 0);
                sourceVM.PlayNearField();
                break;

            default: break;
        }

        recorder.ToggleRecording();
        timer.Begin(RenderDuration);  
    }

    // Updates the state to continue rendering with the next HRTF in the list
    public void ContinueRender()
    {
        recorder.ToggleRecording();

        switch(SelectedRenderMethod)
        {
            case RenderMethod.RenderRooms:
                SteamAudioManager.Singleton.currentHRTF++;
                sourceVM.PlayFarField();
                break;

            case RenderMethod.RenderUser:
                SteamAudioManager.Singleton.currentHRTF = dataVM.FetchUserSOFAIndex(index: 1);
                isLastUserIndex = true;
                sourceVM.PlayNearField();
                break;

            default: break;
        }

        recorder.ToggleRecording();
        timer.Begin(RenderDuration);
    }

    // Stop rendering process
    public void StopRender()
    {
        IsRendering = false;
        timer.Stop();
        recorder.StopRecording();

        Logger.LogTitle();

        sourceVM.SetDefaultSourcePosition();
        sourceVM.SetDefaulReceiverPosition();

        switch (SelectedRenderMethod)
        {
            case RenderMethod.RenderRooms:
            {
                sourceVM.CalculateGeometry();
                SteamAudioManager.Singleton.currentHRTF = 0;       
                Logger.Log(speaker: sourceVM.Speaker);
                break;
            }

            case RenderMethod.RenderUser:
            {
                SteamAudioManager.Singleton.currentHRTF = 0;
                Logger.Log(speaker: sourceVM.nearFieldSource);
                break;
            }

            default: break;
        }
    }

    private void HandleRenderProgress()
    {
        switch (SelectedRenderMethod)
        {
            case RenderMethod.RenderRooms:
            {
                HandleRenderRooms();
                break;
            }

            case RenderMethod.RenderUser:
            {
                HandleRenderUser();
                break;
            }

            default: break;
        }

        NotifyObservers();
    }

    private void HandleRenderRooms()
    {
        if (!SteamAudioManager.Singleton.IsLastSOFA())
        {
            ContinueRender();
        }
        else
        {
            StopRender();

            HandleCompletionOfRoomsRenders();
        }
    }

    // used when far field renders are complete
    private void HandleCompletionOfRoomsRenders()
    {
        renderCounter++;
        
        if (!IsLastFarFieldRender)
        {
            // sourceVM.UpdateSelectedSpeaker();
            dataVM.UpdateRenderPath();
            StartRender();
        }
        else if (IsLastFarFieldRender)
        {
            renderCounter = 0;
            SelectedRenderMethod = RenderMethod.RenderUser;
            StartRender();
        }
    }

    // used to render near field source
    private void HandleRenderUser()
    {
        if (!isLastUserIndex)
        {
            ContinueRender();
        } 
        else if (isLastUserIndex)
        {
            StopRender();
        }
    }

    public void SetRenderMethod(RenderMethod renderMethod)
    {
        SelectedRenderMethod = renderMethod;
        SettingsManager.Instance.settings.selectedRenderMethod = renderMethod.ToString();
        SettingsManager.Instance.Save();
    }

    // Used by the AudioCapturer class in conjunction with OnAudioFilterRead() which is a MonoBehavior class that needs an AudioSource.
    // The RenderManager class is added to the receiver object in the scenes.
    // This method binds the Recorder class together with the Audio.
    public void TransmitData(float[] data)
    {
        if (recorder != null && recorder.IsRecording && IsRendering) {
            recorder.ConvertAndWrite(data);
        }
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
            // skip the near field source
            if (audioSource.CompareTag("nearField")) { continue; }

            // retrieve the steamAudioSource associated with the respective audioSource object
            SteamAudioSource steamSource = audioSource.gameObject.GetComponent<SteamAudioSource>();
            Speaker speaker = new(audioSource, steamSource);
            // load default clip
            speaker.audioSource.clip = Resources.Load<AudioClip>("Audio/" + Paths.defaultClipName);
            foundSpeakers.Add(speaker);
        }
        
        // sort speakers by name
        foundSpeakers.Sort((speaker1, speaker2) => speaker1.name.CompareTo(speaker2.name));
        return foundSpeakers;
    }

    public Speaker FindNearFieldSpeaker()
    {
        GameObject gameObject = GameObject.FindGameObjectWithTag("nearField");
        AudioSource nearFieldAudioSource = gameObject.GetComponent<AudioSource>();
        SteamAudioSource nearFieldSteamSource = gameObject.GetComponent<SteamAudioSource>();
        Speaker nearFieldSpeaker = new(audioSource: nearFieldAudioSource, steamAudioSource: nearFieldSteamSource);
        
        return nearFieldSpeaker;   
    }
}