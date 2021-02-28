using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using RealmsNetwork;
using LiteNetLib.Utils;
using LiteNetLib;
using System.Linq;
using TMPro;
using ConnectionState = RealmsNetwork.ConnectionState;

public class UIConnectionState : NetworkedMonoBehaviour
{
    public TextMeshProUGUI connectionStateText;

    public override void OnConnectionStateUpdate(ConnectionState state)
    {
        switch (state)
        {
            case ConnectionState.Connected:
                connectionStateText.text = "";
                break;
            case ConnectionState.Connecting:
                connectionStateText.text = "Подключение";
                break;
            case ConnectionState.Disconnected:
                connectionStateText.text = "Отключено (Проверьте интернет-соединение)";
                break;
        }
    }
}
