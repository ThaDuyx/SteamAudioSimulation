using System;
using System.Collections.Generic;

[Serializable]
public class RoomData
{
    public string name;
    public int index;
    public List<SourceData> sources;

    public RoomData(string name, int index, List<SourceData> sources) 
    {
        this.name = name;
        this.index = index;
        this.sources = sources;
    }
}

