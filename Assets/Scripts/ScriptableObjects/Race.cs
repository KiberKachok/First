using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[CreateAssetMenu(menuName = "Race"), Serializable]
public class Race : ScriptableObject
{
    public string header;
    public Color regionColor;
    public List<Ability> abilities = new List<Ability>();

    public int regionMaxLevel = 4;
    public int[] regionUpgradePrice;
    public int[] regionUpgradeMaxUnits;
    public float[] regionUpgradeUnitsGrowSpeed;
    public float[] regionUpgradeDefenceCoefficient;

    public int portBuildPrice = 25;
}
