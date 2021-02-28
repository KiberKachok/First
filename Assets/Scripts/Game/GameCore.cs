using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using System.Globalization;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
using RealmsNetwork;
using LiteNetLib.Utils;

public class GameCore : NetworkedMonoBehaviour
{
    public Dictionary<string, Kingdom> playersKingdoms = new Dictionary<string, Kingdom>();
    public Kingdom[] kingdoms;
    public Region[] regions;
    public Region[] lands;
    public Region[] waters;
    public List<UnitsController> unitsControllers = new List<UnitsController>();
    public static string ownHash;
    public string kingdomsData = "None";
    public bool isCamerAligned = false;
    public bool isFirstGameDataReceiving = true;

    [ShowInInspector]
    public Region SelectedRegion
    {
        get => _selectedRegion;
        set
        {
            if (value == null)
            {
                _selectedRegion.material.SetInt("isSelected", 0);
            }
            else
            {
                value.material.SetInt("isSelected", 1);
            }
            _selectedRegion = value;
            _lineDrawer.BuildLines();
        }
    }
    private Region _selectedRegion;

    [ShowInInspector]
    public Region EndRegion
    {
        get => _endRegion;
        set
        {
            if (value == null)
            {
                _endRegion.material.SetInt("isSelected", 0);
                _guiController.HideSlider();
            }
            else
            {
                value.material.SetInt("isSelected", 1);
                _guiController.ShowSlider();
            }
            _endRegion = value;
            _lineDrawer.BuildLines();
        }
    }
    private Region _endRegion;

    public GameObject unitsPrefab;

    public int roomSeed;
    public Palette palette;

    private GUIController _guiController;
    private Chat _chat;
    private LineDrawer _lineDrawer;
    public static GameCore main;

    public GameObject capitalMarkPrefab;

    public bool isImitatorMode = false;
    public int botsCount = 10;
    public Region[] botStartRegions;

    void Awake()
    {
        _guiController = FindObjectOfType<GUIController>();
        _chat = FindObjectOfType<Chat>();
        _lineDrawer = FindObjectOfType<LineDrawer>();
        main = this;
    }


    void Start()
    {
        bool isActivation = !Convert.ToBoolean(PlayerPrefs.GetInt("isActivation"));

        if(Client.main != null)
        {
            ownHash = Client.main.hash;
        }

        if (isImitatorMode)
        {
            List<Kingdom> _kingdoms = new List<Kingdom>();
            List<Region> _regions = new List<Region>();
            List<Region> _mixedRegions = lands.OrderBy(a => Guid.NewGuid()).ToList();
            int kingdomsCount = botsCount;

            for (int i = 0; i < kingdomsCount; i++)
            {
                Region mainRegion;
                if (isActivation)
                {
                    mainRegion = botStartRegions[i];
                }
                else
                {
                    mainRegion = _mixedRegions[i % _mixedRegions.Count];
                }

                Kingdom k = new Kingdom(i, i.ToString(), i.ToString(), palette.GetColor(i));
                _mixedRegions.Remove(mainRegion);

                mainRegion.kingdom = k;
                mainRegion.IsCapital = true;
                mainRegion.Units = Random.Range(5, 16);
                mainRegion.currentUnits = Random.Range(0f, 1f);

                int provincesCount = Random.Range(0, 1);
                List<Region> _mixedNeighbours = mainRegion.neighbours.OrderBy(a => Guid.NewGuid()).ToList();

                for (int j = 0; j < provincesCount; j++)
                {
                    Region targetRegion = _mixedNeighbours[j];
                    if (targetRegion.kingdom == null && targetRegion.cellType == Region.CellType.Land)
                    {
                        _mixedRegions.Remove(targetRegion);
                        targetRegion.kingdom = k;
                        targetRegion.Units = Random.Range(2, 10);
                        targetRegion.currentUnits = Random.Range(0f, 1f);
                    }
                }

                _kingdoms.Add(k);
            }

            kingdoms = _kingdoms.ToArray();
            StartImitator();
        }
    }

    public void StartImitator()
    {
        for (int i = 0; i < kingdoms.Length; i++)
        {
            Imitator imitator = gameObject.AddComponent<Imitator>();
            imitator.kingdom = kingdoms[i];
            imitator.gameCore = this;
        }
    }

    [ContextMenu("Align Camera")]
    public void AlignCamera(Vector3 pos)
    {
        Camera.main.transform.position = new Vector3(pos.x, Camera.main.transform.position.y, pos.z);
    }

