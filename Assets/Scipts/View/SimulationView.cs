using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SimulationView : MonoBehaviour
{
    [SerializeField] private TMP_Text timerText, currentSOFAText, sampleRateText, simulationDurationText;
    [SerializeField] private TMP_Dropdown audioClipDropdown, speakerDropdown, renderMethodDropdown, roomDropdown;
    [SerializeField] private Slider bounceSlider, volumeSlider, directMixLevelSlider, reflectionMixLevelSlider;
    [SerializeField] private Toggle applyReflToHRTFToggle;
    
    private RenderMethod chosenMethod = RenderMethod.OneByOne;

    // Basic Unity MonoBehaviour method - Essentially a start-up function
    private void Start()
    {
        SetContent();

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

        if (Input.GetKeyDown(KeyCode.O))
        {
            RenderManager.Instance.ToggleAudio();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            // RenderManager.Instance.ToggleAllAudio();
            SetUI();
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
            switch (chosenMethod)
            {
                case RenderMethod.AllAtOnce:
                    if (RenderManager.Instance.IsRendering && !RenderManager.Instance.IsLastSOFA)
                    {
                        RenderManager.Instance.ContinueRender(renderMethod: chosenMethod);
                        
                        SetUI();
                    }
                    else if (RenderManager.Instance.IsRendering && RenderManager.Instance.IsLastSOFA)
                    {
                        RenderManager.Instance.StopRender();

                        SetUI();
                    }
                    break;

                case RenderMethod.OneByOne:
                    if (RenderManager.Instance.IsRendering && !RenderManager.Instance.IsLastSpeaker) 
                    {
                        RenderManager.Instance.ContinueRender(renderMethod: chosenMethod);
                        
                        SetUI();
                    }
                    else if (RenderManager.Instance.IsRendering && RenderManager.Instance.IsLastSpeaker)
                    {
                        RenderManager.Instance.StopRender();

                        SetUI();
                    }
                    break;
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

    private void SetContent()
    {
        // feed dropdowns with data
        speakerDropdown.options.Clear();
        foreach (string speakerName in RenderManager.Instance.GetSpeakerNames())
        {
            speakerDropdown.options.Add(new TMP_Dropdown.OptionData() { text = speakerName });
        }
        speakerDropdown.RefreshShownValue();

        audioClipDropdown.options.Clear();
        foreach (string audioClip in DataManager.Instance.GetAudioClips())
        {
            audioClipDropdown.options.Add(new TMP_Dropdown.OptionData() { text = audioClip });
        }

        roomDropdown.options.Clear();
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            // We don't want the first scene since it is just the Canvas UI
            if (i != 0)
            {
                roomDropdown.options.Add(new TMP_Dropdown.OptionData() { text = "Room " + i });
            }
        }
        roomDropdown.RefreshShownValue();

        renderMethodDropdown.options.Clear();
        foreach (string method in Enum.GetNames(typeof(RenderMethod)))
        {
            renderMethodDropdown.options.Add(new TMP_Dropdown.OptionData() { text = method });
        }
        renderMethodDropdown.RefreshShownValue();
    }

    // Updates elements in the UI
    private void SetUI()
    {
        timerText.text = "Press T";

        simulationDurationText.text = "Idle state";

        currentSOFAText.text = RenderManager.Instance.ActiveSOFAName;

        sampleRateText.text = "fs: " + AudioSettings.outputSampleRate.ToString();

        bounceSlider.value = RenderManager.Instance.RealTimeBounces;
        bounceSlider.GetComponentInChildren<TMP_Text>().text = bounceSlider.value.ToString();

        volumeSlider.value = RenderManager.Instance.Volume;
        volumeSlider.GetComponentInChildren<TMP_Text>().text = volumeSlider.value.ToString("F2");

        directMixLevelSlider.value = RenderManager.Instance.DirectMixLevel;
        directMixLevelSlider.GetComponentInChildren<TMP_Text>().text = directMixLevelSlider.value.ToString("F2");

        reflectionMixLevelSlider.value = RenderManager.Instance.ReflectionMixLevel;
        reflectionMixLevelSlider.GetComponentInChildren<TMP_Text>().text = reflectionMixLevelSlider.value.ToString("F2");

        applyReflToHRTFToggle.isOn = RenderManager.Instance.ApplyHRTFToReflections;

        audioClipDropdown.value = GetAudioClipIndex();
        audioClipDropdown.RefreshShownValue();
    }

    public void SpeakerDropdownChanged(int index)
    {
        RenderManager.Instance.SetSelectedSpeaker(index);
        SetUI();
    }

    public void AudioClipDropdownChanged(int index)
    {
        RenderManager.Instance.AudioClip = audioClipDropdown.options[index].text;
        RenderManager.Instance.PersistRoom();
    }
    
    public void HRTFToggleChanged(bool isOn)
    {
        RenderManager.Instance.ApplyHRTFToReflections = isOn;
    }

    public void BounceSliderChanged(float value)
    {
        bounceSlider.value = value;
        bounceSlider.GetComponentInChildren<TMP_Text>().text = bounceSlider.value.ToString();
    }
    public void BounceSliderEndDrag()
    {
        RenderManager.Instance.RealTimeBounces = (int)bounceSlider.value;

        RoomManager.Instance.ChangeScene(sceneIndexInBuildSettings: RoomManager.Instance.ActiveSceneIndex);
    }

    public void VolumeSliderChanged(float value)
    {
        RenderManager.Instance.Volume = value;
        volumeSlider.GetComponentInChildren<TMP_Text>().text = volumeSlider.value.ToString("F2");
    }

    public void DirectMixLevelSliderChanged(float value)
    {
        RenderManager.Instance.DirectMixLevel = value;
        directMixLevelSlider.GetComponentInChildren<TMP_Text>().text = directMixLevelSlider.value.ToString("F2");
    }

    public void ReflectionMixLevelSliderChanged(float value)
    {
        RenderManager.Instance.ReflectionMixLevel = value;
        reflectionMixLevelSlider.GetComponentInChildren<TMP_Text>().text = reflectionMixLevelSlider.value.ToString("F2");
    }

    public void RenderMethodDropDownChanged(int index)
    {
        chosenMethod = (RenderMethod)index;
    }

    public void RoomDropdownChanged(int index)
    {
        // Plus one since we're ignoring the first scene being the Canvas UI
        RoomManager.Instance.ChangeScene(sceneIndexInBuildSettings: index + 1);
        
        // Call-back function that reloads the UI with new data when scenes has been changed
        RoomManager.OnSceneUnloaded += HandleSceneUnloaded;
    }

    private void HandleSceneUnloaded()
    {
        speakerDropdown.value = 0;

        SetUI();
    }

    public void SliderEndDrag()
    {
        RenderManager.Instance.PersistRoom();
    }

    private int GetAudioClipIndex()
    {
        for (int i = 0; i < audioClipDropdown.options.Count; i++)
        {
            if (audioClipDropdown.options[i].text == RenderManager.Instance.AudioClip + ".wav"
            || audioClipDropdown.options[i].text == RenderManager.Instance.AudioClip + ".mp3")
            {
                return i;
            }
        }

        return 0;
    }
}