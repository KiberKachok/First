using System;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon.StructWrapping;
using IngameDebugConsole;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class RoomListController : MonoBehaviourPunCallbacks
{
    [SerializeField] private RectTransform roomListRect;
    [SerializeField] private GameObject roomPrefab;
    
    public static void CreateRoom()
    {
        string roomId = Random.Range(100, 999).ToString();
        RoomOptions roomOptions = new RoomOptions()
        {
            MaxPlayers = 16,
            IsOpen = true,
            IsVisible = true,
            EmptyRoomTtl = 0
        };

        PhotonNetwork.CreateRoom(roomId, roomOptions, TypedLobby.Default);
    }

    public static void JoinRoom(RoomInfo room)
    {
        PhotonNetwork.JoinRoom(room.Name);
    }

    private List<RoomInfo> _openRooms = new List<RoomInfo>();
    private List<RoomInfo> _runningRooms = new List<RoomInfo>();
    private Dictionary<RoomInfo, GameObject> _roomDisplay = new Dictionary<RoomInfo, GameObject>();

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        roomList = roomList.Where(p => p.IsOpen && p.PlayerCount > 0).ToList();
        List<RoomInfo> newRooms = roomList.Except(_roomDisplay.Keys).ToList();
        List<RoomInfo> deletedRooms = _openRooms.Except(roomList).ToList();

        foreach (var room in newRooms)
        {
            GameObject roomObject = Instantiate(roomPrefab, Vector3.zero, Quaternion.identity, roomListRect);
            _roomDisplay.Add(room, roomObject);
            Button roomButton = roomObject.GetComponent<Button>();
            roomButton.onClick.AddListener(delegate { JoinRoom(room); });
            roomObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = room.Name;
        }

        foreach (var room in deletedRooms)
        {
            Destroy(_roomDisplay[room]);
            _roomDisplay.Remove(room);
        }   
        
        _openRooms = roomList;
    }
}
