using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;


public class Ability : ScriptableObject
{
    public string header;
    public List<Region> regions;
    public string[] regionsRequirement;

    public virtual void Execute()
    {
        Destroy(this);
    }

    public bool Add(Region region)
    {
        if (RegionHelper.isSuit(region, regionsRequirement[regions.Count],
            regions, out string comment))
        {
            regions.Add(region);
            return IsReady();
        }

        return false;
    }

    public bool IsReady()
    {
        return (regions.Count == regionsRequirement.Length);
    }
}
