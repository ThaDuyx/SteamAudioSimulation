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
    private readonly RenderMethod chosenMethod = RenderMethod.LoneSpeaker;

    // Basic Unity MonoBehaviour method - Essentially a start-up function
    private void Start()
    {
        SetUI();
    }

    // Basic Unity MonoBehaviour method - Update is called every frame, if the MonoBehaviour is enabled.
    void Update()
    {
        HandleKeyStrokes();

        HandleRender();
    }

    // Method for handling whenever specific keys are pressed on the keyboard.
    private void HandleKeyStrokes()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            ToggleRender();

            SetUI();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            RenderManager.Instance.ToggleAllAudio();
        }
    }

    // Called in the Update() MonoBehavior method
    private void HandleRender()
    {
        if (RenderManager.Instance.IsTiming && RenderManager.Instance.IsRendering)
        {
            // Update time while rendering
            timerText.text = "Time left: " + RenderManager.Instance.TimeLeft + "s";
            simulationDurationText.text = "Time left: " + RenderManager.Instance.TimeLeftOfRender + "s";
        }
        else 
        {
            if (RenderManager.Instance.IsRendering && !RenderManager.Instance.IsLastSOFA
            || RenderManager.Instance.IsRendering && !RenderManager.Instance.IsLastSpeaker)
                {
                    RenderManager.Instance.ContinueRender(renderMethod: chosenMethod);
                    
                    SetUI();
                }
                else if (RenderManager.Instance.IsRendering && RenderManager.Instance.IsLastSOFA
                || RenderManager.Instance.IsRendering && RenderManager.Instance.IsLastSpeaker)
                {
                    RenderManager.Instance.StopRender();

                    SetUI();
                }
        }
    }

    // Either starts or stops the simulation dependent on which state currently is active.
    private void ToggleRender()
    {
        if (RenderManager.Instance.IsRendering)
        {
            RenderManager.Instance.StopRender();
        }
        else 
        {
            RenderManager.Instance.StartRender(renderMethod: chosenMethod);
        }
    }

    // Updates elements in the UI
    private void SetUI()
    {
        timerText.text = "Press T";

        simulationDurationText.text = "Idle state";

        currentHRTFText.text = RenderManager.Instance.ActiveSOFAName;

        sampleRateText.text = "fs: " + AudioSettings.outputSampleRate.ToString();

        bounceSlider.value = RenderManager.Instance.GetRealTimeBounces();

        bounceSlider.GetComponentInChildren<TMP_Text>().text = bounceSlider.value.ToString();

        applyReflToHRTFToggle.isOn = RenderManager.Instance.GetHRTFReflectionStatus();
    }

    public void BounceSliderChanged(float value)
    {
        RenderManager.Instance.SetRealTimeBounces(value);
        bounceSlider.GetComponentInChildren<TMP_Text>().text = bounceSlider.value.ToString();
    }

    public void HRTFToggleChanged(bool isOn)
    {
        RenderManager.Instance.SetHRTFReflectionStatus(isOn);
    }
}