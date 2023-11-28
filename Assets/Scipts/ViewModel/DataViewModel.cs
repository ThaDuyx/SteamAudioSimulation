using System;
using System.IO;

public class DataViewModel
{
    public string recordingPath;
    public string selectedRenderPath;
    public string selectedRoomPath;
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

    public void CreateNewRoomFolder() 
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
}