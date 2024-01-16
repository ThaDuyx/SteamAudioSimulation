using System;
using System.IO;
using SteamAudio;

struct Logger
{
    private static string LogPath { get { return RenderManager.Instance.dataVM.RecordingPath + "log.txt"; } }
    private static string TimeStamp { get { return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); } }

    public static void Log(Source source)
    {
        using StreamWriter writer = new(LogPath, true);

        WriteSpeakerInfo(writer, source: source);

        writer.Close();
    }

    public static void LogTitle()
    {
        using StreamWriter writer = new(LogPath, true);

        WriteTitle(writer);

        writer.Close();
    }

    private static void WriteTitle(StreamWriter writer)
    {
        writer.WriteLine($"Simulation Log : { TimeStamp }");
        writer.WriteLine("// --------------------");
        writer.Write($"Room name : room{ RoomManager.Instance.ActiveSceneIndex }");
        
        // TODO - Make enum from room manager to get the room sizes

        writer.WriteLine($"\nLow Freq. Absorption : { RoomManager.Instance.Material.lowFreqAbsorption }");
        writer.WriteLine($"Mid Freq. Absorption : { RoomManager.Instance.Material.midFreqAbsorption }");
        writer.WriteLine($"High Freq. Absorption : { RoomManager.Instance.Material.highFreqAbsorption }");
        writer.WriteLine($"Scattering : { RoomManager.Instance.Material.scattering }");
        // ------
        
        writer.WriteLine($"\nSample rate : { UnityEngine.AudioSettings.outputSampleRate } Hz" );
    }

    private static void WriteSpeakerInfo(StreamWriter writer, Source source)
    {
        writer.WriteLine($"\nName : { source.name }");
        writer.WriteLine($"        Audio Clip : { source.audioSource.clip.name }");
        writer.WriteLine($"    Geometry : ");
        writer.WriteLine($"        Distance To Receiver : { source.distanceToReceiver } units");
        writer.WriteLine($"        Azimuth angle : { source.azimuth }");
        writer.WriteLine($"        Elevation angle : { source.elevation }");
        
        writer.WriteLine($"\n    Parameters : ");
        writer.WriteLine($"        Real Time Bounces : { SteamAudioSettings.Singleton.realTimeBounces }");
        writer.WriteLine($"        Volume: { source.audioSource.volume }");
        writer.WriteLine($"        Direct Mix Level : { source.steamAudioSource.directMixLevel }");
        writer.WriteLine($"        Reflection Mix Level : { source.steamAudioSource.reflectionsMixLevel } \n");
        writer.WriteLine($"        HRTF Applied To Reflections? : { (source.steamAudioSource.applyHRTFToReflections ? "yes" : "no") }");
        writer.WriteLine($"        Distance Attenuation Applied : { (source.steamAudioSource.distanceAttenuation ? "yes" : "no") }");
        writer.WriteLine($"        Air Absorption Applied : { (source.steamAudioSource.airAbsorption ? "yes" : "no") }");
        writer.WriteLine("\n\n");
    }
}