    private void Update()
    {
        if (_selectedRegion && _selectedRegion.kingdom != null && _selectedRegion.kingdom.hash != ownHash)
        {
            EndRegion = null;
            SelectedRegion = null;
        }

        if (isImitatorMode)
        {
            for(int i = 0; i < regions.Length; i++)
            {
                regions[i].Recalculate();
            }
        }
    }
    
    public void OnTap(Vector3 pos)
    {
        if (_chat.isSenderOpened())
        {
            _chat.HideSender();
            return;
        }
        
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            Region rg = hit.transform.GetComponent<Region>();

            if (SelectedRegion)
            {
                if (EndRegion)
                {
                    SelectedRegion = null;
                    EndRegion = null;
                }
                else
                {
                    if (SelectedRegion.neighbours.Contains(rg))
                    {
                        EndRegion = rg;
                    }
                    else
                    {
                        SelectedRegion = null;
                    }
                }
            }
            else
            {
                if (rg.kingdom != null && rg.kingdom.hash == ownHash)
                {
                    SelectedRegion = rg;
                }
            }
        }
    }

    public void SpawnUnits(int fromId, int toId, int kingdomId, int units)
    {
        Region from = regions[fromId];
        Region to = regions[toId];
        Kingdom kingdom = kingdoms[kingdomId];
        
        if (units > 0 && from.neighbours.Contains(to))
        {
            GameObject unitsObject = Instantiate(unitsPrefab, from.transform.position, Quaternion.Euler(270 + 180, 0, -90));
            TextMeshPro t = unitsObject.transform.GetChild(0).GetComponent<TextMeshPro>();
            t.text = units.ToString();
            t.color = kingdom.color;
            UnitsController unitsController = unitsObject.GetComponent<UnitsController>();
            unitsController.Units = units;
            unitsController.from = from;
            unitsController.to = to;
            unitsController.kingdom = kingdom;
            unitsControllers.Add(unitsController);
        }
    }

    [ContextMenu("SetIds")]
    public void SetIds()
    {
        for (int i = 0; i < regions.Length; i++)
        {
            regions[i].id = i;
        }
    }

    public override void OnGameDataUpdate(NetDataReader data)
    {
        int[] regionsKingdoms = data.GetIntArray();
        int[] regionsUnits = data.GetIntArray();
        bool[] regionsCapitals = data.GetBoolArray();

        for (int i = 0; i < 110; i++)
        {
            Kingdom newKingdom = regionsKingdoms[i] != -1 ? Client.main.roomKingdoms.ElementAt(regionsKingdoms[i]) : null;

            if(regions[i].kingdom != newKingdom)
            {
                regions[i].kingdom = newKingdom;
            }

            if(regions[i].Units != regionsUnits[i])
            {
                regions[i].Units = regionsUnits[i];
            }

            if (regions[i].IsCapital != regionsCapitals[i])
            {
                regions[i].IsCapital = regionsCapitals[i];
            }
        }

        if (isFirstGameDataReceiving)
        {
            Kingdom ownKingdom = kingdoms.Where(p => p.hash == Client.main.hash).FirstOrDefault();

            if (ownKingdom != null)
            {
                for (int i = 0; i < regions.Length; i++)
                {
                    if (regions[i].IsCapital && regions[i].kingdom != null && regions[i].kingdom.hash == ownKingdom.hash)
                    {
                        AlignCamera(regions[i].transform.position);
                    }
                }
            }

            isFirstGameDataReceiving = false;
        }
    }

    public override void OnGameMetaDataUpdate(List<Kingdom> kingdoms)
    {
        this.kingdoms = kingdoms.ToArray();
        Kingdom ownKingdom = kingdoms.Where(p => p.hash == Client.main.hash).FirstOrDefault();
    }

    public override void OnSendUnits(int fromId, int toId, int kingdomId, int units)
    {
        SpawnUnits(fromId, toId, kingdomId, units);
    }

    public override void OnKingdomCaptured(int kingdomId)
    {
        List<GameObject> unitsOfCapturedKingdom = new List<GameObject>();

        foreach(UnitsController uc in unitsControllers)
        {
            if(uc.kingdom.id == kingdomId)
            {
                unitsOfCapturedKingdom.Add(uc.gameObject);
            }
        }

        foreach (GameObject g in unitsOfCapturedKingdom)
        {
            Destroy(g);
        }
    }
}
