using TMPro;
using UnityEngine;

public class SimulationView : MonoBehaviour
{
    [SerializeField]
    private TMP_Text timerText;
    [SerializeField]
    private TMP_Text currentHRTFText;
    [SerializeField]
    private TMP_Text sampleRateText;
    [SerializeField] 
    private TMP_Text distanceText;
    [SerializeField]
    private TMP_Text wallDistanceText;
    [SerializeField]
    private TMP_Text simulationDurationText;
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

            SetUI();
        }
    }

    // Either starts or stops the simulation dependent on which state currently is active.
    private void ToggleSimulation()
    {
        if (SimulationManager.Instance.IsRendering())
        {
            SimulationManager.Instance.StopRender();
        }
        else 
        {
            SimulationManager.Instance.StartRender();
        }
    }

    // Called in the Update() MonoBehavior method
    private void HandleSimulation()
    {
        if (SimulationManager.Instance.IsTiming() && SimulationManager.Instance.IsRendering())
        {
            // Update time while rendering
            timerText.text = "Time left: " + SimulationManager.Instance.TimeLeft() + "s";
            simulationDurationText.text = "Time left: " + SimulationManager.Instance.TimeLeftOfSimulation() + "s";
        }
        else 
        {
            // Continue rendering until we reach the Last HRTF in our list where the rendering come to a halt
            if (SimulationManager.Instance.IsRendering() && !SimulationManager.Instance.IsLastHRTF())
            {
                SimulationManager.Instance.ContinueRender();
                
                SetUI();
            }
            else if (SimulationManager.Instance.IsRendering() && SimulationManager.Instance.IsLastHRTF())
            {
                SimulationManager.Instance.StopRender();

                SetUI();
            }
        }
    }

    // Updates elements in the UI
    private void SetUI()
    {
        timerText.text = "Press T";

        simulationDurationText.text = "Idle state";

        currentHRTFText.text = "sofa: " + SimulationManager.Instance.CurrentHRTFName();

        distanceText.text = "d(source): " + GeometryManager.Instance.SourceDistance() + " units (m)";

        wallDistanceText.text = "d(wall): " + GeometryManager.Instance.WallDistance() + " units (m)";

        sampleRateText.text = "fs: " + UnityEngine.AudioSettings.outputSampleRate.ToString();
    }
}