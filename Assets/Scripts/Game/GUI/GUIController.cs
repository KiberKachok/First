using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GUIController : MonoBehaviourPunCallbacks
{
    public float sendUnitsPercent;
    public bool showUpgradeButton;
    private GameHandler _gameHandler;
    
    private void Start()
    {
        _gameHandler = FindObjectOfType<GameHandler>();
    }
    
    public void LeaveRoom()
    {
        PhotonNetwork.LoadLevel("Menu");
        PhotonNetwork.LeaveRoom();
    }

    public void AgreeSendUnits()
    {
        if (_gameHandler.SelectedRegion && _gameHandler.EndRegion)
        {
            _gameHandler.photonView.RPC("SendUnits", RpcTarget.All, _gameHandler.SelectedRegion, _gameHandler.EndRegion, sendUnitsPercent );
        }
        _gameHandler.SelectedRegion = null;
        _gameHandler.EndRegion = null;
    }
    
    
    public void CancelSendUnits()
    {
        _gameHandler.SelectedRegion = null;
        _gameHandler.EndRegion = null;
    }
    
    public void OnUnitsSliderChanged(float v)
    {
        sendUnitsPercent = v;
    }

}
