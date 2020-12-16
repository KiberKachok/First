using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class UnitsController : MonoBehaviour
{
    public int Units
    {
        get
        {
            return units;
        }
        set
        {
            units = value;
            _unitsCounter.SetText(Units.ToString());
        }
    }
    private int units;
    
    public Region from;  
    public Region to;

    public Team team;
    public float speed = 5;

    private UnitsCounter _unitsCounter;
    
    void Update()
    {
        if(transform.position != to.transform.position)
        {
            transform.position = Vector3.MoveTowards(transform.position, to.transform.position, speed * Time.deltaTime);
        }
        else
        {
            OnGetDestination();
        }
    }

    public void OnGetDestination()
    {
        float defenceCoefficient = to.DefenceCoefficient;
            
        if (team == to.Team)
        {
            to.Units += units;
        }
        else
        {
            if (units - to.Units * (1 + defenceCoefficient) > 0)
            {
                to.Units = Mathf.FloorToInt(units - to.Units * (1 + defenceCoefficient));
                to.Team = team;
            }
            else
            {
                to.Units -= Mathf.FloorToInt(units * (1 - defenceCoefficient));
            }
        }
            
        Destroy(gameObject);
    }

    public void Init(Region from, Region to, int units)
    {
        _unitsCounter = GetComponent<UnitsCounter>();
        this.from = from;
        this.to = to;
        this.Units = units;
        team = from.Team;
    }
}
