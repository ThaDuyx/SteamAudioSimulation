using System.Collections.Generic;
using UnityEngine;
using SteamAudio;

public class SourceViewModel
{
    public List<Speaker> speakers;
    public Speaker Speaker { get { return speakers[selectedSpeaker]; }}
    public Speaker nearFieldSource;
    public AudioListener receiver;
    private readonly Room room;
    private int selectedSpeaker = 0;
    private bool isAllSpeakersSelected;

    public int AmountOfSpeakers { get { return speakers.Count ;}}
    public int SpeakerCount { get { return speakers.Count; }}
    public bool IsLastRoom { get { return selectedSpeaker + 1 == speakers.Count; }}
    public string GetSelectorIndex() { return selectedSpeaker.ToString(); }
    
    public SourceViewModel(List<Speaker> speakers, Speaker nearFieldSource, AudioListener receiver)
    {
        this.speakers = speakers;
        this.nearFieldSource = nearFieldSource;
        this.receiver = receiver;

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
            Speaker.audioSource.clip = Resources.Load<AudioClip>("Audio/" + audioClipWithoutFileType);
        }
    }

    public bool AirAbsorption
    {
        get { return Speaker.steamAudioSource.airAbsorption; }
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
                Speaker.steamAudioSource.distanceAttenuation = value; 
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

    public List<string> GetSpeakerNames()
    {
        List<string> names = new();
        speakers.ForEach(speaker => names.Add(speaker.name));
        
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

            Speaker.audioSource.volume = room.sources[selectedSpeaker].volume;
            Speaker.steamAudioSource.directMixLevel = room.sources[selectedSpeaker].directMixLevel;
            Speaker.steamAudioSource.reflectionsMixLevel = room.sources[selectedSpeaker].reflectionMixLevel;
            Speaker.audioSource.clip = Resources.Load<AudioClip>("Audio/" + room.sources[selectedSpeaker].audioClip);
            Speaker.steamAudioSource.applyHRTFToReflections = room.sources[selectedSpeaker].applyHRTFToReflections == 1;
        }
    }

    public void UpdateSelectedSpeaker()
    {
        selectedSpeaker++;   
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

    public void PlayAudio()
    {
        speakers[selectedSpeaker].audioSource.Play();
    }

    public void StopAudio()
    {
        speakers[selectedSpeaker].audioSource.Stop();
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
        Speaker.distanceToReceiver = Calculator.CalculateDistanceToReceiver(receiver.transform, Speaker.audioSource.transform);
        Speaker.azimuth = Calculator.CalculateAzimuth(receiver.transform, Speaker.audioSource.transform);
        Speaker.elevation = Calculator.CalculateElevation(receiver.transform, Speaker.audioSource.transform);
    }

    public void SetDefaulPosition()
    {
        receiver.transform.localPosition = Dimensions.defaultLocation;
    }
    public void RandomisePosition()
    {
        receiver.transform.localPosition = Calculator.CalculateNewPosition();
    }
}