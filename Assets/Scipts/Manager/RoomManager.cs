using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomManager : MonoBehaviour
{
    // Singleton object
    public static RoomManager Instance { get; private set; }

    public delegate void SceneUnloadedAction();
    public static event SceneUnloadedAction OnSceneUnloaded;

    public int SceneCounter { get { return SceneManager.sceneCountInBuildSettings; } }
    public int ActiveSceneIndex { get { return SceneManager.GetSceneAt(1).buildIndex; } }

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
        // Unload previous scene asynchronously 
        StartCoroutine(UnloadActiveScene()); 
        
        // Load the newly selected scene
        SceneManager.LoadScene(sceneIndexInBuildSettings, LoadSceneMode.Additive);
    }

    private IEnumerator UnloadActiveScene()
    {
        // Start task of unloading with the current scene index. 
        var progress = SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(SceneManager.sceneCount - 1));
        
        // Waiting for task is completed
        while (!progress.isDone)
        {
            yield return null;
        }
        
        // Calling call-back function used on the view
        OnSceneUnloaded?.Invoke();
    }
}
