using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class AvatarController : MonoBehaviour
{
    [SerializeField] private GameObject playerAvatarPrefab;
    Dictionary<Player, GameObject> _playersAvatars = new Dictionary<Player, GameObject>();

    public void InstantiateAvatars()
    {
        Player[] players = PhotonNetwork.PlayerList;
        foreach (var player in players)
        {
            GameObject avatar = Instantiate(playerAvatarPrefab, transform);
            _playersAvatars.Add(player, avatar);
        }
    }

    public void SetPlayerColor(Player player, Color color)
    {
        _playersAvatars[player].transform.GetChild(0).GetComponent<Image>().color = color;
    }
    
    public void SetPlayerIcon(Player player, Sprite icon)
    {
        _playersAvatars[player].transform.GetChild(1).GetComponent<Image>().sprite = icon;
    }
}
