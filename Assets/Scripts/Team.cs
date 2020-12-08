using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Photon.Realtime;
using UnityEngine;
using Random = UnityEngine.Random;

public class Team : ScriptableObject
{
    public string ownerHash;
    public Race race;
    public Color regionColor;
    public void Init(Player owner, Race race)
    {
        ownerHash = owner.GetHash();
        this.race = race;
        name = race.header;
        regionColor = race.regionColor;
    }
}
