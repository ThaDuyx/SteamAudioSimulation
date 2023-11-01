using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class DataManager : MonoBehaviour
{
    // Singleton object
    public static DataManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        { 
            Destroy(this); 
        }
        else 
        { 
            Instance = this; 
        }
    }

    // Write data from to a persisted room
    public void SaveRoomData(Room room)
    {
        string json = JsonUtility.ToJson(room, true);

        try 
        {
            File.WriteAllText(Application.persistentDataPath + "/roomData/room" + room.index.ToString() + ".json", json);
        }
        catch 
        {
            Debug.LogError("Failed saving room data");
        }
    }

    // Fetch data from a persisted room or create a default setting if it doesn't exist
    public Room LoadRoomData(int amountOfSpeakers)
    {
        int activeRoomIndex = SceneManager.GetSceneAt(1).buildIndex;

        if (File.Exists(Application.persistentDataPath + "/roomData/room" + activeRoomIndex.ToString() + ".json"))
        {
            string jsonData = File.ReadAllText(Application.persistentDataPath + "/roomData/room" + activeRoomIndex.ToString() + ".json");
            Room loadedRoomData = JsonUtility.FromJson<Room>(jsonData);
            
            return loadedRoomData;
        }
        else
        {
            // Make default room data
            List<Source> sources = new();
            for (int i = 0; i < amountOfSpeakers; i++)
            {
                sources.Add(new Source(name: "speaker" + (i + 1).ToString(), volume: 0.091f, directMixLevel: 0.2f, reflectionMixLevel: 1.0f, audioClip: "sweep_48kHz"));
            }
            
            Room defaultRoom = new("room" + activeRoomIndex.ToString(), activeRoomIndex, sources);    
            
            // Serialize & write the data
            string defaultRoomJson = JsonUtility.ToJson(defaultRoom, true);
            File.WriteAllText(Application.persistentDataPath + "/roomData/room" + activeRoomIndex.ToString() + ".json", defaultRoomJson);

            return defaultRoom;            
        }
    }

    public List<string> GetAudioClips()
    {
        string audioClipsPath = Path.Combine(Application.dataPath, "Plugins/SteamAudio/Resources");

        if (Directory.Exists(audioClipsPath))
        {
            string[] audioClipFilePaths = Directory.GetFiles(audioClipsPath);

            List<string> audioClips = new();

            foreach (string audioClipFilePath in audioClipFilePaths)
            {
                string audioClip = Path.GetFileName(audioClipFilePath);
                if (FileIsWAVorMP3(audioClip))
                {
                    audioClips.Add(audioClip);
                }
            }

            return audioClips;
        }
        else 
        {
            return new List<string>();
        }
    }

    // Ensures that our file is WAV or MP3 since Unity creates .meta files when importing audio files from the Assets folder
    private bool FileIsWAVorMP3(string fileName) 
    {
        return fileName.EndsWith(".wav", System.StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".mp3", System.StringComparison.OrdinalIgnoreCase);
    }
}