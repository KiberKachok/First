using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class RoomListController : MonoBehaviourPunCallbacks
{
    [SerializeField] private RectTransform roomListRect;
    [SerializeField] private float paddingBetweenRooms = 5;
    
    [SerializeField] private GameObject roomPrefab;

    public void CreateRoom()
    {
        string roomId = Random.Range(100, 999).ToString();
        RoomOptions roomOptions = new RoomOptions()
        {
            MaxPlayers = 16,
            IsOpen = true,
            IsVisible = true,
        };

        PhotonNetwork.CreateRoom(roomId, roomOptions, TypedLobby.Default);
    }

    private List<RoomInfo> _openRooms = new List<RoomInfo>();
    private List<RoomInfo> _runningRooms = new List<RoomInfo>();
    private Dictionary<RoomInfo, GameObject> _roomDisplay = new Dictionary<RoomInfo, GameObject>();

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log(roomList.Count);
        List<RoomInfo> newRooms = roomList.Where(p => p.IsOpen).Except(_openRooms).ToList();
        List<RoomInfo> deletedRooms = _openRooms.Except(roomList).ToList();
        _openRooms.AddRange(newRooms);
        
        foreach (var room in newRooms)
        {
            _roomDisplay.Add(room, Instantiate(roomPrefab, Vector3.zero, Quaternion.identity, roomListRect));
        }

        foreach (var room in deletedRooms)
        {
            Destroy(_roomDisplay[room]);
            _roomDisplay.Remove(room);
        }   
    }
}
