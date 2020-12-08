using System;
using System.Collections;
using System.Collections.Generic;
using Shapes;
using UnityEngine;

public static class RegionHelper
{
    //team:Our/Enemy
    
    public static bool isSuit(Region region, string expression, List<Region> all, out string comment)
    {
        string[] requirements = expression.Split('|');
        bool isSuitable = true;

        comment = "Соответствует условиям";
        
        foreach (var s in requirements)
        {
            if (s.StartsWith("team"))
            {
                string exp = s.Substring(5);

                if (exp.StartsWith("Our"))
                {
                    if (region.Team != GameManager.main.ownTeam)
                    {
                        isSuitable = false;
                        comment = "Выбран не ваш регион";
                        break;
                        
                    }
                }
                else
                {
                    if (region.Team == GameManager.main.ownTeam)
                    {
                        isSuitable = false;
                        comment = "Выбран ваш регион";
                        break;
                        
                    }
                }
            }
        }

        return isSuitable;
    }
}

public enum CellType
{
    Land,
    Water
}