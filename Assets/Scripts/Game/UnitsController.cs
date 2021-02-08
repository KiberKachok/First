using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
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
    public float speed = 0.6f;
    public float percentsToDest = 0f;

    private void Start()
    {
        if(to.cellType == Region.CellType.Land)
        {
            speed = 0.3f;
        }
        else
        {
            speed = 0.44f;
        }
    }

    private void Update()
    {
        percentsToDest = Vector3.Distance(transform.position, from.transform.position) / Vector3.Distance(from.transform.position, to.transform.position);
        if (transform.position != to.transform.position)
        {
            transform.position = Vector3.MoveTowards(transform.position, to.transform.position,
                speed * Time.deltaTime);
        }
        else
        {
            if (PhotonNetwork.IsMasterClient || GameCore.main.isImitatorMode)
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
                            Debug.Log(to.kingdom);
                            GameCore.main.CaptureKingdom(to.kingdom, kingdom);
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
