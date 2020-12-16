using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class Network : MonoBehaviourPunCallbacks
{
    [SerializeField] private float connectionCheckRate = 0.7f;
    
    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.JoinLobby();
        PhotonNetwork.NickName = Nickname;
        StartCoroutine(ConnectionChecker());
    }

    private void Update()
    {
        State = PhotonNetwork.NetworkClientState;
        if (Input.GetKeyDown(KeyCode.C))
        {
            Connect();
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            Disconnect();
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            PhotonNetwork.JoinLobby();
        }
    }

    [ShowInInspector]
    public string Nickname
    {
        get
        {
            return _nickname;
        }
        set
        {
            _nickname = value;
            PhotonNetwork.NickName = _nickname;
        }
    }
    private string _nickname = "User";
    
    [ShowInInspector]
    public ClientState State
    {
        get
        {
            return _state;
        }
        set
        {
            if (value != _state)
            {
                _state = value;
                OnClientStateChanged();
            }
        }
    }
    private ClientState _state;
    void OnClientStateChanged()
    {
        stateLabel.text = _state + (_state == ClientState.Disconnected ? " " + _disconnectCause : "");
        onStateChanged?.Invoke(_state);
    }

    [Serializable]
    public class StateEvent : UnityEvent<ClientState> { };
    public StateEvent onStateChanged;
    [SerializeField] private TextMeshProUGUI stateLabel;
    
    IEnumerator ConnectionChecker()
    {
        while (true)
        {
            yield return new WaitForSeconds(connectionCheckRate);
            if (State == ClientState.Leaving || State == ClientState.Disconnected)
            {
                ClientState preState = State;
                yield return new WaitForSeconds(connectionCheckRate);
                if (preState == State)
                {
                    PhotonNetwork.ConnectUsingSettings();
                }
            }
        }
    }

    [ContextMenu("Connect")]
    public void Connect()
    {
        PhotonNetwork.ConnectUsingSettings();
    }
    
    [ContextMenu("Disconnect")]
    public void Disconnect()
    {
        PhotonNetwork.Disconnect();
    }
    
    
    public override void OnDisconnected(DisconnectCause cause)
    {
        _disconnectCause = cause.ToString();
    }
    private string _disconnectCause;
    
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }
}
