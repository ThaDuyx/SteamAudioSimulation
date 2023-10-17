using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

// IMPORTANT: This is the class that make us able to record in game!
public class AudioCapturer : MonoBehaviour
{
    // OnAudioFilterRead is a MonoBehaviour method that has to be attached to a GameObject with a AudioListener or AudioSource
    private void OnAudioFilterRead(float [] data, int channels)
    {
        if (SimulationManager.Instance != null && SimulationManager.Instance.IsRendering())
        {
            SimulationManager.Instance.TransmitData(data);
        }
    }
}
