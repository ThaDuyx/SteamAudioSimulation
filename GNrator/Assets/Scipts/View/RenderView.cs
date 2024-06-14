using System;
using SteamAudio;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RenderView : MonoBehaviour, IRenderObserver
{
    [SerializeField] private TMP_Text timerText, currentSOFAText, sampleRateText, totalTimeText, rendersLeftText, stateText, progressText;
    [SerializeField] private TMP_Dropdown audioClipDropdown, speakerDropdown, renderMethodDropdown, roomDropdown, sofaFileDropdown, renderAmountDropdown;
    [SerializeField] private Slider bounceSlider, volumeSlider, directMixLevelSlider, reflectionMixLevelSlider, progressSlider;
    [SerializeField] private Slider lowFreqAbsorpSlider, midFreqAbsorpSlider, highFreqAbsorpSlider, scatteringSlider;
    [SerializeField] private Toggle applyReflToHRTFToggle, distAttenuationToggle, airAbsorptionToggle;
    [SerializeField] private Button renderBtn;
    private float currentVelocity = 0.0f; // progress slider

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
        
        HandleRenderState();
    }

    // Observer notification method
    public void OnNotify()
    {
        SetUI();
    }

    public void RenderComplete()
    {
        progressSlider.gameObject.SetActive(false);
        progressSlider.value = 0;
    }

    // Method for handling whenever specific keys are pressed on the keyboard.
    private void HandleKeyStrokes()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            RenderManager.Instance.ToggleRender();

            SetUI();
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            RenderManager.Instance.sourceVM.ToggleAudio();
        }
    }

    // Called in the Update() MonoBehavior method
    private void HandleRenderState()
    {
        if (RenderManager.Instance.IsRendering)
        {
            stateText.text = "Rendering";
            timerText.text = RenderManager.Instance.CurrentTimeLeft + "s";
            totalTimeText.text = RenderManager.Instance.InRoomTimeLeft + "s";
            progressText.text = RenderManager.Instance.TotalProgress + "s";
            rendersLeftText.text = RenderManager.Instance.FetchRenderProgress();
            
            float currentProgress = Mathf.SmoothDamp(progressSlider.value, RenderManager.Instance.Progress, ref currentVelocity, 100 * Time.deltaTime);
            progressSlider.value = currentProgress;
        }
    }

    private void SetContent()
    {
        RenderManager.Instance.AddObserver(this);

        // feed dropdowns with data
        speakerDropdown.options.Clear();
        foreach (string speakerName in RenderManager.Instance.sourceVM.GetSourceNames())
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

        renderMethodDropdown.options.ForEach(option => 
        {
            if (SettingsManager.Instance.settings.selectedRenderMethod == option.text)
            {
                renderMethodDropdown.value = renderMethodDropdown.options.IndexOf(option);
            }
        });
        renderMethodDropdown.RefreshShownValue();
        
        
        sofaFileDropdown.options.Clear();
        foreach (string sofaFile in SteamAudioManager.Singleton.hrtfNames)
        {
            sofaFileDropdown.options.Add(new TMP_Dropdown.OptionData() { text = sofaFile });
        }
        sofaFileDropdown.RefreshShownValue();

        renderAmountDropdown.options.Clear();
        for (int i = 1; i <= Constants.renderAmountAllowed; i++) 
        {
            renderAmountDropdown.options.Add(new TMP_Dropdown.OptionData() { text = i.ToString() });
        }
        renderAmountDropdown.RefreshShownValue();
        renderAmountDropdown.value = SettingsManager.Instance.settings.selectedRenderAmount - 1;

        bounceSlider.value = SettingsManager.Instance.settings.reflectionBounce;
        SteamAudioSettings.Singleton.realTimeBounces = SettingsManager.Instance.settings.reflectionBounce;
        bounceSlider.GetComponentInChildren<TMP_Text>().text = bounceSlider.value.ToString();

        volumeSlider.value = RenderManager.Instance.sourceVM.Volume;
        volumeSlider.GetComponentInChildren<TMP_Text>().text = volumeSlider.value.ToString("F2");

        directMixLevelSlider.value = RenderManager.Instance.sourceVM.DirectMixLevel;
        directMixLevelSlider.GetComponentInChildren<TMP_Text>().text = directMixLevelSlider.value.ToString("F2");

        reflectionMixLevelSlider.value = RenderManager.Instance.sourceVM.ReflectionMixLevel;
        reflectionMixLevelSlider.GetComponentInChildren<TMP_Text>().text = reflectionMixLevelSlider.value.ToString("F2");

        lowFreqAbsorpSlider.value = SettingsManager.Instance.settings.lowFreqAbsorption;
        RoomManager.Instance.Material.lowFreqAbsorption = SettingsManager.Instance.settings.lowFreqAbsorption;
        lowFreqAbsorpSlider.GetComponentInChildren<TMP_Text>().text = lowFreqAbsorpSlider.value.ToString("F2");
        
        midFreqAbsorpSlider.value = SettingsManager.Instance.settings.midFreqAbsorption;
        RoomManager.Instance.Material.midFreqAbsorption = SettingsManager.Instance.settings.midFreqAbsorption;
        midFreqAbsorpSlider.GetComponentInChildren<TMP_Text>().text = midFreqAbsorpSlider.value.ToString("F2");
        
        highFreqAbsorpSlider.value = SettingsManager.Instance.settings.highFreqAbsorption;
        RoomManager.Instance.Material.highFreqAbsorption = SettingsManager.Instance.settings.highFreqAbsorption;
        highFreqAbsorpSlider.GetComponentInChildren<TMP_Text>().text = highFreqAbsorpSlider.value.ToString("F2");

        scatteringSlider.value = SettingsManager.Instance.settings.scattering;
        RoomManager.Instance.Material.scattering = SettingsManager.Instance.settings.scattering;
        scatteringSlider.GetComponentInChildren<TMP_Text>().text = scatteringSlider.value.ToString("F2");
    }

    // Updates elements in the UI
    private void SetUI()
    {
        stateText.text = "Idle";
        
        timerText.text = "Idle";

        totalTimeText.text = "Idle";

        progressText.text = "Idle";

        rendersLeftText.text = "Idle";

        currentSOFAText.text = SteamAudioManager.Singleton.ActiveSOFAName();

        sampleRateText.text = UnityEngine.AudioSettings.outputSampleRate.ToString();

        renderBtn.GetComponentInChildren<TMP_Text>().text = RenderManager.Instance.IsRendering ? "Stop Render" : "Start Render";

        audioClipDropdown.options.ForEach(option => 
        {
            if (RenderManager.Instance.sourceVM.AudioClipIsTheSameAs(option.text))
            {
                audioClipDropdown.value = audioClipDropdown.options.IndexOf(option);
            } 
        });
        audioClipDropdown.RefreshShownValue();

        volumeSlider.value = RenderManager.Instance.SelectedRenderMethod == RenderMethod.NearField ? RenderManager.Instance.sourceVM.NearFieldSource.audioSource.volume : RenderManager.Instance.sourceVM.Volume;
        volumeSlider.GetComponentInChildren<TMP_Text>().text = volumeSlider.value.ToString("F2");
        directMixLevelSlider.value = RenderManager.Instance.SelectedRenderMethod == RenderMethod.NearField ? RenderManager.Instance.sourceVM.NearFieldSource.steamAudioSource.directMixLevel : RenderManager.Instance.sourceVM.DirectMixLevel;
        directMixLevelSlider.GetComponentInChildren<TMP_Text>().text = directMixLevelSlider.value.ToString("F2");
        reflectionMixLevelSlider.value = RenderManager.Instance.SelectedRenderMethod == RenderMethod.NearField ? RenderManager.Instance.sourceVM.NearFieldSource.steamAudioSource.reflectionsMixLevel : RenderManager.Instance.sourceVM.ReflectionMixLevel;
        reflectionMixLevelSlider.GetComponentInChildren<TMP_Text>().text = reflectionMixLevelSlider.value.ToString("F2");

        applyReflToHRTFToggle.isOn = RenderManager.Instance.sourceVM.ApplyHRTFToReflections;
    }

    public void RenderBtnPressed()
    {
        RenderManager.Instance.ToggleRender();
        renderBtn.GetComponentInChildren<TMP_Text>().text = RenderManager.Instance.IsRendering ? "Stop Render" : "Start Render";
        progressSlider.gameObject.SetActive(true);
        SetUI();
    }

    public void SpeakerDropdownChanged(int index)
    {
        RenderManager.Instance.sourceVM.SetSelectedSource(index);
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
        SteamAudioSettings.Singleton.realTimeBounces = (int)bounceSlider.value;
        SettingsManager.Instance.settings.reflectionBounce = (int)bounceSlider.value;
        SettingsManager.Instance.Save();
        
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
        SettingsManager.Instance.settings.lowFreqAbsorption = value;
        SettingsManager.Instance.Save();
    }
    public void MidFreqAbsorpSliderChanged(float value) 
    { 
        RoomManager.Instance.Material.midFreqAbsorption = value; 
        midFreqAbsorpSlider.GetComponentInChildren<TMP_Text>().text = midFreqAbsorpSlider.value.ToString("F2");
        SettingsManager.Instance.settings.midFreqAbsorption = value;
        SettingsManager.Instance.Save();
    }
    public void HighFreqAbsorpSliderChanged(float value) 
    { 
        RoomManager.Instance.Material.highFreqAbsorption = value; 
        highFreqAbsorpSlider.GetComponentInChildren<TMP_Text>().text = highFreqAbsorpSlider.value.ToString("F2");
        SettingsManager.Instance.settings.highFreqAbsorption = value;
        SettingsManager.Instance.Save();
    }
    public void ScatteringSliderChanged(float value) 
    {
        RoomManager.Instance.Material.scattering = value; 
        scatteringSlider.GetComponentInChildren<TMP_Text>().text = scatteringSlider.value.ToString("F2");
        SettingsManager.Instance.settings.scattering = value;
        SettingsManager.Instance.Save();
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

    public void RoomDropdownChanged(int index)
    {
        // Plus one since we're ignoring the first scene being the Canvas UI
        RoomManager.Instance.ChangeScene(sceneIndexInBuildSettings: index + 1);
        
        // Call-back function that reloads the UI with new data when scenes has been changed
        RoomManager.OnSceneUnloaded += HandleSceneUnloaded;
    }

    public void RenderAmountDropdownChanged(int index)
    {
        RenderManager.Instance.AmountOfRenders = index;
        SettingsManager.Instance.settings.selectedRenderAmount = index + 1;
        SettingsManager.Instance.Save();
        SetUI();
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