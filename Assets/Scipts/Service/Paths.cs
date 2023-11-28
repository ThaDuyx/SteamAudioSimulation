using System.IO;
struct Paths 
{
    // Should be modified for specific needs - TODO: Change to dynamic folder structure
    public static string defaultClipName = "sweep_48kHz";
    public static string folderPath = "/Users/duyx/Code/Jabra/python/renders/";
    public static string roomsPath = "/Users/duyx/Code/Jabra/python/renders/rooms/";

    public static void SetupDirectories()
    {
        if (!Directory.Exists(Paths.folderPath)) { System.IO.Directory.CreateDirectory(Paths.folderPath); }
        if (!Directory.Exists(Paths.roomsPath)) { System.IO.Directory.CreateDirectory(Paths.roomsPath); }
    }
}