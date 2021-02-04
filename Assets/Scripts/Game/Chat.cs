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
    public string targetHash = "None";
    public Message[] ableMessages;
    public RectTransform sender;
    public Vector3 senderPadding;
    public GameObject messagePrefab;
    public GameObject receivedMessagePrefab;
    public Transform chatBox;

    public GameObject mutePrefab;
    public GameObject muteObject;
    public List<string> mutedPlayers = new List<string>();
    
    private GUIController _guiController;
    
    void Awake()
    {
        _guiController = FindObjectOfType<GUIController>();
    }

    private void Start()
    {
        BuildSender();
    }

    public void OnTap(string hash)
    {
        if (targetHash == "None")
        {
            if (PhotonNetwork.PlayerList.Select(p => p.GetHash()).Contains(hash))
            {
                Player player = PhotonNetwork.PlayerList.Where(k => k.GetHash() == hash).ElementAt(0);
                Debug.Log("Установлен получатель " + player.GetName());
                targetHash = hash;
                ShowSender(hash);
            }
        }
        else
        {
            HideSender();
        }
    }
    
    public void ShowSender(string hash)
    {
        sender.gameObject.SetActive(true);
        sender.transform.position = _guiController.avatars[hash].transform.position - senderPadding;
    }

    public void HideSender()
    {
        sender.gameObject.SetActive(false);
        targetHash = "None";
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
        if (mutedPlayers.Contains(targetHash))
        {
            mutedPlayers.Remove(targetHash);
            muteObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Заглушить";
        }
        else
        {
            mutedPlayers.Add(targetHash);
            muteObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Разглушить";
        }
    }

    public void SendMessage(Message msg)
    {
        string message = msg.text;
        message = message.Replace("{sender}", PhotonNetwork.LocalPlayer.GetName());
        Player player = PhotonNetwork.PlayerList.Where(k => k.GetHash() == targetHash).ElementAt(0);
        Debug.Log("Отправлено сообщение" + player);
        photonView.RPC("OnTakeMessage", player, PhotonNetwork.LocalPlayer, message);
        HideSender();
    }

    [PunRPC]
    public void OnTakeMessage(Player p, string message)
    {
        Debug.Log("Получено сообщение: «" + message + "»");
        if (!mutedPlayers.Contains(p.GetHash()))
        {
            GameObject m = Instantiate(receivedMessagePrefab, Vector3.zero, Quaternion.identity, chatBox);
            m.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = message;
        }
    }
}
