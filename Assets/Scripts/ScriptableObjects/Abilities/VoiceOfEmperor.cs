using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Voice Of Emperor")]
public class VoiceOfEmperor : Ability
{
    public override void Execute()
    {
        regions[0].Units += 10;
        Debug.Log("Император призвал войск");
    }
}
