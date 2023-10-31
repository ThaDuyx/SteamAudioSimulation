using SteamAudio;
using UnityEngine;

public class Speaker
{
    public AudioSource audioSource;
    public SteamAudioSource steamAudioSource;

    public string Name { get; set; }
    public float Azimuth{ get; set; }
    public float Elevation { get; set; }
    public float DistanceToReceiver { get; set; }

    public Speaker(AudioSource source, SteamAudioSource steamSource)
    {
        audioSource = source;
        steamAudioSource = steamSource;
        Name = source.name;
    }
}
