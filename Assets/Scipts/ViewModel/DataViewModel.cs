using System;
using System.Collections.Generic;
using System.IO;
using SteamAudio;

public class DataViewModel
{
    private string selectedRenderPath;
    private string selectedRoomPath;
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

    public void CreateDirectories() 
    {
        int folderCount = Directory.GetDirectories(Paths.roomsPath).Length;
        System.IO.Directory.CreateDirectory(Paths.roomsPath + "render" + folderCount.ToString() + "/");
        selectedRoomPath = Paths.roomsPath + "render" + folderCount.ToString();
        
        // TODO: Insert the correct amount
        CreateFoldersForRenders(amountOfSpeakers: RenderManager.Instance.sourceVM.AmountOfSpeakers);
    }

    private void CreateFoldersForRenders(int amountOfSpeakers)
    {
        for (int i = 0; i <= amountOfSpeakers - 1; i++)
        {
            int folderCount = Directory.GetDirectories(selectedRoomPath).Length;
            selectedRenderPath = selectedRoomPath + "/" + "inroom" + folderCount.ToString() + "/";
            System.IO.Directory.CreateDirectory(selectedRenderPath);
        }
        
        selectedUserIndex = Calculator.RandomiseIndex();
        string userPath = selectedRoomPath + "/user" + selectedUserIndex.ToString() + "/";
        System.IO.Directory.CreateDirectory(userPath);

        selectedRenderPath = selectedRoomPath + "/" + "inroom0" + "/";
    }
    public void CreateRootRenderFolder()
    {
        int folderCount = Directory.GetDirectories(Paths.roomsPath).Length;
        System.IO.Directory.CreateDirectory(Paths.roomsPath + "render" + folderCount.ToString() + "/");
    }

    public void CreateFarFieldRenderFolder(int activeRoom)
    {
        selectedRenderPath = selectedRoomPath + "/inroom" + activeRoom.ToString() + "/";
    }

    public void CreateNearFieldRenderFolder()
    {
        selectedUserIndex = Calculator.RandomiseIndex();
        string userPath = selectedRoomPath + "/user" + selectedUserIndex.ToString() + "/";
        System.IO.Directory.CreateDirectory(userPath);

        selectedRenderPath = selectedRoomPath + "/" + "inroom0" + "/";
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

    public List<int> FetchUserSOFAIndicies()
    {
        return SteamAudioManager.Singleton.GetUserSOFAIndices(selectedUserIndex);
    }
}