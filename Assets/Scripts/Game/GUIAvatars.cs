using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RealmsNetwork;
using TMPro;

public class GUIAvatars : NetworkedMonoBehaviour
{
    public GameObject avatarPrefab;
    public RectTransform avatarPanelRectTransform;
    public Dictionary<string, GameObject> avatars = new Dictionary<string, GameObject>();
    public Chat chat;

    public void BuildAvatars(List<Kingdom> kingdoms)
    {
        avatars = new Dictionary<string, GameObject>();

        foreach (Transform child in avatarPanelRectTransform)
        {
            Destroy(child.gameObject);
        }

        foreach (var kingdom in kingdoms)
        {
            GameObject avatar = Instantiate(avatarPrefab, avatarPanelRectTransform);
            avatar.transform.GetChild(0).GetComponent<Image>().color = kingdom.color;
            string name = kingdom.name;
            avatar.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = name;
            if (Client.main.roomPlayers.ContainsKey(kingdom.hash))
            {
                avatar.GetComponent<Button>().onClick.AddListener(delegate { chat.OnTapOnAvatar(kingdom.hash); });
                avatar.transform.GetChild(2).gameObject.SetActive(false);
            }
            else
            {
                avatar.transform.GetChild(2).gameObject.SetActive(true);
            }
            avatars.Add(kingdom.hash, avatar);
        }
    }

    public override void OnGameMetaDataUpdate(List<Kingdom> kingdoms)
    {
        BuildAvatars(kingdoms);
    }
}
