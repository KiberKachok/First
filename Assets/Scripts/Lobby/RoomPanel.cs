using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class RoomPanel : MonoBehaviourPunCallbacks
{
    [SerializeField] private RectTransform playerListRect; 
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Button startGameButton;
    
    public void LeaveRoom()
    {
        RoomLayout.main.HideRoomPanel();
        PhotonNetwork.LeaveRoom();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerList(PhotonNetwork.CurrentRoom.Players.Values.ToList());
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdatePlayerList(PhotonNetwork.CurrentRoom.Players.Values.ToList());
    }

    public override void OnJoinedRoom()
    {
        UpdatePlayerList(PhotonNetwork.CurrentRoom.Players.Values.ToList());
        UpdateButton();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        UpdateButton();
    }

    public void UpdateButton()
    {
        startGameButton.interactable = PhotonNetwork.IsMasterClient;
    }

    public void StartGame()
    {
        PhotonNetwork.LoadLevel("main");
    }

    public void UpdatePlayerList(List<Player> playerList)
    {
        foreach (Transform child in playerListRect)
        {
            Destroy(child.gameObject);
        }

        foreach (var player in playerList)
        {
            GameObject playerObject = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity, playerListRect);
            playerObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = player.NickName;
        }
    }
}
