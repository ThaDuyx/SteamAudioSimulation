using SteamAudio;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class SimulationManager : MonoBehaviour
{
    // Singleton object
    public static SimulationManager Instance { get; private set; }

    [SerializeField]
    private AudioListener mainListener;
    private AudioSource[] audioSources;
    private SteamAudioManager steamAudioManager;
    private SteamAudioSource[] steamAudioSources;
    private Recorder recorder;
    private Timer timer;

    private bool _isRendering;
    private readonly float simulationLength = 8.0f;

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
        SteamAudioManager[] steamAudioManagers = FindObjectsOfType<SteamAudioManager>();
        steamAudioManager = steamAudioManagers[0];

        steamAudioSources = FindObjectsOfType<SteamAudioSource>();
    }

    // Basic Unity MonoBehaviour method - Essentially a start-up function / Constructor of the class
    void Start()
    {   
        recorder = new Recorder(UnityEngine.AudioSettings.outputSampleRate);
        timer = gameObject.AddComponent<Timer>();
        audioSources = FindObjectsOfType<AudioSource>();
    }

    // Start rendering process
    public void StartRender()
    {
        _isRendering = true;
        ResetAudioSources();
        UpdateHRTF();
        recorder.ToggleRecording();
        timer.Begin(simulationLength);
    }

    // Updates the state to continue rendering with the next HRTF in the list
    public void ContinueRender()
    {
        ResetAudioSources();

        recorder.ToggleRecording();     // Stop previous recording
        UpdateHRTF();                   // Select next HRTF
        recorder.ToggleRecording();     // Start recording the next

        timer.Begin(simulationLength);
    }

    // Stop rendering process
    public void StopRender()
    {
        _isRendering = false;
        steamAudioManager.currentHRTF = 0;
        timer.Stop();
        recorder.StopRecording();
        
    }

    // Used by the AudioCapturer class in conjunction with OnAudioFilterRead() which is a MonoBehavior class that needs an AudioSource.
    // This method binds the Recorder class together with the Audio.
    public void TransmitData(float[] data)
    {
        if (recorder != null && recorder.IsRecording() && _isRendering) {
            recorder.ConvertAndWrite(data);
        }
    }

    public bool IsRendering()
    {
        return _isRendering;
    }

    public string CurrentHRTFName()
    {
        return steamAudioManager.hrtfNames[steamAudioManager.currentHRTF]; // SOFA file name
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
}