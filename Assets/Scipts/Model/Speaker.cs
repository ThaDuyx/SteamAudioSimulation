using SteamAudio;
using UnityEngine;

public class Speaker
{
    public readonly AudioSource audioSource;
    public readonly SteamAudioSource steamAudioSource;

    public string Name { get; set; }
    public float Azimuth{ get; set; }
    public float Elevation { get; set; }
    public float DistanceToReceiver { get; set; }
    public float Volume() { return audioSource.volume; }
    public float DirectMixLevel() { return steamAudioSource.directMixLevel; }
    public float ReflectionMixLevel() { return steamAudioSource.reflectionsMixLevel; }
    public bool HRTFAppliedToReflection() { return steamAudioSource.applyHRTFToReflections; }
    public string ClipName() { return audioSource.clip.name; }
    public bool DistanceAttenuation() { return steamAudioSource.distanceAttenuation; }
    public bool AirAbsorption() { return steamAudioSource.airAbsorption; }

    public Speaker(AudioSource source, SteamAudioSource steamSource)
    {
        audioSource = source;
        steamAudioSource = steamSource;
        Name = source.name;
    }
}
