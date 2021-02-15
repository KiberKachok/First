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
    public RectTransform sender;
    public RectTransform avatarsRect;
    public Vector3 senderPadding;
    public GameObject messagePrefab;
    public GameObject receivedMessagePrefab;
    public Transform chatBox;

    public GameObject leftArrow;
    public GameObject rightArrow;
    public int senderActivePageNumber = 0; //По индексу
    public GameObject[] senderPages;
    public Button[] muteButtons;

    public List<string> mutedPlayers = new List<string>();
    
    private GUIController _guiController;
    
    void Awake()
    {
        _guiController = FindObjectOfType<GUIController>();
    }

    private void Start()
    {

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

    //(-1 - влево) (1 - вправо)
    public void SwitchPage(int direction)
    {
        senderActivePageNumber = Mathf.Clamp(senderActivePageNumber + direction, 0, senderPages.Length - 1);
        UpdateSender();
    }

    public void UpdateSender()
    {
        foreach(GameObject page in senderPages)
        {
            page.SetActive(false);
        }

        senderPages[senderActivePageNumber].SetActive(true);

        if (senderActivePageNumber == 0)
        {
            leftArrow.SetActive(false);
        }
        else
        {
            leftArrow.SetActive(true);
        }

        if (senderActivePageNumber == senderPages.Length - 1)
        {
            rightArrow.SetActive(false);
        }
        else
        {
            rightArrow.SetActive(true);
        }
    }
    
    public void ShowSender(string hash)
    {
        senderActivePageNumber = 0;
        UpdateSender();
        sender.gameObject.SetActive(true);
        sender.anchoredPosition = new Vector2(_guiController.avatars[hash].GetComponent<RectTransform>().anchoredPosition.x, sender.anchoredPosition.y);
    }

    public void HideSender()
    {
        senderActivePageNumber = 0;
        sender.gameObject.SetActive(false);
        targetHash = "None";
    }

    public bool isSenderOpened()
    {
        return sender.gameObject.activeSelf;
    }

    public void UpdateMuteButton()
    {
        if (mutedPlayers.Contains(targetHash))
        {
            mutedPlayers.Remove(targetHash);
            foreach(Button button in muteButtons)
            {
                button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Заглушить";
            }
        }
        else
        {
            mutedPlayers.Add(targetHash);
            foreach (Button button in muteButtons)
            {
                button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Разглушить";
            }
        }
    }

    public void SendChatMessage(string msg)
    {
        string message = msg;
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
