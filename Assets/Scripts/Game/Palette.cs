using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable, CreateAssetMenu]
public class Palette : ScriptableObject
{
    public Color[] colors;

    public Color GetColor(int id)
    {
        id = Mathf.Clamp(id, 0, colors.Length);
        return colors[id];
    }
}
