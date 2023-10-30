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
}
