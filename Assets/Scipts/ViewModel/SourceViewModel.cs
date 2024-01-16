using System.Collections.Generic;
using UnityEngine;

public class SourceViewModel
{
    public Source FarFieldSource { get { return farFieldSources[selectedSource]; }}
    public List<Source> farFieldSources;
    public Source NearFieldSource;
    public AudioListener receiver;
    private readonly RoomData room;
    private int selectedSource = 0;
    private bool isAllSourcesSelected;

    public int AmountOfSources { get { return farFieldSources.Count; }}
    public int SourceCount { get { return farFieldSources.Count; }}
    public bool IsLastRoom { get { return selectedSource + 1 == farFieldSources.Count; }}
    public string GetSelectorIndex() { return selectedSource.ToString(); }
    
    public SourceViewModel(List<Source> farFieldSources, Source nearFieldSource, AudioListener receiver)
    {
        this.farFieldSources = farFieldSources;
        this.NearFieldSource = nearFieldSource;
        this.receiver = receiver;

        // Try to load a persisted room or else fetch a default one.
        room = DataManager.Instance.LoadRoomData(amountOfSources: farFieldSources.Count);

        // Load json data into our source array - TODO: could be done more clean by applying the json data directly into the object insted
        for (int i = 0; i < farFieldSources.Count; i++)
        {
            farFieldSources[i].audioSource.volume = room.sources[i].volume;
            farFieldSources[i].steamAudioSource.directMixLevel = room.sources[i].directMixLevel;
            farFieldSources[i].steamAudioSource.reflectionsMixLevel = room.sources[i].reflectionMixLevel;
            farFieldSources[i].audioSource.clip = Resources.Load<AudioClip>("Audio/" + room.sources[i].audioClip);
            farFieldSources[i].steamAudioSource.applyHRTFToReflections = room.sources[i].applyHRTFToReflections == 1;
            farFieldSources[i].steamAudioSource.airAbsorption = room.sources[i].airAbsorption == 1;
        }
    }

    public bool ApplyHRTFToReflections
    {
        get { return FarFieldSource.steamAudioSource.applyHRTFToReflections; }
        set { FarFieldSource.steamAudioSource.applyHRTFToReflections = value; }
    }
    public float Volume
    {
        get { return FarFieldSource.audioSource.volume; }
        set {
                if (isAllSourcesSelected) 
                {
                    farFieldSources.ForEach(source => source.audioSource.volume = value);
                }
                else 
                {
                    FarFieldSource.audioSource.volume = value; 
                }
            }
    }

    public float DirectMixLevel
    {
        get { return FarFieldSource.steamAudioSource.directMixLevel; }
        set 
        { 
            if (isAllSourcesSelected) 
            {
                farFieldSources.ForEach(source => source.steamAudioSource.directMixLevel = value);
            }
            else 
            {
                FarFieldSource.steamAudioSource.directMixLevel = value; 
            }
        }
    }

    public float ReflectionMixLevel
    {
        get { return FarFieldSource.steamAudioSource.reflectionsMixLevel; }
        set 
        { 
            if (isAllSourcesSelected) 
            {
                farFieldSources.ForEach(source => source.steamAudioSource.reflectionsMixLevel = value);
            }
            else 
            {
                FarFieldSource.steamAudioSource.reflectionsMixLevel = value; 
            }
        }
    }

    public string AudioClip { 
        get { return FarFieldSource.audioSource.clip.name; } 
        set 
        { 
            // Deletes the 4 last characters of the string meaning either '.wav' or '.mp3'. Unity does not use the file type when searching in the library.
            string audioClipWithoutFileType = value[..^4];

            // Replace the audio clip with the new one
            FarFieldSource.audioSource.clip = Resources.Load<AudioClip>("Audio/" + audioClipWithoutFileType);
        }
    }

    public bool AirAbsorption
    {
        get { return FarFieldSource.steamAudioSource.airAbsorption; }
        set 
        {
            if (isAllSourcesSelected)
            {
                farFieldSources.ForEach(source => source.steamAudioSource.airAbsorption = value);
            }
            else 
            {
                FarFieldSource.steamAudioSource.airAbsorption = value;
            }
        }
    }

    public bool DistanceAttenuation
    {
        get { return FarFieldSource.steamAudioSource.distanceAttenuation; }
        set 
        { 
            if (isAllSourcesSelected)
            {
                farFieldSources.ForEach(source => source.steamAudioSource.distanceAttenuation = value);
            }
            else 
            {
                FarFieldSource.steamAudioSource.distanceAttenuation = value; 
            }
        }
    }

