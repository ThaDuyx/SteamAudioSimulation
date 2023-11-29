using System;

[Serializable]
public class Settings 
{
    public int selectedSOFA;
    public string selectedRoomDirectory, selectedRenderDirectory, selectedRenderMethod;

    public Settings(string selectedRoomDirectory, string selectedRenderDirectory, int selectedSOFA, string selectedRenderMethod) 
    {
        this.selectedRoomDirectory = selectedRoomDirectory;
        this.selectedRenderDirectory = selectedRenderDirectory;
        this.selectedSOFA = selectedSOFA;
        this.selectedRenderMethod = selectedRenderMethod;
    }
}