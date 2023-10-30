using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomManager : MonoBehaviour
{
    // Singleton object
    public static RoomManager Instance { get; private set; }

    public int SceneCounter { get { return SceneManager.sceneCountInBuildSettings; } }
    public int ActiveSceneIndex { get { return SceneManager.GetActiveScene().buildIndex; } }
    public int activeRoom = 1;

    private void Awake()
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

    public void ChangeScene(int sceneIndexInBuildSettings)
    {
        activeRoom = sceneIndexInBuildSettings;
        SceneManager.LoadScene(sceneIndexInBuildSettings);
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
        if (File.Exists(Application.persistentDataPath + "/roomData/room" + ActiveSceneIndex.ToString() + ".json"))
        {
            string jsonData = File.ReadAllText(Application.persistentDataPath + "/roomData/room" + ActiveSceneIndex.ToString() + ".json");
            Room loadedRoomData = JsonUtility.FromJson<Room>(jsonData);
            
            return loadedRoomData;
        }
        else 
        {
            // Make default room data
            List<Source> sources = new();
            for (int i = 0; i < amountOfSpeakers; i++)
            {
                sources.Add(new Source(name: "speaker" + (i+1).ToString(), volume: 0.091f, directMixLevel: 0.2f, reflectionMixLevel: 1.0f));
            }
            Room defaultRoom = new("room" + ActiveSceneIndex.ToString(), ActiveSceneIndex, sources);    
            
            // Serialize & write the data
            string defaultRoomJson = JsonUtility.ToJson(defaultRoom, true);
            File.WriteAllText(Application.persistentDataPath + "/roomData/room" + ActiveSceneIndex.ToString() + ".json", defaultRoomJson);

            return defaultRoom;            
        }
    }
}
