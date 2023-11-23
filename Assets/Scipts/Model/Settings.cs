public class Settings 
{
    public string selectedRoomDirectory;
    public string selectedRenderDirectory;
    public int selectedSOFA;
    public string selectedRenderMethod;

    public Settings(string selectedRoomDirectory, string selectedRenderDirectory, int selectedSOFA, string selectedRenderMethod) 
    {
        this.selectedRoomDirectory = selectedRoomDirectory;
        this.selectedRenderDirectory = selectedRenderDirectory;
        this.selectedSOFA = selectedSOFA;
        this.selectedRenderMethod = selectedRenderMethod;
    }
}