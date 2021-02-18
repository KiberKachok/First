using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine.UI;
using CodeStage.AntiCheat.ObscuredTypes;

public class Region : SerializedMonoBehaviour
{
    public int id;
    
    [SerializeField] 
    private float maxUnits = 20;
    [SerializeField]
    private float unitsGrowSpeed = 0.3f;
    [SerializeField]
    public float currentUnits = 0;
    [SerializeField]
    private ObscuredInt _units;
    [ShowInInspector]
    public int Units
    {
        get
        {
            return _units;
        }
        set
        {
            if (value == 0 && cellType == CellType.Water)
            {
                kingdom = null;
                _counter.SetActive(false);
            }
            else
            {
                _counter.SetActive(true);
            }
            _counterText.text = value.ToString();
            _units = value;
        }
    }
    
    private Kingdom _kingdom;
    [ShowInInspector] public Kingdom kingdom
    {
        get
        {
            return _kingdom;
        }
        set
        {
            if(value != kingdom)
            {
                if (value != null)
                {
                    material.SetColor("_Color", value.color);
                    _counterText.color = value.color;
                }
                else
                {
                    if (cellType == CellType.Water)
                    {
                        material.SetColor("_Color", waterColor);
                    }
                }
                _kingdom = value;
            }
        }
    }

    private bool _isCapital = false;
    [ShowInInspector]
    public bool IsCapital
    {
        get
        {
            return _isCapital;
        }
        set
        {
            if(_isCapital != value)
            {
                _isCapital = value;

                if (_isCapital)
                {
                    Instantiate(GameCore.main.capitalMarkPrefab, transform.position, Quaternion.Euler(270 + 180, 0, -90), transform);
                }
                else
                {
                    DestroyImmediate(transform.GetChild(1).gameObject);
                }
            }
        }
    }

    [PropertyOrder(2), DictionaryDrawerSettings(KeyLabel = "Region", ValueLabel = "isSeparated")]
    public Region[] neighbours;
    public CellType cellType;
    public Color waterColor;
    
    public Material material;
    private TextMeshPro _counterText;
    public GameObject _counter;
    
    private void Awake()
    {
        material = GetComponent<MeshRenderer>().material;
        SpawnCounter();
        if (cellType == CellType.Land)
        {
            Units = 10;
        }
    }

    public void Recalculate()
    {
        if (cellType == CellType.Land && kingdom != null) 
        {
            if (Units < maxUnits)
            {
                currentUnits += unitsGrowSpeed * Time.deltaTime;
                if (currentUnits > 1)
                {
                    currentUnits--;
                    Units++;
                }   
            }
            else
            {
                currentUnits = 0;
            }
        }
    }

    [ContextMenu("SetNeighbours")]
    public void RecalculateNeighbours()
    {
        List<Transform> regionsTransforms = FindObjectsOfType<Region>().Select(p => p.transform).ToList(); 
        regionsTransforms.Remove(transform); //Transform всех регионов, кроме этого

        List<Region> neighboursList = new List<Region>();
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
                neighboursList.Add(neighbourRegion);
            }
        }

        neighbours = neighboursList.ToArray();
    }

    [PropertyOrder(3)]
    public GameObject counterPrefab;
    [ContextMenu("SpawnCounter")]
    public void SpawnCounter()
    {
        _counter = Instantiate(counterPrefab, transform.position, Quaternion.Euler(270 + 180, 0, -90), transform);
        _counterText = _counter.transform.GetChild(0).GetComponent<TextMeshPro>();
        _counterText.text = Units.ToString();
        
        if (Units == 0 && cellType == CellType.Water)
        {
            _counter.SetActive(false);
        }
        else
        {
            _counter.SetActive(true);
        }
    }

    public string ToStringFull()
    {
        if (kingdom != null)
        {
            return kingdom.id + ":" + Units;
        }
        else
        {
            return "#" + ":" + Units;
        }
    }

    [ContextMenu("DeleteCounter")]
    public void DeleteCounters()
    {
        DestroyImmediate(transform.GetChild(0).gameObject);
    }
    
    public enum CellType
    {
        Land,
        Water
    }
}