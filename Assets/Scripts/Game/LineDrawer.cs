using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Shapes;
using UnityEngine;
using UnityEngine.Rendering;

public class LineDrawer : MonoBehaviour
{
    [Space(10)]
    public float linePadding = 0.5f;
    public DashedLineStyle mainStyle;
    public DashedLineStyle common;
    public List<Vector3[]> lines = new List<Vector3[]>();

    private GameCore _gameCore;

    private void Start()
    {
        _gameCore = GameCore.main;
    }

    public void BuildLines()
    {
        if (_gameCore.SelectedRegion)
        {
            Vector3 selPos = _gameCore.SelectedRegion.transform.position;
            if (_gameCore.EndRegion)
            {
                lines = new List<Vector3[]>();
                Vector3 lineDirection = (_gameCore.EndRegion.transform.position - selPos).normalized * linePadding;
                lines.Add(new []{selPos + lineDirection, _gameCore.EndRegion.transform.position - lineDirection});
            }
            else
            {
                lines = new List<Vector3[]>();
                Vector3[] neiPos = _gameCore.SelectedRegion.neighbours.Select(p => p.transform.position).ToArray();
                
                for (int i = 0; i < neiPos.Length; i++)
                {
                    Vector3 lineDirection = (neiPos[i] - selPos).normalized * linePadding;
                    lines.Add(new []{selPos + lineDirection, neiPos[i] - lineDirection});
                }
            }
        }
        else
        {
            lines = new List<Vector3[]>();
        }
    }
    
    void OnEnable()
    {
        RenderPipelineManager.endCameraRendering += RenderPipelineManager_endCameraRendering;
    }
    void OnDisable()
    {
        RenderPipelineManager.endCameraRendering -= RenderPipelineManager_endCameraRendering;
    }
    private void RenderPipelineManager_endCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        OnPostRender();
    }
    
    private void OnPostRender()
    {
        DashedLineStyle lineStyle = _gameCore.EndRegion ? common : mainStyle;
        
        foreach (var line in lines)
        {
            Draw.LineDashStyle.type = DashType.Basic;
            Draw.LineGeometry = lineStyle.lineGeometry;
            Draw.LineDashStyle.offset = (Time.time * lineStyle.offsetSpeed) % 1;
            Draw.LineDashStyle.size = lineStyle.dashSize;
            Draw.LineDashStyle.spacing = lineStyle.spaceSize;
            Draw.LineEndCaps = LineEndCap.Square;
            Draw.ZTest = lineStyle.compareFunction;
            Draw.LineDashed(line[0], line[1], lineStyle.thickness, lineStyle.color);
        }
    }
}

[Serializable]
public class LineStyle
{
    public float thickness = 0.01f;
    public Color color;
    public LineGeometry lineGeometry;
    public CompareFunction compareFunction;
}

[Serializable]
public class DashedLineStyle : LineStyle
{
    public float dashSize = 0.01f;
    public float spaceSize = 0.01f;
    public float offsetSpeed;
}