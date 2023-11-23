using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static TMPro.TMP_Dropdown;

public class SimulationView : MonoBehaviour
{
    [SerializeField] private TMP_Text timerText, currentSOFAText, sampleRateText, simulationDurationText;
    [SerializeField] private TMP_Dropdown audioClipDropdown, speakerDropdown, renderMethodDropdown, roomDropdown, sofaFileDropdown, roomFolderDropdown, renderFolderDropdown;
    [SerializeField] private Slider bounceSlider, volumeSlider, directMixLevelSlider, reflectionMixLevelSlider;
    [SerializeField] private Slider lowFreqAbsorpSlider, midFreqAbsorpSlider, highFreqAbsorpSlider, scatteringSlider;
    [SerializeField] private Toggle applyReflToHRTFToggle, distAttenuationToggle, airAbsorptionToggle;
    [SerializeField] private TMP_InputField roomsInputField; 
    [SerializeField] private Button addRoomFolderButton, deleteRoomFolderButton, addRenderFolderButton, deleteRenderFolderButton, defaultLocationButton, locationRandomiserButton;
    
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
            RenderManager.Instance.SetDefaultLocation();
            RenderManager.Instance.CreateNewRoomFolder();

            ToggleRender();

            SetUI();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            print("L Clicked");

            RenderManager.Instance.CreateNewRoomFolder();
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            RenderManager.Instance.ToggleAudio();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            RenderManager.Instance.ToggleAllAudio();
        }
    }

    // Either starts or stops the simulation dependent on which state currently is active.
    private void ToggleRender()
    {
        if (RenderManager.Instance.IsRendering)
        {
            RenderManager.Instance.StopRender(renderMethod: chosenMethod);
        }
        else
        {
            RenderManager.Instance.StartRender(renderMethod: chosenMethod, sofaIndex: sofaFileDropdown.value);
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
            HandleRenderMethod();
        }
    }

    private void HandleRenderMethod()
    {
        switch (chosenMethod)
            {
                case RenderMethod.AllAtOnce:
                {
                    if (RenderManager.Instance.IsRendering && !RenderManager.Instance.IsLastSOFA)
                    {
                        RenderManager.Instance.ContinueRender(renderMethod: chosenMethod);
                        
                        SetUI();
                    }
                    else if (RenderManager.Instance.IsRendering && RenderManager.Instance.IsLastSOFA)
                    {
                        RenderManager.Instance.StopRender(renderMethod: chosenMethod);

                        SetUI();
                    }
                    break;
                }

                case RenderMethod.RenderRooms:
                {
                    if (RenderManager.Instance.IsRendering && !RenderManager.Instance.IsLastSOFA)
                    {
                        RenderManager.Instance.ContinueRender(renderMethod: chosenMethod);

                        SetUI();
                    }
                    else if (RenderManager.Instance.IsRendering && RenderManager.Instance.IsLastSOFA)
                    {
                        RenderManager.Instance.StopRender(renderMethod: chosenMethod);

                        SetUI();

                        if (!RenderManager.Instance.IsLastRoom)
                        {
                            RenderManager.Instance.UpdateSelectedSpeaker();
                            RenderManager.Instance.UpdateRenderPath();
                            RenderManager.Instance.GoToDefaultLocation();
                            
                            ToggleRender();

                            SetUI();
                        }
                    }
                    break;
                }

                case RenderMethod.OneByOne:
                {
                    if (RenderManager.Instance.IsRendering && !RenderManager.Instance.IsLastSpeaker) 
                    {
                        RenderManager.Instance.ContinueRender(renderMethod: chosenMethod);
                        
                        SetUI();
                    }
                    else if (RenderManager.Instance.IsRendering && RenderManager.Instance.IsLastSpeaker)
                    {
                        RenderManager.Instance.StopRender(renderMethod: chosenMethod);

                        SetUI();
                    }
                    break;
                }

                default: break;
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
        renderMethodDropdown.options.ForEach((option) => 
        {
            if (option.text == RenderManager.Instance.SelectedRenderMethod.ToString())
            {
                int index = renderMethodDropdown.options.IndexOf(option);
                renderMethodDropdown.value = index;

            }
        });
        renderMethodDropdown.RefreshShownValue();

        sofaFileDropdown.options.Clear();
        foreach (string sofaFile in RenderManager.Instance.SOFANames)
        {
            sofaFileDropdown.options.Add(new TMP_Dropdown.OptionData() { text = sofaFile});
        }
        sofaFileDropdown.RefreshShownValue();

        LoadRoomFolderDropdownData();
        roomFolderDropdown.options.ForEach((option) => 
        {        
            if (RenderManager.roomsPath + option.text + "/" == RenderManager.Instance.SelectedRoomPath)
            {
                int index = roomFolderDropdown.options.IndexOf(option);
                roomFolderDropdown.value = index;
            }
        });
        roomFolderDropdown.RefreshShownValue();

        LoadRenderFolderDropdownData();
        renderFolderDropdown.options.ForEach((option) => 
        {
            if (RenderManager.Instance.SelectedRoomPath + option.text + "/" == RenderManager.Instance.SelectedRenderPath)
            {
                int index = renderFolderDropdown.options.IndexOf(option);
                renderFolderDropdown.value = index;
            }
        });
        renderFolderDropdown.RefreshShownValue();

        RenderManager.Instance.ToggleStartUp();
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

        lowFreqAbsorpSlider.value = RoomManager.Instance.Material.lowFreqAbsorption;
        lowFreqAbsorpSlider.GetComponentInChildren<TMP_Text>().text = lowFreqAbsorpSlider.value.ToString("F2");
        midFreqAbsorpSlider.value = RoomManager.Instance.Material.midFreqAbsorption;
        midFreqAbsorpSlider.GetComponentInChildren<TMP_Text>().text = midFreqAbsorpSlider.value.ToString("F2");
        highFreqAbsorpSlider.value = RoomManager.Instance.Material.highFreqAbsorption;
        highFreqAbsorpSlider.GetComponentInChildren<TMP_Text>().text = highFreqAbsorpSlider.value.ToString("F2");
        scatteringSlider.value = RoomManager.Instance.Material.scattering;
        scatteringSlider.GetComponentInChildren<TMP_Text>().text = scatteringSlider.value.ToString("F2");
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

    public void SOFAFileDropdownChanged(int index)
    {
        currentSOFAText.text = sofaFileDropdown.options[index].text;
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
    
    public void WallSliderEndDrag()
    {
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
        RenderManager.Instance.DistanceAttenuation = isOn;
        RenderManager.Instance.PersistRoom();
    }

    public void AirAbsorptionToggleChanged(bool isOn)
    {
        RenderManager.Instance.AirAbsorption = isOn;
        RenderManager.Instance.PersistRoom();
    }

    public void RenderMethodDropDownChanged(int index)
    {
        chosenMethod = (RenderMethod)index;
        RenderManager.Instance.SetRenderMethod(renderMethod: (RenderMethod)index);
    }

    public void RoomsInputFieldChanged(string input)
    {
        if (int.TryParse(input, out int amountOfRooms) && amountOfRooms < 20)
        {
            RenderManager.Instance.SetAmountOfRooms(amount: amountOfRooms);
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

    public void RoomFolderDropdownChanged(int index)
    {
        RenderManager.Instance.SetRoomPath(index: index);
        
        LoadRenderFolderDropdownData();
        renderFolderDropdown.RefreshShownValue();
    }

    public void RenderFolderDropdownChanged(int index)
    {
        // TODO: Change the render folder of interest
        RenderManager.Instance.SetRenderPath(index: index);
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

    public void AddRoomFolderButtonPressed()
    {
        RenderManager.Instance.CreateNewRoomFolder();
        LoadRoomFolderDropdownData();
        roomFolderDropdown.value = roomFolderDropdown.options.Count - 1;
        renderFolderDropdown.ClearOptions();
    }

    public void DeleteRoomFolderButtonPressed()
    {
        RenderManager.Instance.DeleteRoomFolder();
        LoadRoomFolderDropdownData();
        roomFolderDropdown.value = roomFolderDropdown.options.Count - 1;
    }

    public void AddRenderFolderButtonPressed()
    {
        RenderManager.Instance.CreateNewRenderFolder();
        LoadRenderFolderDropdownData();
        renderFolderDropdown.value = renderFolderDropdown.options.Count - 1;
    }

    public void DeleteRenderFolderButtonPressed()
    {
        RenderManager.Instance.DeleteRenderFolder();
        LoadRenderFolderDropdownData();
        renderFolderDropdown.value = renderFolderDropdown.options.Count - 1;
    }

    // Used for retrieving the index from the selected audio clip
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

    private void LoadRoomFolderDropdownData()
    {
        roomFolderDropdown.options.Clear();
        foreach (string directory in RenderManager.Instance.Directories)
        {
            int pathLength = RenderManager.roomsPath.Length;
            string modifiedString = directory[pathLength..];
            roomFolderDropdown.options.Add(new TMP_Dropdown.OptionData() { text = modifiedString });
        }
    }

    private void LoadRenderFolderDropdownData()
    {
        if (RenderManager.Instance.RenderPaths.Length != 0)
        {
            renderFolderDropdown.options.Clear();
            foreach (string renderFolder in RenderManager.Instance.RenderPaths)
            {
                int pathLength = RenderManager.Instance.SelectedRoomPath.Length;
                string modifiedString = renderFolder[pathLength..];
                renderFolderDropdown.options.Add(new TMP_Dropdown.OptionData() { text = modifiedString });
            }
        }
        else 
        {
            renderFolderDropdown.options.Clear();
        }
        
        renderFolderDropdown.RefreshShownValue();
    }

    public void DefaultLocationPressed()
    {
        RenderManager.Instance.GoToDefaultLocation();
    }

    public void LocationRandomiserPressed()
    {
        RenderManager.Instance.RandomiseLocation();
    }
}