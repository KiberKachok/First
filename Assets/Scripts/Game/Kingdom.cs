using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Photon.Realtime;
using UnityEngine;

[CanBeNull, Serializable]
public class Kingdom
{
    public int id;
    public string name;
    public string hash;
    public Color color;

    public Kingdom(int id, string name, string hash, Color color)
    {
        this.id = id;
        this.name = name;
        this.hash = hash;
        this.color = color;
    }

    public override string ToString()
    {
        return "Kingdom " + id;
    }
}
