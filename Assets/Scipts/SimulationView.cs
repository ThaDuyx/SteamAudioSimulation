using TMPro;
using UnityEngine;

public class SimulationView : MonoBehaviour
{
    [SerializeField]
    private Transform cameraTransform;
    [SerializeField]
    private Transform speakerTransform;
    [SerializeField]
    private TMP_Text timerText;
    [SerializeField]
    private TMP_Text currentHRTFText;
    [SerializeField]
    private TMP_Text sampleRateText;

    // Basic Unity MonoBehaviour method - Essentially a start-up function
    void Start()
    {
        SetUI();
    }

    // Basic Unity MonoBehaviour method - Update is called every frame, if the MonoBehaviour is enabled.
    void Update()
    {
        HandleKeyStrokes();

        HandleSimulation();
    }

    // Method for handling whenever specific keys are pressed on the keyboard.
    private void HandleKeyStrokes()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            ToggleSimulation();
        }
    }

    // Either starts or stops the simulation dependent on which state currently is active.
    private void ToggleSimulation()
    {
        if (SimulationManager.Instance.IsRendering())
        {
            SimulationManager.Instance.StopRendering();
        }
        else 
        {
            SimulationManager.Instance.StartRendering();
        }
    }

    // Called in the Update() MonoBehavior method
    private void HandleSimulation()
    {
        if (SimulationManager.Instance.IsTiming() && SimulationManager.Instance.IsRendering())
        {
            // Update time while rendering
            timerText.text = "Time left: " + SimulationManager.Instance.TimeLeft();
        }
        else 
        {
            // Continue rendering until we reach the Last HRTF in our list where the rendering come to a halt
            if (SimulationManager.Instance.IsRendering() && !SimulationManager.Instance.IsLastHRTF())
            {
                SimulationManager.Instance.ContinueRendering();
                
                currentHRTFText.text = SimulationManager.Instance.CurrentHRTFName();
            }
            else if (SimulationManager.Instance.IsRendering() && SimulationManager.Instance.IsLastHRTF())
            {
                SimulationManager.Instance.StopRendering();

                SetUI();
            }
        }
    }

    // Updates elements in the UI
    private void SetUI()
    {
        timerText.text = "Press T";

        float distance = Vector3.Distance(cameraTransform.position, speakerTransform.position);

        currentHRTFText.text = distance.ToString() + " units.";

        sampleRateText.text = UnityEngine.AudioSettings.outputSampleRate.ToString();
    }
}