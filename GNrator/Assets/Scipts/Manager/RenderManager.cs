using System;
using System.Collections.Generic;
using SteamAudio;
using UnityEngine;

public enum RenderMethod
{
   FarField,       // Renders the far field audio source
   NearField,      // Render the near field audio source
   FullRender      // Render near and far field audio sources 
}

public class RenderManager : MonoBehaviour
{
    public static RenderManager Instance { get; private set; } // singleton instance
    private readonly List <IRenderObserver> observers = new();
    public RenderMethod SelectedRenderMethod { get; private set; }

    public SourceViewModel sourceVM;    // source tasks
    public DataViewModel dataVM;        // write tasks
    private Recorder recorder;
    private Timer timer;

    public bool isLastUserIndex = false;
    private int _renderCounter = 0, _nearFieldCounter = 0, _fullRenderAmount = 0, _amountOfRenders = 0;

    public bool IsRendering { get; private set; }
    public int AmountOfRenders { get { return _amountOfRenders; } set { _amountOfRenders = value + 1; }} // + 1 because an index operator is used that can be 0
    public int RenderCounter { get { return _renderCounter; }}
    public float RenderDuration { get { return 6.0f; } private set { RenderDuration = value; }}
    public int FullRenderAmount { get { return _fullRenderAmount; }}
    public bool IsLastFarFieldRender { get { return _renderCounter == AmountOfRenders; }}
    public bool IsTiming { get { return timer.IsActive; }}
    public float CurrentTimeLeft { get { return timer.CurrentDuration; }}
    public float InRoomTimeLeft { get { return timer.InRoomDuration; }}
    public float TotalProgress { get { return timer.TotalDuration; }}
    public string SelectorIndex { get { return sourceVM.GetSelectorIndex(); }}
    public float Progress { get { return (timer.DurationContiner - TotalProgress) / timer.DurationContiner; }}


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
        List<Source> farFieldSources = FindFarFieldSources();
        Source nearFieldSource = FindNearFieldSource();
        Dimensions.defaultSourceLocation = farFieldSources[0].audioSource.transform.localPosition;
        
        // construct view models
        sourceVM = new(farFieldSources: farFieldSources, nearFieldSource: nearFieldSource, receiver: receiver);
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
        
        if (SelectedRenderMethod != RenderMethod.NearField)
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
            case RenderMethod.FarField:
                sourceVM.RandomiseSourceParameters();
                sourceVM.RandomiseSourcePosition();
                dataVM.CreateFarFieldRenderFolder(_renderCounter);
                
                SteamAudioManager.Singleton.currentHRTF++;
                sourceVM.PlayFarField();
                break;
                
            case RenderMethod.NearField:
                sourceVM.RandomiseNearFieldParameters();
                dataVM.CreateNearFieldRenderFolder();
                isLastUserIndex = false;                    
                SteamAudioManager.Singleton.currentHRTF = dataVM.FetchUserSOFAIndex(index: _nearFieldCounter);
                sourceVM.PlayNearField();
                break;

            case RenderMethod.FullRender:
                sourceVM.RandomiseSourceParameters();
                sourceVM.RandomiseSourcePosition();
                dataVM.CreateFarFieldRenderFolder(_renderCounter);
                _fullRenderAmount = 1 + AmountOfRenders;
                
                SteamAudioManager.Singleton.currentHRTF++;
                sourceVM.PlayFarField();
                break;

