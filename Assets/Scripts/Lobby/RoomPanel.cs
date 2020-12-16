using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class RoomPanel : MonoBehaviour
{
    public void LeaveRoom()
    {
        RoomLayout.main.HideRoomPanel();
        PhotonNetwork.LeaveRoom();
    }
}
