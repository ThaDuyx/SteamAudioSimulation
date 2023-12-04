using System;

[Serializable]
public class Settings 
{
    public int selectedSOFA, reflectionBounce;
    public string selectedRoomDirectory, selectedRenderDirectory, selectedRenderMethod;
    public float lowFreqAbsorption, midFreqAbsorption, highFreqAbsorption, scattering;

    public Settings(string selectedRoomDirectory, string selectedRenderDirectory, int selectedSOFA, string selectedRenderMethod, int reflectionBounce, float lowFreqAbsorption, float midFreqAbsorption, float highFreqAbsorption, float scattering)
    {
        this.selectedRoomDirectory = selectedRoomDirectory;
        this.selectedRenderDirectory = selectedRenderDirectory;
        this.selectedSOFA = selectedSOFA;
        this.selectedRenderMethod = selectedRenderMethod;
        this.reflectionBounce = reflectionBounce;
        this.lowFreqAbsorption = lowFreqAbsorption;
        this.midFreqAbsorption = midFreqAbsorption;
        this.highFreqAbsorption = highFreqAbsorption;
        this.scattering = scattering;
    }
}