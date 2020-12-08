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
            race = _team.race;
            StartCoroutine(Colorize(_team, 0.5f));
        }
    }
    private Team _team;

    [PropertyOrder(-1)]
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

                _unitsCounterGui.SetText(value.ToString());
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
                int i = Mathf.Clamp(value, 1, race.regionMaxLevel);
                unitsMax = race.regionUpgradeMaxUnits[i - 1];
                unitsGrowSpeed = race.regionUpgradeUnitsGrowSpeed[i - 1];
                DefenceCoefficient = race.regionUpgradeDefenceCoefficient[i - 1];
                _level = i;   
            }
        }
    }
    private int _level = 1;
    
    [ShowInInspector]
    public float DefenceCoefficient {get; set;} = 0.1f;

    void Start()
    {
        _material = GetComponent<MeshRenderer>().material;
        _unitsCounterGui.cellType = cellType;
        _unitsCounterGui.SetText(Units.ToString());
        _gameManager = GameManager.main;

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

    #region RarelyUsedVariables
    
    [PropertyOrder(1), PropertySpace(15)]
    public CellType cellType;
    
    [PropertyOrder(2), DictionaryDrawerSettings(KeyLabel = "Region", ValueLabel = "isSeparated")]
    public Dictionary<Region, bool> neighbours;

    private GameManager _gameManager;
    private UnitsCounterGUI _unitsCounterGui;
    private Material _material;

    #endregion

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

    public void SetCounter(UnitsCounterGUI counter)
    {
        _unitsCounterGui = counter;
    }

    public bool IsNeighbour(Region n)
    {
        return neighbours.ContainsKey(n);
    }
    #endregion
}
