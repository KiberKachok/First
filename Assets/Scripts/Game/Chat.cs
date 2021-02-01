using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Chat : MonoBehaviourPunCallbacks
{
    public Player targetPlayer;
    public Message[] ableMessages;
    public RectTransform sender;
    public Vector3 senderPadding;
    public GameObject messagePrefab;
    public GameObject receivedMessagePrefab;
    public Transform chatBox;

    public GameObject mutePrefab;
    public GameObject muteObject;
    public List<Player> mutedPlayers = new List<Player>();
    
    private GUIController _guiController;
    
    void Awake()
    {
        _guiController = FindObjectOfType<GUIController>();
    }

    private void Start()
    {
        BuildSender();
    }

    public void OnTap(Player p)
    {
        if (targetPlayer == null)
        {
            if (PhotonNetwork.PlayerList.Where(k => k.GetHash() == p.GetHash()).Count() > 0)
            {
                Player player = PhotonNetwork.PlayerList.Where(k => k.GetHash() == p.GetHash()).ElementAt(0);
                Debug.Log("Установлен получатель " + player.GetName());
                targetPlayer = player;
                ShowSender(player);
            }
        }
        else
        {
            HideSender();
        }
    }
    
    public void ShowSender(Player p)
    {
        sender.gameObject.SetActive(true);
        sender.transform.position = _guiController.avatars[p].transform.position - senderPadding;
    }

    public void HideSender()
    {
        sender.gameObject.SetActive(false);
        targetPlayer = null;
    }

    public bool isSenderOpened()
    {
        return sender.gameObject.activeSelf;
    }

    public void BuildSender()
    {
        foreach (var message in ableMessages)
        {
            GameObject m = Instantiate(messagePrefab, Vector3.zero, Quaternion.identity, sender);
            
            if (message.icon != null)
            {
                m.transform.GetChild(1).GetComponent<Image>().sprite = message.icon;
            }
            else
            {
                m.transform.GetChild(1).gameObject.SetActive(false);
                m.transform.GetChild(0).GetComponent<RectTransform>().anchoredPosition = new Vector2(16, 0);
            }

            m.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = message.title;
            m.GetComponent<Image>().color = message.color;
            m.GetComponent<Button>().onClick.AddListener(delegate { SendMessage(message); });
        }
        muteObject = Instantiate(mutePrefab, Vector3.zero, Quaternion.identity, sender);
        muteObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Заглушить";
        muteObject.GetComponent<Button>().onClick.AddListener(delegate { UpdateMuteButton(); });
    }

    public void UpdateMuteButton()
    {
        if (mutedPlayers.Contains(targetPlayer))
        {
            mutedPlayers.Remove(targetPlayer);
            muteObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Заглушить";
        }
        else
        {
            mutedPlayers.Add(targetPlayer);
            muteObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Разглушить";
        }
    }

    public void SendMessage(Message msg)
    {
        string message = msg.text;
        message = message.Replace("{sender}", PhotonNetwork.LocalPlayer.GetName());
        Debug.Log("Отправлено сообщение" + targetPlayer);
        photonView.RPC("OnTakeMessage", targetPlayer, PhotonNetwork.LocalPlayer, message);
        HideSender();
    }

    [PunRPC]
    public void OnTakeMessage(Player p, string message)
    {
        Debug.Log("Получено сообщение: «" + message + "»");
        if (!mutedPlayers.Contains(p))
        {
            GameObject m = Instantiate(receivedMessagePrefab, Vector3.zero, Quaternion.identity, chatBox);
            m.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = message;
        }
    }
}