            default: break;
        }

        recorder.ToggleRecording();
        timer.SetTo(RenderDuration, AmountOfRenders);
    }

    // Updates the state to continue rendering with the next HRTF in the list
    public void ContinueRender()
    {
        recorder.ToggleRecording(); // stop previous recording

        switch(SelectedRenderMethod)
        {
            case RenderMethod.FarField: case RenderMethod.FullRender:
                SteamAudioManager.Singleton.currentHRTF++;
                sourceVM.PlayFarField();
                break;

            case RenderMethod.NearField:
                SteamAudioManager.Singleton.currentHRTF = dataVM.FetchUserSOFAIndex(index: _nearFieldCounter);
                // isLastUserIndex = true;
                sourceVM.PlayNearField();
                break;

            default: break;
        }

        recorder.ToggleRecording();
        timer.SetTo(RenderDuration, AmountOfRenders);
    }

    // Stop rendering process
    public void StopRender()
    {
        // IsRendering = false;
        timer.Stop();
        recorder.StopRecording();

        Logger.LogTitle();

        sourceVM.SetDefaultSourcePosition();
        sourceVM.SetDefaulReceiverPosition();

        switch (SelectedRenderMethod)
        {
            case RenderMethod.FarField: case RenderMethod.FullRender:
            {
                sourceVM.CalculateGeometry();
                SteamAudioManager.Singleton.currentHRTF = 0;
                Logger.Log(source: sourceVM.FarFieldSource);
                break;
            }

            case RenderMethod.NearField:
            {
                timer.ResetProgress();
                SteamAudioManager.Singleton.currentHRTF = 0;
                Logger.Log(source: sourceVM.NearFieldSource);
                _nearFieldCounter = 0;
                IsRendering = false;
                break;
            }

            default: break;
        }
    }

    // will be called everytime the timer ends a render duration
    private void HandleRenderProgress()
    {
        switch (SelectedRenderMethod)
        {
            case RenderMethod.FarField: case RenderMethod.FullRender:
            {
                HandleFarField();
                break;
            }

            case RenderMethod.NearField:
            {
                HandleNearField();
                break;
            }

            default: break;
        }

        NotifyObservers(); // update view
    }

    private void HandleFarField()
    {
        if (!SteamAudioManager.Singleton.IsLastSOFA())
        {
            ContinueRender();
        }
        else
        {
            StopRender();

            HandleFarFieldCompletion();
        }
    }

    // used when far field renders are complete
    private void HandleFarFieldCompletion()
    {
        _renderCounter++;
        
        if (!IsLastFarFieldRender)
        {
            dataVM.UpdateRenderPath();
            StartRender();
        }
        else if (IsLastFarFieldRender)
        {
            if (SelectedRenderMethod == RenderMethod.FullRender)
            {
                
                SelectedRenderMethod = RenderMethod.NearField;
                StartRender();
                _fullRenderAmount = 1;
            }
            _renderCounter = 0;
        }
    }

    // used to render near field source
    private void HandleNearField()
    {
        _nearFieldCounter++;
        if (_nearFieldCounter != 5)
        {
            
            ContinueRender();
        } 
        else if (_nearFieldCounter == 5)
        {
            StopRender();
            NotifyCompletion();
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

    // Observer pattern methods
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
        observers.ForEach(observer => observer.OnNotify());
    }

    private void NotifyCompletion()
    {
        observers.ForEach(observer => observer.RenderComplete());
    }

    public List<Source> FindFarFieldSources() {
        // Fetch audio objects in scene
        AudioSource[] audioSources = FindObjectsOfType<AudioSource>();
        List<Source> sourcesInScene = new();

        // iterate source and pair them in a speaker model
        foreach (var audioSource in audioSources)
        {
            // skip the near field source
            if (audioSource.CompareTag("nearField")) { continue; }

            // retrieve the steamAudioSource associated with the respective audioSource object
            SteamAudioSource steamSource = audioSource.gameObject.GetComponent<SteamAudioSource>();
            Source farFieldSource = new(audioSource, steamSource);
            // load default clip
            farFieldSource.audioSource.clip = Resources.Load<AudioClip>("Audio/" + Paths.defaultClipName);
            sourcesInScene.Add(farFieldSource);
        }
        
        // sort speakers by name
        sourcesInScene.Sort((source1, source2) => source1.name.CompareTo(source2.name));
        return sourcesInScene;
    }

    public Source FindNearFieldSource()
    {
        GameObject gameObject = GameObject.FindGameObjectWithTag("nearField");
        AudioSource nearFieldAudioSource = gameObject.GetComponent<AudioSource>();
        SteamAudioSource nearFieldSteamSource = gameObject.GetComponent<SteamAudioSource>();
        Source nearFieldSource = new(audioSource: nearFieldAudioSource, steamAudioSource: nearFieldSteamSource);
        
        return nearFieldSource;   
    }

    public string FetchRenderProgress()
    {
        return SelectedRenderMethod switch
        {
            RenderMethod.FarField => (AmountOfRenders - RenderCounter).ToString(),
            RenderMethod.NearField => "1",
            RenderMethod.FullRender => (FullRenderAmount - RenderCounter).ToString(),
            _ => "1",
        };
    }
}