using System.Collections;
using System.Collections.Generic;
using SteamAudio;
using UnityEngine;

public class Settings : MonoBehaviour
{
    public static Settings Instance { get; private set; }

    private SteamAudioSettings steamAudioSettings;
    // Start is called before the first frame update
    void Start()
    {
        SteamAudioSettings[] steamAudioSettingsContainer = FindObjectsOfType<SteamAudioSettings>();
        steamAudioSettings = steamAudioSettingsContainer[0];
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PrintBounceSettings()
    {
        print(steamAudioSettings.realTimeBounces);
    }
}
