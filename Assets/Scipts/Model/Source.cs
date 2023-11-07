using System;

[Serializable]
public class Source
{
    public string name;
    public float volume;
    public float directMixLevel;
    public float reflectionMixLevel;
    public string audioClip;
    public int applyHRTFToReflections;

    public Source(string name, float volume, float directMixLevel, float reflectionMixLevel, string audioClip, int applyHRTFToReflections)
    {
        this.name = name;
        this.volume = volume;
        this.directMixLevel = directMixLevel;
        this.reflectionMixLevel = reflectionMixLevel;
        this.audioClip = audioClip;
        this.applyHRTFToReflections = applyHRTFToReflections;
    }
}
