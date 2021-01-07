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
                _lineDrawer.RecalculateLines();
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
            _lineDrawer.RecalculateLines();
        }
    }
    private Region _endRegion;

    public Ability activeAbility;

    private LineDrawer _lineDrawer;
    #region RarelyUsedVars

    public GameObject sendUnitsPanel;
    public Slider sendUnitsSlider;
    public float sendUnitsPercent;

    public Button upgradeButton;
    public TextMeshProUGUI upgradeRegionPriceText;
    public TextMeshProUGUI upgradeRegionLevelText;

    [SerializeField] private GameObject unitsPrefab;
    public static GameHandler main;
    #endregion

    void Awake()
    {
        main = this;
        _lineDrawer = GetComponent<LineDrawer>();
    }

    public void StartGame()
    {
        
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
}
