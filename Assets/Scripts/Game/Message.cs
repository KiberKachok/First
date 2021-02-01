using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Message : ScriptableObject
{
    public int id;
    public Sprite icon;
    public string title;
    public string text;
    public Color color;
}
