using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LineStyle
{
    public float thickness = 0.01f;
    public Color color;
}

[Serializable]
public class DashedLineStyle : LineStyle
{
    public float dashSize = 0.01f;
    public float spaceSize = 0.01f;
    public float offsetSpeed;
}