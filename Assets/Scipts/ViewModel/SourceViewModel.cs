using System.Collections.Generic;
using SteamAudio;
using UnityEngine;

public class SourceViewModel
{
    List<Speaker> speakers;
    private readonly int selectedSpeaker = 0;
    private bool isAllSpeakersSelected;
    private Speaker nearFieldSource;
    
    public SourceViewModel(List<Speaker> speakers, Speaker nearFieldSource)
    {
        this.speakers = speakers;
        this.nearFieldSource = nearFieldSource;
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
}