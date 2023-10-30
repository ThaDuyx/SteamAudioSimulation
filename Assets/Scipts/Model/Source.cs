using System;

[Serializable]
public class Source
{
    public string name;
    public float volume;
    public float directMixLevel;
    public float reflectionMixLevel;

    public Source(string name, float volume, float directMixLevel, float reflectionMixLevel)
    {
        this.name = name;
        this.volume = volume;
        this.directMixLevel = directMixLevel;
        this.reflectionMixLevel = reflectionMixLevel;
    }
}
