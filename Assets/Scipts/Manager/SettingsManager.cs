using UnityEngine;
public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }
    public Settings settings;
    private bool _didStartUpComplete = false;

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

        settings = DataManager.Instance.LoadSettings();
    }

    public void Save()
    {
        DataManager.Instance.SaveSettings(settings);
    }

    public bool DidStartUpComplete 
    {
        get { return _didStartUpComplete ; }
        set { _didStartUpComplete = value; }
    }
}