using System;
using System.Collections;
using System.Collections.Generic;
using Shapes;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class GameHandler : MonoBehaviour
{
    public Region SelectedRegion
    {
        get
        {
            return selectedRegion;
        }
        set
        {
            selectedRegion = value;
            lines = RecalculateLines();
        }
    }
    [SerializeField] private Region selectedRegion;
    
    public Region EndRegion
    {
        get
        {
            return endRegion;
        }
        set
        {
            endRegion = value;
            sendUnitsPanel.SetActive(endRegion != null);
            if (endRegion) sendUnitsSlider.value = 1f;
            lines = RecalculateLines();
        }
    }
    [SerializeField] private Region endRegion;

    public Ability activeAbility;
    
    public GameObject sendUnitsPanel;
    public Slider sendUnitsSlider;
    public float sendUnitsPercent;

    Dictionary<Vector3, Vector3> lines = new Dictionary<Vector3, Vector3>();
    [Space(10)]
    public float linePadding = 0.5f;
    public DashedLineStyle main;
    public DashedLineStyle common;

    void Start()
    {
        sendUnitsPercent = sendUnitsSlider.value;
    }
    
    public void AgreeSendUnits()
    {
        Debug.Log("Отправить войска");
        if (SelectedRegion && EndRegion)
        {
            SendUnits(SelectedRegion, EndRegion, sendUnitsPercent);
        }
        SelectedRegion = null;
        EndRegion = null;
    }

    public void CancelSendUnits()
    {
        Debug.Log("Отменить отправку войск");
        SelectedRegion = null;
        EndRegion = null;
    }
    
    public void SendUnits(Region from, Region to, float percent)
    {
        if (from && to)
        {
            int units = Mathf.CeilToInt(from.Units * percent);
            float defenceCoefficient = to.DefenceCoefficient;
            
            if (from.Team == to.Team)
            {
                from.Units -= units;
                to.Units += units;
            }
            else
            {
                from.Units -= units;
                if (units - to.Units * (1 + defenceCoefficient) > 0)
                {
                    to.Units = Mathf.FloorToInt(units - to.Units * (1 + defenceCoefficient));
                    to.Team = from.Team;
                }
                else
                {
                    to.Units -= Mathf.FloorToInt(units * (1 - defenceCoefficient));
                }
            }
        }
    }

    public void OnTap(Vector3 pos)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.TryGetComponent(out Region region))
            {
                if (activeAbility)
                {
                    if (activeAbility.Add(region))
                    {
                        activeAbility.Execute();
                        ClearAbility();
                    }
                }
                else
                {
                    if (SelectedRegion)
                    {
                        if (region == SelectedRegion)
                        {
                            SelectedRegion = null;
                            EndRegion = null;
                        }
                        else
                        {
                            if (EndRegion)
                            {
                                if (region != EndRegion)
                                {
                                    SelectedRegion = null;
                                }
                                EndRegion = null;
                            }
                            else
                            {
                                if (SelectedRegion.Team == GameManager.main.ownTeam)
                                {
                                    if (SelectedRegion.IsNeighbour(region))
                                    {
                                        EndRegion = region;
                                    }
                                    else
                                    {
                                        SelectedRegion = null;
                                    }
                                }
                                else
                                {
                                    SelectedRegion = null;
                                }
                            }
                        }
                    }
                    else SelectedRegion = region;
                }
            }
        }
    }

    void CreateAbility(Ability _ability)
    {
        Ability ability = Instantiate(_ability);
        ability.header = _ability.header;
        ability.regions = new List<Region>();
        ability.regionsRequirement = _ability.regionsRequirement;
        activeAbility = ability;
        
        if(activeAbility.IsReady()) 
            activeAbility.Execute();
    }

    void ClearAbility()
    {
        if(activeAbility) Destroy(activeAbility);
    }



    //TODO: Переделать на LineRenderer
    private void OnPostRender()
    {
        DashedLineStyle lineStyle = EndRegion ? main : common;
        
        if(lines.Count > 0 && SelectedRegion.Team == GameManager.main.ownTeam)
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
    
    public void OnUnitsSliderChanged(float v)
    {
        sendUnitsPercent = v;
    }

    #region Utils
    
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
    
    Dictionary<Vector3, Vector3> RecalculateLines()
    {
        Dictionary<Vector3, Vector3> lines = new Dictionary<Vector3, Vector3>();

        if (SelectedRegion && SelectedRegion.Team == GameManager.main.ownTeam)
        {
            if (EndRegion)
            {
                Vector3 lineDirection = (EndRegion.transform.position - SelectedRegion.transform.position).normalized * linePadding;
                lines.Add(SelectedRegion.transform.position + lineDirection, EndRegion.transform.position - lineDirection);
            }
            else
            {
                foreach (var n in SelectedRegion.neighbours.Keys)
                {
                    Vector3 lineDirection = (SelectedRegion.transform.position - n.transform.position).normalized * linePadding;
                    lines.Add(n.transform.position + lineDirection, SelectedRegion.transform.position - lineDirection);
                }   
            }
        }
        
        return lines;
    }
 
    #endregion
}
