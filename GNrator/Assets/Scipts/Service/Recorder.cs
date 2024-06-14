using System;
using System.IO;
using System.Text.RegularExpressions;
using SteamAudio;
using UnityEngine;

// Reference: http://abolfazltanha.com/Source-codes-92045/
// Youtube link: https://www.youtube.com/watch?v=tI0PuFlfMfI
public class Recorder
{
    private readonly int headerSize = 44; // Default for uncompressed wav
    public bool IsRecording { get; private set; }
    private FileStream fileStream;
    private float[] tempDataSource;
    private readonly int outputSampleRate;
    private string fileName;

    public Recorder(int outputSampleRate)
    {
        this.outputSampleRate = outputSampleRate;
    }

    public void ToggleRecording()
    {
        if (IsRecording)
        {
            StopRecording();
        }
        else 
        {   
            StartRecording();
        }
    }

    public void StartRecording() 
    {
        fileName = ExtractFileName();

        if (!IsRecording)
        {
            StartWriting(fileName);
            IsRecording = true;
        }
        else 
        {
            Debug.Log("Recording is in progress");
        }
    }

    public void StopRecording()
    {
        IsRecording = false;
        WriteHeader();
    }

    private void StartWriting(string name) 
    {
        fileStream = new FileStream(RenderManager.Instance.dataVM.RecordingPath + fileName, FileMode.Create);        
        // fileStream = new FileStream("/Users/duyx/Code/Jabra/python/renders/" + fileName, FileMode.Create);
        // fileStream = new FileStream(Application.persistentDataPath + "/" + fileName, FileMode.Create);
        
        var emptyByte = new byte();
        for (int i = 0; i < headerSize; i++) // Preparing wav header
        {
            fileStream.WriteByte(emptyByte);
        }
    }
    
    public void ConvertAndWrite(float[] dataSource)
    {
        var intData = new Int16[dataSource.Length];
        var bytesData = new Byte[dataSource.Length * 2];
        var rescaleFactor = 32767;
        for (var i = 0; i < dataSource.Length; i++)
        {
            intData[i] = (Int16)(dataSource[i] * rescaleFactor);
            var byteArr = new Byte[2];
            byteArr = BitConverter.GetBytes(intData[i]);
            byteArr.CopyTo(bytesData, i * 2);
        }
        fileStream.Write(bytesData, 0, bytesData.Length);
        tempDataSource = new float[dataSource.Length];
        tempDataSource = dataSource;
    }
    
    private void WriteHeader()
    {
        fileStream.Seek(0, SeekOrigin.Begin);
        var riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
        fileStream.Write(riff, 0, 4);
        var chunkSize = BitConverter.GetBytes(fileStream.Length - 8);
        fileStream.Write(chunkSize, 0, 4);
        var wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
        fileStream.Write(wave, 0, 4);
        var fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
        fileStream.Write(fmt, 0, 4);
        var subChunk1 = BitConverter.GetBytes(16);
        fileStream.Write(subChunk1, 0, 4);
        UInt16 two = 2;
        UInt16 one = 1;
        var audioFormat = BitConverter.GetBytes(one);
        fileStream.Write(audioFormat, 0, 2);
        var numChannels = BitConverter.GetBytes(two);
        fileStream.Write(numChannels, 0, 2);
        var sampleRate = BitConverter.GetBytes(outputSampleRate);
        fileStream.Write(sampleRate, 0, 4);
        var byteRate = BitConverter.GetBytes(outputSampleRate * 4);
        fileStream.Write(byteRate, 0, 4);
        UInt16 four = 4;
        var blockAlign = BitConverter.GetBytes(four);
        fileStream.Write(blockAlign, 0, 2);
        UInt16 sixteen = 16;
        var bitsPerSample = BitConverter.GetBytes(sixteen);
        fileStream.Write(bitsPerSample, 0, 2);
        var dataString = System.Text.Encoding.UTF8.GetBytes("data");
        fileStream.Write(dataString, 0, 4);
        var subChunk2 = BitConverter.GetBytes(fileStream.Length - headerSize);
        fileStream.Write(subChunk2, 0, 4);
        fileStream.Close();
    }

    private string ExtractFileName()
    {
        string sofaFile = SteamAudioManager.Singleton.ActiveSOFAName();
        string micPairIndicator;
        
        if (RenderManager.Instance.SelectedRenderMethod == RenderMethod.NearField)
        {
            micPairIndicator = Regex.Replace(sofaFile, "[^0-9]", "")[1..].Insert(1, "_");
        } 
        else
        {
            micPairIndicator = Regex.Replace(sofaFile, "[^0-9]", "").Insert(1, "_");
        }
        
        string concatenatedString = "mic_" + micPairIndicator;

        return Path.GetFileNameWithoutExtension(concatenatedString) + ".wav";
    }
}