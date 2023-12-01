using System;
using System.Collections.Generic;
using System.IO;
using SteamAudio;

public class DataViewModel
{
    private string selectedRenderPath;
    private string selectedRoomPath;
    public string RecordingPath { get { return selectedRenderPath; }}
    public string recordingPath;
    public int selectedUserIndex;
    public string[] Directories { get { return Directory.GetDirectories(Paths.roomsPath); }}
    public string[] RenderPaths { get { return Directory.GetDirectories(selectedRoomPath); }}
    
    public DataViewModel()
    {
        selectedRoomPath = SettingsManager.Instance.settings.selectedRoomDirectory;
        selectedRenderPath = SettingsManager.Instance.settings.selectedRenderDirectory;
    }
    
    public void SetRecordingPath(RenderMethod renderMethod)
    {
        if (renderMethod == RenderMethod.RenderRooms)
        {
            recordingPath = selectedRenderPath;
        }
        else if (renderMethod == RenderMethod.RenderUser)
        {
            recordingPath = selectedRoomPath + "/user" + selectedUserIndex.ToString() + "/";
        }
        else 
        {
            string timeStamp = DateTime.Now.ToString("ddMM-yy_HHmmss");
            recordingPath = Paths.folderPath + timeStamp + "/";
            System.IO.Directory.CreateDirectory(recordingPath);
        }
    }

    public void CreateRootRenderFolder()
    {
        int folderCount = Directory.GetDirectories(Paths.roomsPath).Length;
        selectedRoomPath = Paths.roomsPath + "render" + folderCount.ToString() + "/";
        System.IO.Directory.CreateDirectory(selectedRoomPath);
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