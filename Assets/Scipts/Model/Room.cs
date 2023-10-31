using System;
using System.Collections.Generic;

[Serializable]
public class Room
{
    public string name;
    public int index;
    public List<Source> sources;

    public Room(string name, int index, List<Source> sources) 
    {
        this.name = name;
        this.index = index;
        this.sources = sources;
    }
}

