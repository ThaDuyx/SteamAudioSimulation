using System;
using System.Collections.Generic;
using System.IO;
using SteamAudio;
using UnityEngine;

public enum RenderMethod
{
   OneByOne,    // Render all sources in a room with the same SOFA file
   AllAtOnce,   // Render all sources together in a room with every individual SOFA files
   RenderRooms  // Render all sources together in a room with every individual SOFA files, and different parameters
}
public class RenderManager : MonoBehaviour
{
    // Singleton object
    public static RenderManager Instance { get; private set; }

    private AudioListener receiver;
    private UnityEngine.Vector3 defaultLocation;
    private List<Speaker> speakers;
    private Recorder recorder;
    private Room room;
    private Settings settings;
    private Timer timer;
    private Logger logger;
    private Calculator calculator;
    
    // Integer used for tracking which speaker should play during the OneByOne render method & for selecting speakers and configuring their parameters
    private int activeSpeaker = 0, selectedSpeaker = 0, amountOfRooms = 0, activeRoom = 0;
    private bool isAllSpeakersSelected = false, didStartUpComplete = false;

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
    public string[] Directories { get { return Directory.GetDirectories(roomsPath); }}
    public string[] RenderPaths { get { return Directory.GetDirectories(SelectedRoomPath); }}
    public string SelectedRoomPath { get; private set; }
    public string SelectedRenderPath { get; private set; } 
    public RenderMethod SelectedRenderMethod { get; private set; }
    
    // Should be modified for specific needs - TODO: Change to dynamic folder structure
    public static string folderPath = "/Users/duyx/Code/Jabra/python/renders/";
    public static string roomsPath = "/Users/duyx/Code/Jabra/python/renders/rooms/";
    public string currentRenderPath = "/Users/duyx/Code/Jabra/python/renders/rooms/render0/";
    public static string defaultClipName = "sweep_48kHz";
    public string recordingPath;

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

        // Load json data into our speaker array - TODO: could be done more clean by applying the json data directly into the object insted
        for (int i = 0; i < speakers.Count; i++)
        {
            speakers[i].audioSource.volume = room.sources[i].volume;
            speakers[i].steamAudioSource.directMixLevel = room.sources[i].directMixLevel;
            speakers[i].steamAudioSource.reflectionsMixLevel = room.sources[i].reflectionMixLevel;
            speakers[i].audioSource.clip = Resources.Load<AudioClip>("Audio/" + room.sources[i].audioClip);
            speakers[i].steamAudioSource.applyHRTFToReflections = room.sources[i].applyHRTFToReflections == 1;
            speakers[i].steamAudioSource.airAbsorption = room.sources[i].airAbsorption == 1;
        }

        if (!Directory.Exists(folderPath))
        {
            System.IO.Directory.CreateDirectory(folderPath);
        }

        if (!Directory.Exists(roomsPath))
        {
            System.IO.Directory.CreateDirectory(roomsPath);
        }

        if(!Directory.Exists(currentRenderPath))
        {
            System.IO.Directory.CreateDirectory(currentRenderPath);
        }

        settings = DataManager.Instance.LoadSettings();

