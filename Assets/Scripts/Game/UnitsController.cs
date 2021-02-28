using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitsController : MonoBehaviour
{
    private int _units;
    public int Units
    {
        get
        {
            return _units;
        }
        set
        {
            _units = value;
        }
    }
    public Kingdom kingdom;
    public Region from;
    public Region to;
    public float percentsToDest = 0f;

    float t;
    float timeToGetTarget;

    private void Start()
    {
        if(to.cellType == Region.CellType.Land)
        {
            timeToGetTarget = Vector3.Distance(from.transform.position, to.transform.position) / 0.31f;
        }
        else
        {
            timeToGetTarget = Vector3.Distance(from.transform.position, to.transform.position) / 0.42f;
        }
    }

    private void Update()
    {
        percentsToDest = Vector3.Distance(transform.position, from.transform.position) / Vector3.Distance(from.transform.position, to.transform.position);
        if (transform.position != to.transform.position)
        {
            t += Time.deltaTime / timeToGetTarget;
            transform.position = Vector3.Lerp(from.transform.position, to.transform.position, t);
        }
        else
        {
            if (GameCore.main.isImitatorMode)
            {
                if (to.kingdom != null && to.kingdom.hash == kingdom.hash)
                {
                    to.Units += Units;
                }
                else
                {
                    to.Units -= Units;
                    if (to.Units < 0)
                    {
                        to.Units = Mathf.Abs(to.Units);
                        if (to.IsCapital)
                        {
                            to.IsCapital = false;
                        }
                        to.kingdom = kingdom;
                    }
                }
            }
            GameCore.main.unitsControllers.Remove(this);
            Destroy(gameObject);
        }
    }
}
