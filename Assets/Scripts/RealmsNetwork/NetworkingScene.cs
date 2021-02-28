using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RealmsNetwork;

public class NetworkingScene : MonoBehaviour
{
    NetworkedMonoBehaviour[] callbacksTargets;
    Client main;

    // Start is called before the first frame update
    void Start()
    {
        callbacksTargets = FindObjectsOfType<NetworkedMonoBehaviour>();
        main = FindObjectOfType<Client>();
        main.callbacksTargets = callbacksTargets;

        foreach (var i in callbacksTargets)
        {
            i.OnLobbyStatisticsUpdate(main.playersInGameCount, main.playersInRoomsCount);
            i.OnRoomListUpdate(main.rooms);
            i.OnGameMetaDataUpdate(main.roomKingdoms);
            i.OnConnectionStateUpdate(main.ConnectionState);
        }
    }

    public void LeaveRoom()
    {
        main.LeaveRoom();
    }

    public void CreateRoom()
    {
        main.CreateRoom();
    }

    public void StartGame()
    {
        main.StartGame();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
