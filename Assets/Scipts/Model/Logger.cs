using System;
using System.IO;
using SteamAudio;

public class Logger
{
    private string LogPath { get { return SimulationManager.Instance.folderPath + "log.txt"; } }
    private string TimeStamp { get { return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); } }

    public void Log(Speaker speaker)
    {
        using StreamWriter writer = new(LogPath, true);

        WriteSpeakerInfo(writer, speakerInfo: speaker);

        writer.Close();
    }

    public void LogTitle()
    {
        using StreamWriter writer = new(LogPath, true);

        WriteTitle(writer);

        writer.Close();
    }

    private void WriteTitle(StreamWriter writer)
    {
        writer.WriteLine($"Simulation Log : { TimeStamp }");
        writer.WriteLine("// --------------------");
        // TODO - Make enum from room manager to get the room sizes
        writer.WriteLine("Room size : 20x16 units");
        writer.WriteLine($"Sample rate : { SimulationManager.Instance.SampleRate } Hz" );
    }

    private void WriteSpeakerInfo(StreamWriter writer, Speaker speakerInfo)
    {
        writer.WriteLine($"\nName : { speakerInfo.Name }");
        writer.WriteLine($"    Geometry : ");
        writer.WriteLine($"        Distance To Receiver : { speakerInfo.DistanceToReceiver } units");
        writer.WriteLine($"        Audio Clip : { speakerInfo.ClipName()}");
        writer.WriteLine($"        Azimuth angle : { speakerInfo.Azimuth }");
        writer.WriteLine($"        Elevation angle : { speakerInfo.Elevation }");
        
        writer.WriteLine($"\n    Parameters : ");
        writer.WriteLine($"        Real Time Bounces : { SteamAudioSettings.Singleton.realTimeBounces }");
        writer.WriteLine($"        Volume: { speakerInfo.Volume() }");
        writer.WriteLine($"        Direct Mix Level : { speakerInfo.DirectMixLevel() }");
        writer.WriteLine($"        Reflection Mix Level : { speakerInfo.ReflectionMixLevel() } \n");
        writer.WriteLine($"        HRTF Applied To Reflections? : { (speakerInfo.HRTFAppliedToReflection() ? "yes" : "no") }");
        writer.WriteLine($"        Distance Attenuation Applied : { (speakerInfo.DistanceAttenuation() ? "yes" : "no") }");
        writer.WriteLine($"        Air Absorption Applied : { (speakerInfo.AirAbsorption() ? "yes" : "no") }");
        writer.WriteLine("\n\n");
    }
}
