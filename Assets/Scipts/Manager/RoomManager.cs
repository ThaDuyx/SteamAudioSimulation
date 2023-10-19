using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    // Singleton object
    public static RoomManager Instance { get; private set;}

    [SerializeField] private GameObject[] rooms;
    private GameObject selectedRoom;
    private GameObject selectedSpeaker;
    private List<GameObject> speakerArray;

    // private string speakerTag = "speaker";
    private bool _isPreviewing = false;

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
    
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public bool IsPreviewing()
    {
        return _isPreviewing;
    }
}
