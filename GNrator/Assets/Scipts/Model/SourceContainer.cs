using SteamAudio;
using UnityEngine;

public class Source
{
    public AudioSource audioSource;
    public SteamAudioSource steamAudioSource;

    public string name;
    public float azimuth, elevation, distanceToReceiver;

    public Source(AudioSource audioSource, SteamAudioSource steamAudioSource)
    {
        this.audioSource = audioSource;
        this.steamAudioSource = steamAudioSource;
        name = audioSource.name;
    }
}
