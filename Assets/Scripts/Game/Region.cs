using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;

public class Region : SerializedMonoBehaviour
{
    [ShowInInspector, PropertyOrder(-2)]
    public Team Team
    {
        get { return _team; }
        set
        {
            _team = value;

            Race = _team ? _team.race : GameManager.main.neutral;
            _material.color = _team ? _team.regionColor : GameManager.main.neutral.regionColor;
            //_unitsCounter.SetColor(_team ? _team.counterColor : GameManager.main.neutral.counterColor);

            if (_gameHandler.SelectedRegion == this)
               _gameHandler.RecalculateUpgradeButton();
            
        }
    }
    private Team _team;

    [ShowInInspector, PropertyOrder(-1)]
    public Race Race
    {
        get
        {
            return race;
        }
        set
        {
            race = value; 
            Level = _level;
        }
    }
    public Race race;
    
    public int RecalculateUnits()
    {
        if (_unitsCurrent < unitsMax)
        {
            _unitsCurrent = Mathf.Clamp(_unitsCurrent + Time.deltaTime * unitsGrowSpeed, 0, unitsMax);
        }
        else if (_unitsCurrent > unitsMax)
        {
            _unitsCurrent = Mathf.Clamp(_unitsCurrent - Time.deltaTime * 0.6f, unitsMax,
                float.PositiveInfinity);
        }

        Units = Mathf.FloorToInt(_unitsCurrent);
        return Units;
    }
    [ShowInInspector, PropertyOrder(-1), PropertySpace(15)]
    public int Units
    {
        get { return _units; }
        set
        {
            if (value != _units)
            {

                _unitsCounter.SetText(value.ToString());
                _unitsCurrent = value + (_unitsCurrent - Mathf.Floor(_unitsCurrent));
            }

            _units = value;
        }
    }
    public int unitsMax = 10;
    public float unitsGrowSpeed;
    private int _units;
    private float _unitsCurrent;
    
    [ShowInInspector, PropertySpace(15)]
    public int Level
    {
        get
        {
            return _level;
        }
        set
        {
            if (cellType == CellType.Land)
            {
                int i = Mathf.Clamp(value, 1, Race.regionMaxLevel);
                unitsMax = Race.regionUpgradeMaxUnits[i - 1];
                unitsGrowSpeed = Race.regionUpgradeUnitsGrowSpeed[i - 1];
                DefenceCoefficient = Race.regionUpgradeDefenceCoefficient[i - 1];
                _level = i;
                
                if (_gameHandler.SelectedRegion == this)
                    _gameHandler.RecalculateUpgradeButton();
            }
        }
    }
    public int _level = 1;
    public int GetUpgradePrice()
    {
        int price;
        if (Level != race.regionMaxLevel)
        {
            price = race.regionUpgradePrice[Level];
        }
        else
        {
            price = -1;
        }

        return price;
    }
    
    [ShowInInspector]
    public float DefenceCoefficient {get; set;} = 0.1f;
    
    #region RarelyUsedVariables
    
    [PropertyOrder(1), PropertySpace(15)]
    public CellType cellType;
    
    [PropertyOrder(2), DictionaryDrawerSettings(KeyLabel = "Region", ValueLabel = "isSeparated")]
    public Dictionary<Region, bool> neighbours;

    private GameManager _gameManager;
    private GameHandler _gameHandler;
    private UnitsCounter _unitsCounter;
    private Material _material;

    #endregion

    void Start()
    {
        _material = GetComponent<MeshRenderer>().material;
        _unitsCounter.SetText(Units.ToString());
        _gameManager = GameManager.main;
        _gameHandler = GameHandler.main;

        if (cellType == CellType.Land)
        {
            Level = _level;
            Units = race.regionUpgradeMaxUnits[Level - 1];
        }
        else
        {
            DefenceCoefficient = 0;
            unitsGrowSpeed = 0;
            unitsMax = int.MaxValue;
        }
    }

    #region Utils
    
    IEnumerator Colorize(Team newTeam, float time)
    {
        float t = 0;

        while (t < time)
        {
            t += Time.deltaTime;
            _material.color = Color.Lerp(_material.color, newTeam.regionColor, time);
            yield return null;
        }
        _material.color = newTeam.regionColor;
    }
    
    [ContextMenu("SetNeighbours")]
    public void RecalculateNeighbours()
    {
        List<Transform> regionsTransforms = _gameManager.regions.Select(p => p.gameObject.transform).ToList(); 
        regionsTransforms.Remove(transform); //Transform всех регионов, кроме этого

        neighbours = new Dictionary<Region, bool>();
        Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
        Vector3[] ownVertices = mesh.vertices.Select(p => new Vector3(p.x, 0, -p.y) * transform.localScale.x + transform.localPosition).ToArray();

        foreach (Transform n in regionsTransforms)
        {
            Mesh neighbourMesh = n.GetComponent<MeshFilter>().sharedMesh;
            Vector3[] neighbourVertices = neighbourMesh.vertices.Select(p => new Vector3(p.x, 0, -p.y) * n.localScale.x + n.localPosition).ToArray();

            float distance = float.MaxValue;
                
            foreach (var i in ownVertices)
            foreach (var j in neighbourVertices)
            {
                float dTmp = Vector3.Distance(i, j);
                if (dTmp < distance) distance = dTmp;
            }
            
            if (distance < 0.00001f)
            {
                Region neighbourRegion = n.GetComponent<Region>();
                neighbours.Add(neighbourRegion, false);
            }
        }
    }

    public void SetCounter(UnitsCounter counter)
    {
        _unitsCounter = counter;
        counter.mode = cellType == CellType.Land ? CounterMode.RegionLand : CounterMode.RegionWater;
    }

    public bool IsNeighbour(Region n)
    {
        return neighbours.ContainsKey(n);
    }
    #endregion
}
