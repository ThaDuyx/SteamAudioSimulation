using System.IO;
using UnityEngine;

public class UserPrefsManager : MonoBehaviour
{
     
    public static UserPrefsManager Instance { get; private set; }

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
}