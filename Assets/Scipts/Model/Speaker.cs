using SteamAudio;
using UnityEngine;

public class Speaker
{
    public AudioSource audioSource;
    public SteamAudioSource steamAudioSource;

    public string name;
    public float azimuth, elevation, distanceToReceiver;

    public Speaker(AudioSource audioSource, SteamAudioSource steamAudioSource)
    {
        this.audioSource = audioSource;
        this.steamAudioSource = steamAudioSource;
        name = audioSource.name;
    }
}