    public void PersistRoom()
    {
        // if the all sources option were selected we will iterate over all sources and save it in json format
        if (isAllSourcesSelected) 
        {
            for (int i = 0; i < farFieldSources.Count; i++)
            {
                SetRoomVariables(index: i);
                DataManager.Instance.SaveRoomData(room: room);
            }
        }
        else 
        {
            SetRoomVariables(index: selectedSource);
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

    public List<string> GetSourceNames()
    {
        List<string> names = new();
        farFieldSources.ForEach(source => names.Add(source.name));
        
        return names;
    }

    public void SetSelectedSource(int index)
    {
        // If index is the same as the .Count of sources it means the all sources option were selected
        if (index == farFieldSources.Count )
        {
            isAllSourcesSelected = true;
        }
        else 
        {
            isAllSourcesSelected = false;
            selectedSource = index;

            FarFieldSource.audioSource.volume = room.sources[selectedSource].volume;
            FarFieldSource.steamAudioSource.directMixLevel = room.sources[selectedSource].directMixLevel;
            FarFieldSource.steamAudioSource.reflectionsMixLevel = room.sources[selectedSource].reflectionMixLevel;
            FarFieldSource.audioSource.clip = Resources.Load<AudioClip>("Audio/" + room.sources[selectedSource].audioClip);
            FarFieldSource.steamAudioSource.applyHRTFToReflections = room.sources[selectedSource].applyHRTFToReflections == 1;
        }
    }

    public void UpdateSelectedSource()
    {
        selectedSource++;   
    }

    public void ToggleAudio()
    {
        if (FarFieldSource.audioSource.isPlaying)
        {
            StopAudio();
        }
        else 
        {
            PlayFarField();
        }
    }

    public void PlayFarField()
    {
        FarFieldSource.audioSource.Play();
    }

    public void StopAudio()
    {
        FarFieldSource.audioSource.Stop();
    }

    public void PlayNearField()
    {
        NearFieldSource.audioSource.Play();
    }

    public bool AudioClipIsTheSameAs(string audioClip)
    {
        if (audioClip == AudioClip + ".wav" || audioClip == AudioClip + ".mp3")
        {
            return true;
        }

        return false;
    }

    public void CalculateGeometry()
    {
        FarFieldSource.distanceToReceiver = Calculator.CalculateDistanceToReceiver(receiver.transform, FarFieldSource.audioSource.transform);
        FarFieldSource.azimuth = Calculator.CalculateAzimuth(receiver.transform, FarFieldSource.audioSource.transform);
        FarFieldSource.elevation = Calculator.CalculateElevation(receiver.transform, FarFieldSource.audioSource.transform);
    }

    public void SetDefaulReceiverPosition()
    {
        receiver.transform.localPosition = Dimensions.defaultReceiverLocation;
    }
    public void RandomiseReceiverPosition()
    {
        receiver.transform.localPosition = Calculator.CalculateNewPosition();

    }
    public void SetDefaultSourcePosition()
    {
        FarFieldSource.audioSource.gameObject.transform.position = Dimensions.defaultSourceLocation;
    }

    public void RandomiseSourcePosition()
    {
        FarFieldSource.audioSource.gameObject.transform.position = Calculator.CalculateNewPosition();
    }

    public void RandomiseSourceParameters()
    {
        FarFieldSource.audioSource.volume = Calculator.RandomiseSourceParameters(Constants.lVolumeThreshold, Constants.uVolumeThreshold);
        FarFieldSource.steamAudioSource.directMixLevel = Calculator.RandomiseSourceParameters(Constants.lDirectLevelMixThreshold, Constants.uDirectLevelMixThreshold);
        FarFieldSource.steamAudioSource.reflectionsMixLevel = Calculator.RandomiseSourceParameters(Constants.lReflectionLevelMixThreshold, Constants.uReflectionLevelMixThreshold);
    }

    public void RandomiseNearFieldParameters()
    {
        NearFieldSource.audioSource.volume = Calculator.RandomiseSourceParameters(Constants.lVolumeThreshold, Constants.uVolumeThreshold);
        NearFieldSource.steamAudioSource.directMixLevel = Calculator.RandomiseSourceParameters(Constants.lDirectLevelMixThreshold, Constants.uDirectLevelMixThreshold);
        NearFieldSource.steamAudioSource.reflectionsMixLevel = Calculator.RandomiseSourceParameters(Constants.lNearFieldReflectionLevelMixThreshold, Constants.uNearFieldReflectionLevelMixThreshold);
    }
}