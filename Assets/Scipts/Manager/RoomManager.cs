using UnityEngine;
using UnityEngine.SceneManagement;

enum Room 
{
    TestRoom,
    SmallRoom
}
public class RoomManager : MonoBehaviour
{
    // Singleton object
    public static RoomManager Instance { get; private set; }

    // [SerializeField] private GameObject[] rooms;
    // private GameObject selectedRoom;
    // private GameObject selectedSpeaker;
    // private List<GameObject> speakerArray;
    // private string speakerTag = "speaker";
    
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

    public void TestScene()
    {
        SceneManager.LoadScene(1);
    }
}
