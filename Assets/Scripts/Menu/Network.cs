using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Events;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Random = UnityEngine.Random;
using IngameDebugConsole;

public class Network : MonoBehaviourPunCallbacks
{
    public static string Name;
    public static string Hash;
    public List<RoomInfo> rooms = new List<RoomInfo>();
    public UnityEventWrapper<List<RoomInfo>> onRoomsUpdate = new UnityEventWrapper<List<RoomInfo>>();
    public UnityEvent onRoomPlayersUpdate = new UnityEvent();

    [Serializable]
    public class UnityEventWrapper<T> : UnityEvent<T> { }

    private void Start()
    {
        bool isActivation = !Convert.ToBoolean(PlayerPrefs.GetInt("isActivation"));
        if (isActivation)
        {
            PlayerPrefs.SetInt("isActivation", 1);
            PlayerPrefs.SetString("Hash", Random.Range(100000, 999999).ToString());
        }

        Hash = PlayerPrefs.GetString("Hash");
        Name = PlayerPrefs.GetString("Name");
        Debug.Log(Hash + "-" + Name);

        if(Name.Length > 0)
        {
            FindObjectOfType<GamePanel>().nicknameText.text = Name;
            PhotonNetwork.NickName = Hash + "-" + Name;
        }
        else
        {
            PhotonNetwork.LocalPlayer.NickName = "#" + "-" + FantasyNameGenerator.GetRandomName();
            FindObjectOfType<GamePanel>().EditNickname();
        }

        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.MaxResendsBeforeDisconnect = 7;
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Joined Lobby");
        PhotonNetwork.JoinLobby();
    }

    public void CreateRoom()
    {
        Debug.Log("Created Room");
        RoomOptions roomOptions = new RoomOptions {/*EmptyRoomTtl = 0*/};
        roomOptions.CustomRoomProperties = new Hashtable();
        roomOptions.CustomRoomProperties.Add("Nicknames", PhotonNetwork.LocalPlayer.GetName());
        roomOptions.CustomRoomPropertiesForLobby = new [] { "Nicknames" };
        PhotonNetwork.CreateRoom(Random.Range(100, 1000) + "-000", roomOptions);
    }

    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        onRoomPlayersUpdate.Invoke();
        UpdateRoomNicknames();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        onRoomPlayersUpdate.Invoke();
        UpdateRoomNicknames();
    }

    public override void OnJoinedRoom()
    {
        onRoomPlayersUpdate.Invoke();
    }

    public override void OnLeftRoom()
    {
        onRoomPlayersUpdate.Invoke();
    }

    public void UpdateRoomNicknames()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            string nicknames = string.Join(", ", PhotonNetwork.PlayerList.Select(p => p.GetName()));
            Hashtable data = new Hashtable();
            data.Add("Nicknames", nicknames);
            PhotonNetwork.CurrentRoom.SetCustomProperties(data);
        }
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void LoadLevel(string sceneName)
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.LoadLevel(sceneName);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        rooms = roomList;
        Debug.Log("OnNetworkRoomsUpdate: " + string.Join(" ", rooms.Select(p => p.ToStringFull())));
        onRoomsUpdate.Invoke(rooms);
    }

    [ConsoleMethod("SetNickname", "set nickname")]
    public static void SetNickname(string nickname)
    {
        PhotonNetwork.NickName = nickname;
    }

    [ContextMenu("ClearPlayerPrefs")]
    public void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
    }

    public void SetName(string name)
    {
        Name = name;
        PhotonNetwork.LocalPlayer.NickName = Hash + "-" + Name;
        PlayerPrefs.SetString("Name", Name);
    }
}
