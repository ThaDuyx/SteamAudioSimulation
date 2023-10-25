using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SimulationView : MonoBehaviour
{
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text currentHRTFText;
    [SerializeField] private TMP_Text sampleRateText;
    [SerializeField] private TMP_Text simulationDurationText;
    [SerializeField] private Slider bounceSlider;
    [SerializeField] private Toggle applyReflToHRTFToggle;
    
    // [SerializeField] private TMP_Text distanceText;
    // [SerializeField] private TMP_Text wallDistanceText;
    // [SerializeField] private TMP_Text reflectionDistanceText;

    // Basic Unity MonoBehaviour method - Essentially a start-up function
    private void Start()
    {
        SetUI();
    }

    // Basic Unity MonoBehaviour method - Update is called every frame, if the MonoBehaviour is enabled.
    void Update()
    {
        HandleKeyStrokes();

        // HandleSimulation();

        HandleRender();
    }

    // Method for handling whenever specific keys are pressed on the keyboard.
    private void HandleKeyStrokes()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            ToggleSimulation();

            SetUI();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            SimulationManager.Instance.ToggleAudio();
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            // SimulationManager.Instance.PlayAudio();
            // RoomManager.Instance.TestScene();
        }
    }

    // Either starts or stops the simulation dependent on which state currently is active.
    private void ToggleSimulation()
    {
        if (SimulationManager.Instance.IsRendering)
        {
            SimulationManager.Instance.StopRender();
        }
        else 
        {
            SimulationManager.Instance.StartRender(method: RenderMethod.LoneSpeaker);
        }
    }

    // Called in the Update() MonoBehavior method
    private void HandleSimulation()
    {
        if (SimulationManager.Instance.IsTiming() && SimulationManager.Instance.IsRendering)
        {
            // Update time while rendering
            timerText.text = "Time left: " + SimulationManager.Instance.TimeLeft() + "s";
            simulationDurationText.text = "Time left: " + SimulationManager.Instance.TimeLeftOfSimulation() + "s";
        }
        else 
        {
            // Continue rendering until we reach the Last HRTF in our list where the rendering come to a halt
            if (SimulationManager.Instance.IsRendering && !SimulationManager.Instance.IsLastHRTF())
            {
                SimulationManager.Instance.ContinueRender(method: RenderMethod.AllSpeakers);
                
                SetUI();
            }
            else if (SimulationManager.Instance.IsRendering && SimulationManager.Instance.IsLastHRTF())
            {
                SimulationManager.Instance.StopRender();

                SetUI();
            }
        }
    }

    private void HandleRender()
    {
        if (SimulationManager.Instance.IsTiming() && SimulationManager.Instance.IsRendering)
        {
            // Update time while rendering
            timerText.text = "Time left: " + SimulationManager.Instance.TimeLeft() + "s";
            simulationDurationText.text = "Time left: " + SimulationManager.Instance.TimeLeftOfSimulation() + "s";
        }
        else 
        {
            // Continue rendering until we reach the Last HRTF in our list where the rendering come to a halt
            if (SimulationManager.Instance.IsRendering && !SimulationManager.Instance.IsLastSpeaker())
            {
                SimulationManager.Instance.ContinueRender(method: RenderMethod.LoneSpeaker);
                
                SetUI();
            }
            else if (SimulationManager.Instance.IsRendering && SimulationManager.Instance.IsLastSpeaker())
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

        currentHRTFText.text = SimulationManager.Instance.CurrentHRTFName();

        sampleRateText.text = "fs: " + AudioSettings.outputSampleRate.ToString();

        bounceSlider.value = SimulationManager.Instance.GetRealTimeBounces();

        bounceSlider.GetComponentInChildren<TMP_Text>().text = bounceSlider.value.ToString();

        applyReflToHRTFToggle.isOn = SimulationManager.Instance.GetHRTFReflectionStatus();

        /* Distances
        distanceText.text = "d(source): " + GeometryManager.Instance.DistanceToSource() + " units (m)";
        wallDistanceText.text = "d(wall): " + GeometryManager.Instance.DistanceToWall() + " units (m)";
        reflectionDistanceText.text = "d(refl): " + GeometryManager.Instance.DistanceOfReflection() + " units (m)";
        */
    }

    public void BounceSliderChanged(float value)
    {
        SimulationManager.Instance.SetRealTimeBounces(value);
        bounceSlider.GetComponentInChildren<TMP_Text>().text = bounceSlider.value.ToString();
    }

    public void HRTFToggleChanged(bool isOn)
    {
        SimulationManager.Instance.SetHRTFReflectionStatus(isOn);
    }
}