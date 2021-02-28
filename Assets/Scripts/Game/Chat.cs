using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using RealmsNetwork;

public class Chat : NetworkedMonoBehaviour
{
    public int nameLengthRestriction = 15;

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
    
    public GUIAvatars guiAvatars;
    
    void Awake()
    {

    }

    private void Start()
    {

    }

    public void OnTapOnAvatar(string hash)
    {
        if (targetHash == "None")
        {
            targetHash = hash;
            ShowSender(hash);
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
        sender.anchoredPosition = new Vector2(guiAvatars.avatars[hash].GetComponent<RectTransform>().anchoredPosition.x, sender.anchoredPosition.y);
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
        string nickname = Client.main.nickname;
        if(nickname.Length > nameLengthRestriction)
        {
            nickname = nickname.Substring(0, nameLengthRestriction) + ".";
        }

        message = message.Replace("<b>{sender}</b>", nickname);
        Client.main.SendMessage(Client.main.hash, targetHash, message);
        HideSender();
    }

    public void OnTakeMessage(string senderHash, string message)
    {
        if (!mutedPlayers.Contains(senderHash))
        {
            GameObject m = Instantiate(receivedMessagePrefab, Vector3.zero, Quaternion.identity, chatBox);
            m.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = message;
        }
    }

    public override void OnMessageReceived(string senderHash, string message)
    {
        OnTakeMessage(senderHash, message);
    }
}
