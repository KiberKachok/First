using System.Collections;
using System.Collections.Generic;
using Shapes;
using UnityEngine;
using UnityEngine.Rendering;

public class LineDrawer : MonoBehaviour
{
    public Dictionary<Vector3, Vector3> lines = new Dictionary<Vector3, Vector3>();
    [Space(10)]
    public float linePadding = 0.5f;
    public DashedLineStyle mainStyle;
    public DashedLineStyle common;

    public void RecalculateLines()
    {
        Dictionary<Vector3, Vector3> _lines = new Dictionary<Vector3, Vector3>();
        Region selectedRegion = GameHandler.main.SelectedRegion;
        Region endRegion = GameHandler.main.EndRegion;
        if (selectedRegion)
        {
            if (endRegion)
            {
                Vector3 lineDirection = (endRegion.transform.position - selectedRegion.transform.position).normalized * linePadding;
                _lines.Add(selectedRegion.transform.position + lineDirection, endRegion.transform.position - lineDirection);
            }
            else
            {
                foreach (var n in selectedRegion.neighbours.Keys)
                {
                    Vector3 lineDirection = (selectedRegion.transform.position - n.transform.position).normalized * linePadding;
                    _lines.Add(n.transform.position + lineDirection, selectedRegion.transform.position - lineDirection);
                }   
            }
        }
        
        lines = _lines;
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
        DashedLineStyle lineStyle = GameHandler.main.EndRegion ? mainStyle : common;
        
        if(lines.Count > 0 && GameHandler.main.SelectedRegion.Team == GameManager.main.ownTeam)
            foreach (var line in lines)
            {
                Draw.LineDashStyle.type = DashType.Basic;
                Draw.LineDashStyle.offset = Time.time * lineStyle.offsetSpeed;
                Draw.LineDashStyle.size = lineStyle.dashSize;
                Draw.LineDashStyle.spacing = lineStyle.spaceSize;
                Draw.LineEndCaps = LineEndCap.Square;
                Draw.LineDashed(line.Value, line.Key, lineStyle.thickness, lineStyle.color);
            }
    }
}
