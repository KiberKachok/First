using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon.StructWrapping;
using NaughtyAttributes;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using Shapes;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;
using ScaleMode = Shapes.ScaleMode;

public class MapController : MonoBehaviour
{
    [SerializeField] private GameObject unitsCounterPrefab;
    [SerializeField] private GameManager gameManager;

    private void Awake()
    {
        SpawnCounters();
    }

    [ContextMenu("Initialize")]
    public void Initialize()
    {
        List<Region> regions = new List<Region>();
        
        int addedRegionCount = 0;
        int addedMeshColliderCount = 0;
        int regionsWithoutMainRaceCount = 0;
        List<Region> regionsWithoutMainRace = new List<Region>();
        
        List<Transform> childrenTransforms = GetComponentsInChildren<Transform>().ToList();
        childrenTransforms.Remove(transform);
        
        foreach (Transform child in childrenTransforms)
        {
            child.TryGetComponent(out Region region);
            if (!region)
            {
                region = child.gameObject.AddComponent<Region>();
                addedRegionCount++;
            }
            
            regions.Add(region);
            
            if (!region.race)
            {
                regionsWithoutMainRaceCount++;
                regionsWithoutMainRace.Add(region);
            }

            child.TryGetComponent(out MeshCollider collider);
            if (!collider)
            {
                collider = child.gameObject.AddComponent<MeshCollider>();
                addedMeshColliderCount++;
            }
        }

        gameManager.regions = regions;
        
        Debug.Log("Добавлено <Region>: " + addedRegionCount + "\n" +
                  "Добавлено <MeshCollider>: " + addedMeshColliderCount + "\n" +
                  "Регионов без расы: " + regionsWithoutMainRaceCount + "\n" +
                  string.Join("\n", regionsWithoutMainRace));
    }

    public void SpawnCounters()
    {
        List<Transform> childrenTransforms = GetComponentsInChildren<Transform>().ToList();
        childrenTransforms.Remove(transform);
        
        foreach (Transform child in childrenTransforms)
        {
            if (child.childCount == 0)
            {
                Region region = child.gameObject.GetComponent<Region>();
                GameObject tmp = Instantiate(unitsCounterPrefab, child.transform, false);
                region.SetCounter(tmp.GetComponent<UnitsCounterGUI>());
            }
        }
    }

    [ContextMenu("RandomizeColors")]
    public void RandomizeColors()
    {
        List<Transform> childrenTransforms = GetComponentsInChildren<Transform>().Where(p => p.parent == this.transform).ToList();
        childrenTransforms.Remove(transform);
        
        foreach (Transform child in childrenTransforms)
        {
            Material mat = child.GetComponent<MeshRenderer>().sharedMaterial;
            float red = Mathf.Clamp(mat.color.r + Random.Range(-0.2f, 0.2f), 0f, 1f);
            float green = Mathf.Clamp(mat.color.g + Random.Range(-0.2f, 0.2f), 0, 1f);
            float blue = Mathf.Clamp(mat.color.b + Random.Range(-0.2f, 0.2f), 0, 1f);
            mat.color = new Color(red, green, blue, 1);
        }
    }
    
    [ContextMenu("RandomizeNames")]
    public void RandomizeNames()
    {
        List<Transform> childrenTransforms = GetComponentsInChildren<Transform>().Where(p => p.parent == this.transform).ToList();
        childrenTransforms.Remove(transform);

        for (int i = 0; i < childrenTransforms.Count; i++)
        {
            childrenTransforms[i].name = "Plane " + i;
        }
    }

    [ContextMenu("InitializeBorder")]
    public void InitializeBorder()
    {
        List<Transform> childrenTransforms = GetComponentsInChildren<Transform>().Where(p => p.parent == this.transform).ToList();
        childrenTransforms.Remove(transform);
        
        foreach (Transform child in childrenTransforms)
        {
            Mesh mesh = child.GetComponent<MeshFilter>().mesh;
            List<Vector3> reorderedVertices = new List<Vector3>();
            float randomY = Random.Range(0f, 0.0001f);
            List<Vector3> vertices = mesh.vertices.Select(p => p + new Vector3(0, 0.001f)).ToList();
            OrderVertices(vertices[0], vertices, ref reorderedVertices);
            reorderedVertices = reorderedVertices.Distinct().ToList();
            
        }
    }
    
    void OrderVertices(Vector3 start, List<Vector3> vertList, ref List<Vector3> reorderedVertices)
    {
        List<Vector3> candidates = vertList.Except(reorderedVertices).ToList();
        if (candidates.Count == 0)
        {
            reorderedVertices.Add(start);
        }
        else
        {
            reorderedVertices.Add(start);
            candidates = candidates.OrderBy(p => Vector3.Distance(start, p)).ToList();
            Vector3 nearest = candidates[0];
            OrderVertices(nearest, vertList, ref reorderedVertices);
        }
    }
}
