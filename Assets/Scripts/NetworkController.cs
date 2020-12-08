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

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            PhotonNetwork.CreateRoom("new room", new RoomOptions(), TypedLobby.Default);
        }
        
        if (Input.GetKeyDown(KeyCode.J))
        {
            PhotonNetwork.JoinRandomRoom();
        }
        
        if (Input.GetKeyDown(KeyCode.S))
        {
            PhotonNetwork.LoadLevel("main");
        }
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room");
    }
}
