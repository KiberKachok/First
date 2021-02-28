using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RealmsNetwork;
using System.Linq;

public class KeyCommander : MonoBehaviour
{
    Client client;
    // Start is called before the first frame update
    void Start()
    {
        client = FindObjectOfType<Client>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.C))
        {
            client.CreateRoom();
        }

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.L))
        {
            client.LeaveRoom();
        }

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.J))
        {
            client.JoinRoom(client.rooms.First().Value.name);
        }

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.S))
        {
            //Начать игру
        }
    }
}
