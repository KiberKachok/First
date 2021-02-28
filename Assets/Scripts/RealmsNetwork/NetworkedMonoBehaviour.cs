using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RealmsNetwork;
using LiteNetLib.Utils;

public class NetworkedMonoBehaviour : MonoBehaviour
{
    
    
    public virtual void OnJoinedRoom(Room room)
    {
    }

    public virtual void OnLeftRoom()
    {
    }
    
    public virtual void OnRoomListUpdate(Dictionary<string, Room> rooms)
    {

    }  

    
    public virtual void OnRoomPlayersListUpdate(Dictionary<string, Player> players)
    {

    }
    
    public virtual void OnLobbyStatisticsUpdate(int playersInGameCount, int playersInRoomsCount)
    {

    }

    public virtual void OnGameMetaDataUpdate(List<Kingdom> kingdoms)
    {

    }

    public virtual void OnConnectionStateUpdate(ConnectionState state)
    {

    }

    public virtual void OnGameDataUpdate(NetDataReader data)
    {

    }

    public virtual void OnSendUnits(int fromId, int toId, int kingdomId, int units)
    {

    }

    public virtual void OnMessageReceived(string senderHash, string message)
    {

    }


    public virtual void OnKingdomCaptured(int kingdomId)
    {

    }
}
