using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Random = UnityEngine.Random;

public class Imitator : MonoBehaviour
{
    public Kingdom kingdom;
    public GameCore gameCore;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Imitate());
    }

    IEnumerator Imitate()
    {
        Kingdom[] kingdoms = gameCore.kingdoms;
        Region[] regions = gameCore.regions;
        yield return new WaitForSeconds(Random.Range(1f, 5f));

        while (true)
        {
            List<Region> kindomRegions = regions.Where(p => p.kingdom != null && p.kingdom.hash == kingdom.hash).ToList();

            Region from = kindomRegions.OrderByDescending(p => p.Units).ToArray()[0];
            Region to = null;

            int units = 0;

            Region[] enemyNeighbourRegions = from.neighbours
                .Where(p => p.cellType == Region.CellType.Land && p.kingdom != kingdom && p.Units < from.Units && !p.IsCapital)
                .OrderBy(a => Guid.NewGuid()) //Перемешать
                .ToArray();

            if (enemyNeighbourRegions.Length > 0)
            {
                units = Mathf.Clamp(Mathf.RoundToInt(from.Units * Random.Range(0.9f, 1f)), 0, from.Units);
                to = enemyNeighbourRegions[0];
            }
            else
            {
                Region[] ourNeighbourRegions = from.neighbours
                    .Where(p => p.cellType == Region.CellType.Land && p.kingdom == kingdom)
                    .OrderBy(a => Guid.NewGuid()) //Перемешать
                    .ToArray();

                bool chance = Random.Range(0f, 1f) < 0.2f;

                if(ourNeighbourRegions.Length > 0 && chance)
                {
                    units = Mathf.Clamp(Mathf.RoundToInt(from.Units * Random.Range(0.8f, 1f)), 0, from.Units);
                    to = ourNeighbourRegions[0];
                }
            }

            from.Units -= units;

            if(to != null)
            {
                gameCore.SpawnUnits(from.id, to.id, kingdom.id, units);
            }

            yield return new WaitForSeconds(Random.Range(6f / kindomRegions.Count, 10f / kindomRegions.Count));
        }
    }
}
