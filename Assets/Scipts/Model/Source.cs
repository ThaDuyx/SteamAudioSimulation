using System;
using Unity.VisualScripting.Dependencies.Sqlite;

[Serializable]
public class Source
{
    public string name;
    public float volume;
    public float directMixLevel;
    public float reflectionMixLevel;
    public string audioClip;

    public Source(string name, float volume, float directMixLevel, float reflectionMixLevel, string audioClip)
    {
        this.name = name;
        this.volume = volume;
        this.directMixLevel = directMixLevel;
        this.reflectionMixLevel = reflectionMixLevel;
        this.audioClip = audioClip;
    }
}
