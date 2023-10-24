using System;
using System.IO;
using SteamAudio;

public class Logger
{
    readonly string logPath = "/Users/duyx/Code/Jabra/python/renders/log.txt";

    public void Log(Speaker speaker)
    {
        using StreamWriter writer = new(logPath, true);

        WriteSpeakerInfo(writer, speakerInfo: speaker);

        writer.Close();
    }

    public void LogTitle()
    {
        using StreamWriter writer = new(logPath, true);

        WriteTitle(writer);

        writer.Close();
    }

    private void WriteTitle(StreamWriter writer)
    {
        string timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        writer.WriteLine($"\nSimulation Log : { timeStamp }");
        writer.WriteLine("// --------------------");
    }

    private void WriteSpeakerInfo(StreamWriter writer, Speaker speakerInfo)
    {
        writer.WriteLine($"\nName : { speakerInfo.Name }");
        writer.WriteLine($"Distance To Receiver : { speakerInfo.DistanceToReceiver }");
        writer.WriteLine($"Audio Clip : { speakerInfo.ClipName()}");
        writer.WriteLine($"Azimuth angle : { speakerInfo.Azimuth }");
        writer.WriteLine($"Elevation angle : { speakerInfo.Elevation }");
        writer.WriteLine($"Real Time Bounces : { SteamAudioSettings.Singleton.realTimeBounces }");
        writer.WriteLine($"Volume: { speakerInfo.Volume() }");
        writer.WriteLine($"Direct Mix Level : { speakerInfo.DirectMixLevel() }");
        writer.WriteLine($"Reflection Mix Level : { speakerInfo.ReflectionMixLevel() } \n");
        writer.WriteLine($"HRTF Applied To Reflections? : { (speakerInfo.HRTFAppliedToReflection() ? "yes" : "no") }");
        writer.WriteLine($"Distance Attenuation Applied : { (speakerInfo.DistanceAttenuation() ? "yes" : "no") }");
        writer.WriteLine($"Air Absorption Applied : { (speakerInfo.AirAbsorption() ? "yes" : "no") }");
    }
}
