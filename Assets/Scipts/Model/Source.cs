using System;

[Serializable]
public class Source
{
    public string name, audioClip;
    public float volume, directMixLevel, reflectionMixLevel;
    public int applyHRTFToReflections, airAbsorption, distanceAttenuation;

    public Source(string name, float volume, float directMixLevel, float reflectionMixLevel, string audioClip, int applyHRTFToReflections, int airAbsorption, int distanceAttenuation)
    {
        this.name = name;
        this.volume = volume;
        this.directMixLevel = directMixLevel;
        this.reflectionMixLevel = reflectionMixLevel;
        this.audioClip = audioClip;
        this.applyHRTFToReflections = applyHRTFToReflections;
        this.airAbsorption = airAbsorption;
        this.distanceAttenuation = distanceAttenuation;
    }
}
