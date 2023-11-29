using System;
using SteamAudio;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RenderView : MonoBehaviour, IRenderObserver
{
    [SerializeField] private TMP_Text timerText, currentSOFAText, sampleRateText, simulationDurationText;
    [SerializeField] private TMP_Dropdown audioClipDropdown, speakerDropdown, renderMethodDropdown, roomDropdown, sofaFileDropdown;
    [SerializeField] private Slider bounceSlider, volumeSlider, directMixLevelSlider, reflectionMixLevelSlider;
    [SerializeField] private Slider lowFreqAbsorpSlider, midFreqAbsorpSlider, highFreqAbsorpSlider, scatteringSlider;
    [SerializeField] private Toggle applyReflToHRTFToggle, distAttenuationToggle, airAbsorptionToggle;
    [SerializeField] private TMP_InputField roomsInputField;

    // Basic Unity MonoBehaviour method - Essentially a start-up function
    void Start()
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

    // Observer notification method
    public void OnNotify()
    {
        SetUI();
    }

    // Method for handling whenever specific keys are pressed on the keyboard.
    private void HandleKeyStrokes()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            ExcecuteRender();
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            RenderManager.Instance.sourceVM.ToggleAudio();
        }
    }

    // Either starts or stops the simulation dependent on which state currently is active.
    private void ExcecuteRender()
    {
        RenderManager.Instance.SetupRender();  
        RenderManager.Instance.ToggleRender();
        SetUI();
    }

    // Called in the Update() MonoBehavior method
    private void HandleRender()
    {
        if (RenderManager.Instance.IsTiming && RenderManager.Instance.IsRendering)
        {
            // Update time while rendering
            timerText.text = "Time left: " + RenderManager.Instance.CurrentTimeLeft + "s";
            simulationDurationText.text = "Time left: " + RenderManager.Instance.TotalTimeLeft + "s";
        }
    }

    private void SetContent()
    {
        RenderManager.Instance.AddObserver(this);

        // feed dropdowns with data
        speakerDropdown.options.Clear();
        foreach (string speakerName in RenderManager.Instance.sourceVM.GetSpeakerNames())
        {
            speakerDropdown.options.Add(new TMP_Dropdown.OptionData() { text = speakerName });
        }
        speakerDropdown.options.Add(new TMP_Dropdown.OptionData() { text = "All speakers" });
        speakerDropdown.RefreshShownValue();

        audioClipDropdown.options.Clear();
        foreach (string audioClip in DataManager.Instance.GetAudioClips())
        {
            audioClipDropdown.options.Add(new TMP_Dropdown.OptionData() { text = audioClip });
        }
        
        roomDropdown.options.Clear();
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            // we don't want the first scene since it is just the Canvas UI
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

        sofaFileDropdown.options.Clear();
        foreach (string sofaFile in SteamAudioManager.Singleton.hrtfNames)
        {
            sofaFileDropdown.options.Add(new TMP_Dropdown.OptionData() { text = sofaFile});
        }
        sofaFileDropdown.RefreshShownValue();
    }

    // Updates elements in the UI
    private void SetUI()
    {
        timerText.text = "Press T";

        simulationDurationText.text = "Idle state";

        currentSOFAText.text = SteamAudioManager.Singleton.ActiveSOFAName();

        sampleRateText.text = "fs: " + UnityEngine.AudioSettings.outputSampleRate.ToString();

        bounceSlider.value = RenderManager.Instance.sourceVM.RealTimeBounces;
        bounceSlider.GetComponentInChildren<TMP_Text>().text = bounceSlider.value.ToString();

        volumeSlider.value = RenderManager.Instance.sourceVM.Volume;
        volumeSlider.GetComponentInChildren<TMP_Text>().text = volumeSlider.value.ToString("F2");

        renderMethodDropdown.RefreshShownValue();

        directMixLevelSlider.value = RenderManager.Instance.sourceVM.DirectMixLevel;
        directMixLevelSlider.GetComponentInChildren<TMP_Text>().text = directMixLevelSlider.value.ToString("F2");

        reflectionMixLevelSlider.value = RenderManager.Instance.sourceVM.ReflectionMixLevel;
        reflectionMixLevelSlider.GetComponentInChildren<TMP_Text>().text = reflectionMixLevelSlider.value.ToString("F2");

        applyReflToHRTFToggle.isOn = RenderManager.Instance.sourceVM.ApplyHRTFToReflections;

        lowFreqAbsorpSlider.value = RoomManager.Instance.Material.lowFreqAbsorption;
        lowFreqAbsorpSlider.GetComponentInChildren<TMP_Text>().text = lowFreqAbsorpSlider.value.ToString("F2");
        midFreqAbsorpSlider.value = RoomManager.Instance.Material.midFreqAbsorption;
        midFreqAbsorpSlider.GetComponentInChildren<TMP_Text>().text = midFreqAbsorpSlider.value.ToString("F2");
        highFreqAbsorpSlider.value = RoomManager.Instance.Material.highFreqAbsorption;
        highFreqAbsorpSlider.GetComponentInChildren<TMP_Text>().text = highFreqAbsorpSlider.value.ToString("F2");
        scatteringSlider.value = RoomManager.Instance.Material.scattering;
        scatteringSlider.GetComponentInChildren<TMP_Text>().text = scatteringSlider.value.ToString("F2");

        audioClipDropdown.options.ForEach(option => 
        {
            if (RenderManager.Instance.sourceVM.AudioClipIsTheSameAs(option.text))
            {
                audioClipDropdown.value = audioClipDropdown.options.IndexOf(option);
            } 
        });
        audioClipDropdown.RefreshShownValue();
    }

    public void SpeakerDropdownChanged(int index)
    {
        RenderManager.Instance.sourceVM.SetSelectedSpeaker(index);
        SetUI();
    }

    public void AudioClipDropdownChanged(int index)
    {
        RenderManager.Instance.sourceVM.AudioClip = audioClipDropdown.options[index].text;
        RenderManager.Instance.sourceVM.PersistRoom();
    }

    public void SOFAFileDropdownChanged(int index)
    {
        currentSOFAText.text = sofaFileDropdown.options[index].text;
    }
    
    public void HRTFToggleChanged(bool isOn)
    {
        RenderManager.Instance.sourceVM.ApplyHRTFToReflections = isOn;
    }

    public void BounceSliderChanged(float value)
    {
        bounceSlider.value = value;
        bounceSlider.GetComponentInChildren<TMP_Text>().text = bounceSlider.value.ToString();
    }
    public void BounceSliderEndDrag()
    {
        RenderManager.Instance.sourceVM.RealTimeBounces = (int)bounceSlider.value;
        RoomManager.Instance.ChangeScene(sceneIndexInBuildSettings: RoomManager.Instance.ActiveSceneIndex);
    }
    
    public void WallSliderEndDrag()
    {
        RoomManager.Instance.ChangeScene(sceneIndexInBuildSettings: RoomManager.Instance.ActiveSceneIndex);
    }

    public void VolumeSliderChanged(float value)
    {
        RenderManager.Instance.sourceVM.Volume = value;
        volumeSlider.GetComponentInChildren<TMP_Text>().text = volumeSlider.value.ToString("F2");
    }

    public void DirectMixLevelSliderChanged(float value)
    {
        RenderManager.Instance.sourceVM.DirectMixLevel = value;
        directMixLevelSlider.GetComponentInChildren<TMP_Text>().text = directMixLevelSlider.value.ToString("F2");
    }

    public void ReflectionMixLevelSliderChanged(float value)
    {
        RenderManager.Instance.sourceVM.ReflectionMixLevel = value;
        reflectionMixLevelSlider.GetComponentInChildren<TMP_Text>().text = reflectionMixLevelSlider.value.ToString("F2");
    }
    
    public void LowFreqAbsorpSliderChanged(float value) 
    { 
        RoomManager.Instance.Material.lowFreqAbsorption = value; 
        lowFreqAbsorpSlider.GetComponentInChildren<TMP_Text>().text = lowFreqAbsorpSlider.value.ToString("F2");
    }
    public void MidFreqAbsorpSliderChanged(float value) 
    { 
        RoomManager.Instance.Material.midFreqAbsorption = value; 
        midFreqAbsorpSlider.GetComponentInChildren<TMP_Text>().text = midFreqAbsorpSlider.value.ToString("F2");
    }
    public void HighFreqAbsorpSliderChanged(float value) 
    { 
        RoomManager.Instance.Material.highFreqAbsorption = value; 
        highFreqAbsorpSlider.GetComponentInChildren<TMP_Text>().text = highFreqAbsorpSlider.value.ToString("F2");
    }
    public void ScatteringSliderChanged(float value) 
    {
        RoomManager.Instance.Material.scattering = value; 
        scatteringSlider.GetComponentInChildren<TMP_Text>().text = scatteringSlider.value.ToString("F2");
    }

    public void DistanceAttenuationToggleChanged(bool isOn) 
    {
        RenderManager.Instance.sourceVM.DistanceAttenuation = isOn;
        RenderManager.Instance.sourceVM.PersistRoom();
    }

    public void AirAbsorptionToggleChanged(bool isOn)
    {
        RenderManager.Instance.sourceVM.AirAbsorption = isOn;
        RenderManager.Instance.sourceVM.PersistRoom();
    }

    public void RenderMethodDropDownChanged(int index)
    {
        RenderManager.Instance.SetRenderMethod(renderMethod: (RenderMethod)index);
    }

    public void RoomsInputFieldChanged(string input)
    {
        if (int.TryParse(input, out int amountOfRooms) && amountOfRooms < 20)
        {
            // TODO: Enter amount of rooms added in for rendering
        }
        else
        {
            Console.WriteLine("The string is not a valid integer or too high of a value");
        }
    }

    public void RoomDropdownChanged(int index)
    {
        // Plus one since we're ignoring the first scene being the Canvas UI
        RoomManager.Instance.ChangeScene(sceneIndexInBuildSettings: index + 1);
        
        // Call-back function that reloads the UI with new data when scenes has been changed
        RoomManager.OnSceneUnloaded += HandleSceneUnloaded;
    }

    // callback function used to wait while managers are loading whenever we change scene
    private void HandleSceneUnloaded()
    {
        speakerDropdown.value = 0;

        SetUI();
    }

    public void SliderEndDrag()
    {
        RenderManager.Instance.sourceVM.PersistRoom();
    }
}