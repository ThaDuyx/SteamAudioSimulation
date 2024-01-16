using System.Collections.Generic;
using SteamAudio;

public static class SteamAudioManagerExtensions
{
    public static int SOFACount(this SteamAudioManager steamAudioManager) { return steamAudioManager.hrtfNames.Length; }
    public static string ActiveSOFAName(this SteamAudioManager steamAudioManager) { return steamAudioManager.hrtfNames[steamAudioManager.currentHRTF]; }
    public static bool IsLastSOFA(this SteamAudioManager steamAudioManager) { return steamAudioManager.currentHRTF == 5; } // hardcoded as 2 for now - this is used for only rendering the far field sofa files
    public static List<int> GetUserSOFAIndices(this SteamAudioManager steamAudioManager, int index)
    {
        List<int> indices = new();

        for (int i = 0; i < steamAudioManager.SOFACount(); i++)
        {
            if (steamAudioManager.hrtfNames[i].Contains("config_" + index.ToString()))
            {
                indices.Add(i);
            }
        }

        return indices;
    }
}