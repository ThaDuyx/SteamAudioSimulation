using UnityEngine;

// IMPORTANT: This is the class that make us able to record in game!
public class AudioCapturer : MonoBehaviour
{
    // private bool test = false;
    // OnAudioFilterRead is a MonoBehaviour method that has to be attached to a GameObject with a AudioListener or AudioSource
    private void OnAudioFilterRead(float [] data, int channels)
    {
        if (SimulationManager.Instance != null && SimulationManager.Instance.IsRendering)
        {
            SimulationManager.Instance.TransmitData(data);

            for (int i = 0; i < data.Length; i++) 
            {
                if (data[i] > 1) 
                {
                    Debug.Log("Amp. over 1");
                }

                if (data[i] < -1) 
                {
                    Debug.Log("Amp. under -1");
                }
            }
        }
    }
}