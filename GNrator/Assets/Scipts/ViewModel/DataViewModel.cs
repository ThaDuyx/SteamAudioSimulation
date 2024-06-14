using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using SteamAudio;
using UnityEngine;

public class DataViewModel
{
    public string[] Directories { get { return Directory.GetDirectories(Paths.roomsPath); }}
    public string[] RenderPaths { get { return Directory.GetDirectories(selectedRoomPath); }}
    public string RecordingPath { get { return selectedRenderPath; }}
    private string selectedRenderPath, selectedRoomPath;
    private int selectedUserIndex;
    
    public DataViewModel()
    {
        selectedRoomPath = SettingsManager.Instance.settings.selectedRoomDirectory;
        selectedRenderPath = SettingsManager.Instance.settings.selectedRenderDirectory;
    }

    public void CreateRootRenderFolder()
    {
        int folderCount = Directory.GetDirectories(Paths.roomsPath).Length;
        selectedRoomPath = Paths.roomsPath + "render" + folderCount.ToString() + "/";
        System.IO.Directory.CreateDirectory(selectedRoomPath);
        UnityEngine.Debug.Log(selectedRoomPath);
    }

    public void CreateFarFieldRenderFolder(int numberOfRender)
    {
        selectedRenderPath = selectedRoomPath + "inroom" + numberOfRender.ToString() + "/";
        System.IO.Directory.CreateDirectory(selectedRenderPath);
    }

    public void CreateNearFieldRenderFolder()
    {
        selectedUserIndex = Calculator.RandomiseIndex();
        selectedRenderPath = selectedRoomPath + "user" + selectedUserIndex.ToString() + "/";
        System.IO.Directory.CreateDirectory(selectedRenderPath);
    }

    public void SetRenderPath(int index)
    {
        selectedRenderPath = RenderPaths[index] + "/";
        SettingsManager.Instance.settings.selectedRenderDirectory = selectedRenderPath;

        SettingsManager.Instance.Save();
    }

    public void UpdateRenderPath()
    {
        selectedRenderPath = selectedRoomPath + "/" + "inroom" + RenderManager.Instance.SelectorIndex + "/";
    }

    public int FetchUserSOFAIndex(int index)
    {
        List<int> userIndicies = SteamAudioManager.Singleton.GetUserSOFAIndices(selectedUserIndex);
        return userIndicies[index];
    }
}