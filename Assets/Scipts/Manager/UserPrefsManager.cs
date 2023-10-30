using UnityEngine;

public class KeysEnum
{
    public static readonly string ApplyHRTFToReflections = "applyHRTFToReflections";
}

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

    public bool HRTFSettings 
    {
        get { return PlayerPrefs.GetInt(KeysEnum.ApplyHRTFToReflections) == 0; }
        set { PlayerPrefs.SetInt(KeysEnum.ApplyHRTFToReflections, value == true ? 0 : 1); }
    }
}
