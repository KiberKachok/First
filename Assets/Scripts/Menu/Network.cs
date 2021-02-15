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

public class Network : MonoBehaviourPunCallbacks
{
    public static string Name;
    public static string Hash;
    public List<RoomInfo> rooms = new List<RoomInfo>();
    public UnityEventWrapper<List<RoomInfo>> onRoomsUpdate = new UnityEventWrapper<List<RoomInfo>>();
    public UnityEvent onRoomPlayersUpdate = new UnityEvent();
    Dictionary<string, RoomInfo> cachedRooms = new Dictionary<string, RoomInfo>();

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
        PhotonNetwork.GameVersion = Application.version;
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.MaxResendsBeforeDisconnect = 10;
        PhotonNetwork.ConnectUsingSettings();
        StartCoroutine(UpdatePlayersPing());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            Debug.Log(PhotonNetwork.GetPing());
        }
    }

    IEnumerator UpdatePlayersPing()
    {
        while (true)
        {
            Hashtable PlayerCustomProps = new Hashtable();
            PlayerCustomProps["Ping"] = PhotonNetwork.GetPing();
            PhotonNetwork.LocalPlayer.SetCustomProperties(PlayerCustomProps);
            yield return new WaitForSeconds(3);
        }
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        cachedRooms = new Dictionary<string, RoomInfo>();
    }

    public void CreateRoom()
    {
        Debug.Log("Created Room");
        RoomOptions roomOptions = new RoomOptions {PlayerTtl = 0, EmptyRoomTtl = 0};
        roomOptions.CustomRoomProperties = new Hashtable();
        roomOptions.CustomRoomProperties.Add("Nicknames", PhotonNetwork.LocalPlayer.GetName());
        roomOptions.CustomRoomPropertiesForLobby = new [] { "Nicknames", "Hashes"};
        PhotonNetwork.CreateRoom(Random.Range(100, 1000) + "-000", roomOptions);
    }

    public void JoinRoom(string roomName)
    {
        Debug.Log(PhotonNetwork.NetworkClientState);
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

    public void SetRoomHashes()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            string hashes = string.Join("|", PhotonNetwork.PlayerList.Select(p => p.GetHash()));
            string nicknames = PhotonNetwork.CurrentRoom.CustomProperties["Nicknames"] as string;
            Hashtable data = new Hashtable();
            data.Add("Nicknames", nicknames);
            data.Add("Hashes", hashes);
            PhotonNetwork.CurrentRoom.SetCustomProperties(data);
        }
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void LoadLevel(string sceneName)
    {
        //PhotonNetwork.CurrentRoom.IsOpen = false;

        //if (PhotonNetwork.IsMasterClient)
        //{
        //    int minPing = int.MaxValue;
        //    Player minPingPlayer = null;

        //    foreach (Player player in PhotonNetwork.PlayerList)
        //    {
        //        int ping = Convert.ToInt32(player.CustomProperties["Ping"].ToString());
        //        if (ping < minPing)
        //        {
        //            minPing = ping;
        //            minPingPlayer = player;
        //        }
        //        Debug.Log(player.GetName() + " " + ping);
        //    }

        //    if (minPingPlayer != null)
        //    {
        //        photonView.RPC("StartGame", minPingPlayer);
        //    }
        //    else
        //    {
        //        StartGame();
        //    }
        //}

        SetRoomHashes();
        PhotonNetwork.LoadLevel(sceneName);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach(RoomInfo info in roomList)
        {
            if (info.RemovedFromList)
            {
                cachedRooms.Remove(info.Name);
            }

            if (cachedRooms.ContainsKey(info.Name))
            {
                cachedRooms[info.Name] = info;
            }
            else
            {
                cachedRooms.Add(info.Name, info);
            }
            
        }
        List<RoomInfo> rooms = cachedRooms.Values.ToList();
        Debug.Log("OnNetworkRoomsUpdate: " + string.Join(" ", rooms.Select(p => p.ToStringFull())));
        onRoomsUpdate.Invoke(rooms);
    }

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

    
//            if (PhotonNetwork.IsMasterClient)
//        {
//            int minPing = int.MaxValue;
//    Player minPingPlayer = null;

//            foreach (Player player in PhotonNetwork.PlayerList)
//            {
//                int ping = Convert.ToInt32(player.CustomProperties["Ping"].ToString());
//                if (ping<minPing)
//                {
//                    minPing = ping;
//                    minPingPlayer = player;
//                }
//Debug.Log(player.GetName() + " " + ping);
//            }

//            if (minPingPlayer != null)
//            {
//                PhotonNetwork.SetMasterClient(minPingPlayer);
//            }
//        }
}
