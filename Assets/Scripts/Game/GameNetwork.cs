using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameNetwork : MonoBehaviourPunCallbacks
{
    public void LeaveRoom()
    {
        if(PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            SceneManager.LoadScene("Menu");
        }
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("Menu");
    }
}
