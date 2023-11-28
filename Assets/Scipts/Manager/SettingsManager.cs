using UnityEngine;
public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }
    public Settings settings;

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

    private void Start()
    {
        settings = DataManager.Instance.LoadSettings();
    }

    public void Save()
    {
        DataManager.Instance.SaveSettings(settings);
    }
}