        SelectedRoomPath = settings.selectedRoomDirectory;
        SelectedRenderPath = settings.selectedRenderDirectory;
        SelectedRenderMethod = (RenderMethod)Enum.Parse(typeof(RenderMethod), settings.selectedRenderMethod);
    }

    // - Render Methods
    // Start rendering process
    public void StartRender(RenderMethod renderMethod, int sofaIndex)
    {
        IsRendering = true;
        
        SetRecordingPath(renderMethod);                                 // updates the output for recordings
        
        switch(renderMethod)
        {
            case RenderMethod.AllAtOnce:
                UpdateSOFA();                                           // increments array of SOFA files to our
                RewindAndPlayAudioSources();                            // reset and play every audio source in the current scene
                break;

            case RenderMethod.OneByOne:
                SteamAudioManager.Singleton.currentHRTF = sofaIndex;    // select which SOFA file should be used in the render
                PlayActiveSpeaker();                                    // play the first speaker
                break;

            case RenderMethod.RenderRooms:
                RandomiseLocation();
                UpdateSOFA();                                           // increments array of SOFA files to our
                PlayAudio();                                            // play the first speaker
                break;
                
            default: break;
        }

        recorder.ToggleRecording();
        timer.Begin(SimulationLength, renderMethod);
    }

    // Updates the state to continue rendering with the next HRTF in the list
    public void ContinueRender(RenderMethod renderMethod)
    {
        recorder.ToggleRecording();             // stop previous recording

        switch(renderMethod)
        {
            case RenderMethod.AllAtOnce:
                UpdateSOFA();                   // moves to next HRTF
                RewindAndPlayAudioSources();    // replay the audio sources    
                break;

            case RenderMethod.OneByOne:
                SelectNextSpeaker();            // moves to next speaker  
                PlayActiveSpeaker();            // play
                break;

            case RenderMethod.RenderRooms:
                UpdateSOFA();                   // moves to next HRTF
                PlayAudio();                    // play
                // TODO: - randomize speaker, location or parameters
                break;

            default: break;
        }

        recorder.ToggleRecording();                           // start new recording
        timer.Begin(SimulationLength, renderMethod);          // start new timer
    }

    // Stop rendering process
    public void StopRender(RenderMethod renderMethod)
    {
        IsRendering = false;
        timer.Stop();
        recorder.StopRecording();
        logger.LogTitle();

        switch (renderMethod)
        {
            case RenderMethod.AllAtOnce: case RenderMethod.RenderRooms:
                SteamAudioManager.Singleton.currentHRTF = 0;       
                logger.Log(speaker: speakers[selectedSpeaker]);
                break;

            case RenderMethod.OneByOne:
                activeSpeaker = 0;
                foreach (Speaker speaker in speakers)
                {
                    logger.Log(speaker: speaker);
                }
                break;
            
            default: break;
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

    public void UpdateSelectedSpeaker()
    {
        selectedSpeaker++;   
    }

    private void SelectNextSpeaker()
    {
        activeSpeaker++;
    }

    public void PlayActiveSpeaker()
    {
        speakers[activeSpeaker].audioSource.Play();
    }

    public void PlaySelectedSpeaker()
    {
        speakers[selectedSpeaker].audioSource.Play();
    }

    public void PlayAudio()
    {
        speakers[selectedSpeaker].audioSource.Play();
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

    // Source Attributes
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
        set {
                if (isAllSpeakersSelected) 
                {
                    speakers.ForEach(speaker => speaker.audioSource.volume = value);
                }
                else 
                {
                    speakers[selectedSpeaker].audioSource.volume = value; 
                }
            }
    }

    public float DirectMixLevel
    {
        get { return speakers[selectedSpeaker].steamAudioSource.directMixLevel; }
        set 
        { 
            if (isAllSpeakersSelected) 
            {
                speakers.ForEach(speaker => speaker.steamAudioSource.directMixLevel = value);
            }
            else 
            {
                speakers[selectedSpeaker].steamAudioSource.directMixLevel = value; 
            }
        }
    }

    public float ReflectionMixLevel
    {
        get { return speakers[selectedSpeaker].steamAudioSource.reflectionsMixLevel; }
        set 
        { 
            if (isAllSpeakersSelected) 
            {
                speakers.ForEach(speaker => speaker.steamAudioSource.reflectionsMixLevel = value);
            }
            else 
            {
                speakers[selectedSpeaker].steamAudioSource.reflectionsMixLevel = value; 
            }
        }
    }

    public string AudioClip { 
        get { return speakers[selectedSpeaker].audioSource.clip.name; } 
        set 
        { 
            // Deletes the 4 last characters of the string meaning either '.wav' or '.mp3'. Unity does not use the file type when searching in the library.
            string audioClipWithoutFileType = value[..^4];

            // Replace the audio clip with the new one
            speakers[selectedSpeaker].audioSource.clip = Resources.Load<AudioClip>("Audio/" + audioClipWithoutFileType);
        }
    }

    public bool AirAbsorption
    {
        get { return speakers[selectedSpeaker].steamAudioSource.airAbsorption; }
        set 
        {
            if (isAllSpeakersSelected)
            {
                speakers.ForEach(speaker => speaker.steamAudioSource.airAbsorption = value);
            }
            else 
            {
                speakers[selectedSpeaker].steamAudioSource.airAbsorption = value;
            }
        }
    }

    public bool DistanceAttenuation
    {
        get { return speakers[selectedSpeaker].steamAudioSource.distanceAttenuation; }
        set 
        { 
            if (isAllSpeakersSelected)
            {
                speakers.ForEach(speaker => speaker.steamAudioSource.distanceAttenuation = value);
            }
            else 
            {
                speakers[selectedSpeaker].steamAudioSource.distanceAttenuation = value; 
            }
        }
    }

    public void PersistRoom()
    {
        // if the all speakers option were selected we will iterate over all speakers and save it in json format
        if (isAllSpeakersSelected) 
        {
            for (int i = 0; i < speakers.Count; i++)
            {
                SetRoomVariables(index: i);
                DataManager.Instance.SaveRoomData(room: room);
            }
        }
        else 
        {
            SetRoomVariables(index: selectedSpeaker);
            DataManager.Instance.SaveRoomData(room: room);
        }
    }

    private void SetRoomVariables(int index)
    {
        room.sources[index].volume = Volume;
        room.sources[index].directMixLevel = DirectMixLevel;
        room.sources[index].reflectionMixLevel = ReflectionMixLevel;
        room.sources[index].audioClip = AudioClip;
        room.sources[index].applyHRTFToReflections = ApplyHRTFToReflections ? 1 : 0;
        room.sources[index].airAbsorption = AirAbsorption ? 1 : 0;
        room.sources[index].distanceAttenuation = DistanceAttenuation ? 1 : 0;
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
        // If index is the same as the .Count of speakers it means the all speakers option were selected
        if (index == speakers.Count )
        {
            isAllSpeakersSelected = true;
        }
        else 
        {
            isAllSpeakersSelected = false;
            selectedSpeaker = index;

            speakers[selectedSpeaker].audioSource.volume = room.sources[selectedSpeaker].volume;
            speakers[selectedSpeaker].steamAudioSource.directMixLevel = room.sources[selectedSpeaker].directMixLevel;
            speakers[selectedSpeaker].steamAudioSource.reflectionsMixLevel = room.sources[selectedSpeaker].reflectionMixLevel;
            speakers[selectedSpeaker].audioSource.clip = Resources.Load<AudioClip>("Audio/" + room.sources[selectedSpeaker].audioClip);
            speakers[selectedSpeaker].steamAudioSource.applyHRTFToReflections = room.sources[selectedSpeaker].applyHRTFToReflections == 1;
        }
    }

    public void ToggleStartUp()
    {
        didStartUpComplete = !didStartUpComplete;
    }

    public void SetAmountOfRooms(int amount)
    {
        amountOfRooms = amount;
    }

    private void SetRecordingPath(RenderMethod renderMethod)
    {
        if (renderMethod == RenderMethod.RenderRooms)
        {
            recordingPath = SelectedRenderPath;
        }
        else 
        {
            string timeStamp = DateTime.Now.ToString("ddMM-yy_HHmmss");
            recordingPath = folderPath + timeStamp + "/";
            System.IO.Directory.CreateDirectory(recordingPath);
        }
    }

    public void SetRoomPath(int index)
    {
        SelectedRoomPath = Directories[index] + "/";
        settings.selectedRoomDirectory = SelectedRoomPath;

        if (RenderPaths.Length != 0 && didStartUpComplete)
        {
            SetRenderPath(0);
        }

        DataManager.Instance.SaveSettings(settings);
    }

    public void SetRenderPath(int index)
    {
        SelectedRenderPath = RenderPaths[index] + "/";
        settings.selectedRenderDirectory = SelectedRenderPath;

        DataManager.Instance.SaveSettings(settings);
    }

    public void CreateNewRoomFolder() 
    {
        int folderCount = Directory.GetDirectories(roomsPath).Length;
        System.IO.Directory.CreateDirectory(roomsPath + "render" + folderCount.ToString() + "/");
        SelectedRoomPath = roomsPath + "render" + folderCount.ToString();
        
        CreateFoldersForRenders();
    }

    private void CreateFoldersForRenders()
    {
        for (int i = 0; i <= speakers.Count - 1; i++)
        {
            int folderCount = Directory.GetDirectories(SelectedRoomPath).Length;
            SelectedRenderPath = SelectedRoomPath + "/" + "inroom" + folderCount.ToString() + "/";
            System.IO.Directory.CreateDirectory(SelectedRenderPath);
        }

        SelectedRenderPath = SelectedRoomPath + "/" + "inroom0" + "/";
    }

    public void CreateNewRenderFolder()
    {
        int folderCount = Directory.GetDirectories(SelectedRoomPath).Length;
        SelectedRenderPath = SelectedRoomPath + "inroom" + folderCount.ToString() + "/";
        System.IO.Directory.CreateDirectory(SelectedRenderPath);
    }

    public void UpdateRenderPath()
    {
        SelectedRenderPath = SelectedRoomPath + "/" + "inroom" + selectedSpeaker.ToString() + "/";
    }

    public void DeleteRoomFolder()
    {
        System.IO.Directory.Delete(SelectedRoomPath);
        SelectedRoomPath = Directories[0];
    }

    public void DeleteRenderFolder()
    {
        System.IO.Directory.Delete(SelectedRenderPath);
    }

    public void SetRenderMethod(RenderMethod renderMethod)
    {
        SelectedRenderMethod = renderMethod;
        settings.selectedRenderMethod = renderMethod.ToString();

        DataManager.Instance.SaveSettings(settings: settings);
    }

    public bool IsLastRoom { get { return selectedSpeaker + 1 == speakers.Count; }}


    public void SetDefaultLocation()
    {
        defaultLocation = receiver.gameObject.transform.position;
    }

    public void GoToDefaultLocation()
    {
        receiver.transform.position = defaultLocation;
    }

    public void RandomiseLocation()
    {
        UnityEngine.Vector3 newPosition = calculator.CalculateNewPosition();
        receiver.transform.position = newPosition;
    }
}