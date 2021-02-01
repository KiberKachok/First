using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class NetworkChangeHandler : MonoBehaviourPunCallbacks
{
    public TextMeshProUGUI text;
    public Color normalColor;
    public Color errorColor;
    public List<ClientState> showedStates = System.Enum.GetValues(typeof(ClientState)).Cast<ClientState>().ToList();
    public List<ClientState> errorStates = new List<ClientState>();
    public List<ClientState> hiddenStates = new List<ClientState>();
    public List<ClientState> warningStates = new List<ClientState>();
    ClientState currentState;
    public float currentStateTime = 0;
    public float stateWarningTime = 1f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        currentStateTime += Time.deltaTime;
        if (currentStateTime > stateWarningTime && (warningStates.Contains(currentState) || errorStates.Contains(currentState)))
        {
            currentStateTime = 0;
            Reconnect();
        }


        if (PhotonNetwork.NetworkClientState != currentState)
        {
            currentState = PhotonNetwork.NetworkClientState;
            OnNetworkStateChange(currentState);
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.Reconnect();
            }
            else
            {
                PhotonNetwork.ConnectUsingSettings();
            }
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            PhotonNetwork.Disconnect();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log(PhotonNetwork.SendRate);
            //PhotonNetwork.SendAllOutgoingCommands();
            Debug.Log(PhotonNetwork.GetPing());
        }

    }

    public void Reconnect()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public void OnNetworkStateChange(ClientState state)
    {
        if (showedStates.Contains(state))
        {
            text.color = normalColor;
            text.text = state.ToString();
        }
        else if (errorStates.Contains(state))
        {
            text.color = errorColor;
            text.text = state.ToString();
        }
        else
        {
            text.text = "";
        }
    }
}
