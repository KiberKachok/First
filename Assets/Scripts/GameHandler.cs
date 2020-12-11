using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Shapes;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class GameHandler : MonoBehaviourPunCallbacks
{
    public Region SelectedRegion
    {
        get
        {
            return _selectedRegion;
        }
        set
        {
            if (value != _selectedRegion)
            {
                _selectedRegion = value;
                lines = RecalculateLines();
            }
        }
    }
    private Region _selectedRegion;
    
    public Region EndRegion
    {
        get
        {
            return _endRegion;
        }
        set
        {
            _endRegion = value;
            sendUnitsPanel.SetActive(_endRegion != null);
            if (_endRegion) sendUnitsSlider.value = 1f;
            lines = RecalculateLines();
        }
    }
    private Region _endRegion;

    public Ability activeAbility;
    
    #region RarelyUsedVars

    public GameObject sendUnitsPanel;
    public Slider sendUnitsSlider;
    public float sendUnitsPercent;

    Dictionary<Vector3, Vector3> lines = new Dictionary<Vector3, Vector3>();
    [Space(10)]
    public float linePadding = 0.5f;
    public DashedLineStyle mainStyle;
    public DashedLineStyle common;

    public Button upgradeButton;
    public TextMeshProUGUI upgradeRegionPriceText;
    public TextMeshProUGUI upgradeRegionLevelText;

    [SerializeField] private GameObject unitsPrefab;
    public static GameHandler main;
    #endregion

    void Awake()
    {
        main = this;
    }
    
    void Start()
    {
        sendUnitsPercent = sendUnitsSlider.value;
    }

    private void Update()
    {
        RecalculateUpgradeButton();
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
                    else if(region.Team == GameManager.main.ownTeam)
                    {
                        SelectedRegion = region;
                    }
                }
            }
        }
        RecalculateUpgradeButton();
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
    
    //TODO: Переделать на золото?
    public void RecalculateUpgradeButton()
    {
        if (SelectedRegion && SelectedRegion.cellType == CellType.Land && EndRegion == null)
        {
            upgradeButton.transform.gameObject.SetActive(true);
            upgradeRegionLevelText.text = SelectedRegion.Level.ToString();
            
            if (SelectedRegion.Team == GameManager.main.ownTeam)
            {
                int price = SelectedRegion.GetUpgradePrice();
                upgradeRegionPriceText.text = price == -1 ? "" : price.ToString();
                upgradeButton.interactable = !(price == -1 || price > SelectedRegion.Units);
            }
            else
            {
                upgradeButton.interactable = false;
                upgradeRegionPriceText.text = "";
            }
        }
        else
        {
            upgradeButton.transform.gameObject.SetActive(false);
        }
    }
    
    public void UpgradeRegion()
    {
        if (SelectedRegion.Units >= SelectedRegion.GetUpgradePrice())
        {
            SelectedRegion.Units -= SelectedRegion.GetUpgradePrice();
            _selectedRegion.Level++;
        }
    }

    [PunRPC]
    public void SendUnits(Region from, Region to, float percent)
    {
        if (from && to)
        {
            int units = Mathf.CeilToInt(from.Units * percent);
            if (units > 0)
            {
                UnitsController unitsController = Instantiate(unitsPrefab, from.transform.position, Quaternion.Euler(90, 90, 0)).GetComponent<UnitsController>();
                unitsController.Init(from, to, units);
                from.Units -= units;
                if (from.cellType == CellType.Water && from.Units == 0)
                {
                    from.Team = null;
                    from.Race = null;
                }   
            }
        }
    }
    public void AgreeSendUnits()
    {
        Debug.Log("Отправить войска");
        if (SelectedRegion && EndRegion)
        {
            photonView.RPC("SendUnits", RpcTarget.All, SelectedRegion, EndRegion, sendUnitsPercent );
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
    public void OnUnitsSliderChanged(float v)
    {
        sendUnitsPercent = v;
    }

    #region Utils
    
    //TODO: Переделать на LineRenderer
    private void OnPostRender()
    {
        DashedLineStyle lineStyle = EndRegion ? mainStyle : common;
        
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
