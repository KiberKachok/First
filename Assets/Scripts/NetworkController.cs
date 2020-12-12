using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkController : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.JoinLobby();
        PhotonNetwork.NickName = "34253_ТимурМудрый";
    }

    public void CreateRoom()
    {
        PhotonNetwork.CreateRoom("new room", new RoomOptions(), TypedLobby.Default);
    }

    public void JoinRandomRoom()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    public void StartGame()
    {
        PhotonNetwork.LoadLevel("main");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Подключился к мастеру");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Подключился к комнате");
    }
}
