using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GUIController : MonoBehaviourPunCallbacks
{
    public int currentUnits;
    public int maxUnits;
    public float sliderValue = 1;
    
    public GameObject sliderGameObject;
    public TextMeshProUGUI currentUnitsText;
    public TextMeshProUGUI maxUnitsText;
    
    public GameObject avatarPrefab;
    public Transform avatarPanel;
    public Dictionary<string, GameObject> avatars = new Dictionary<string, GameObject>();
    
    private GameCore _gameCore;
    private Chat _chat;

    private void Awake()
    {
        _gameCore = FindObjectOfType<GameCore>();
        _chat = FindObjectOfType<Chat>();
    }

    private void Update()
    {
        if (_gameCore.SelectedRegion && _gameCore.EndRegion)
        {
            maxUnits = _gameCore.SelectedRegion.Units;
            maxUnitsText.text = maxUnits.ToString();
            currentUnits = Mathf.Clamp(Mathf.RoundToInt(_gameCore.SelectedRegion.Units * sliderValue), 0, maxUnits);
            currentUnitsText.text = currentUnits.ToString();
        }
    }

    public void OnSliderValueChanged(float v)
    {
        sliderValue = v;
    }

    public void CancelSend()
    {
        _gameCore.SelectedRegion = null;
        _gameCore.EndRegion = null;
    }
    
    public void AgreeSend()
    {
        _gameCore.SendUnits(_gameCore.SelectedRegion.id, _gameCore.EndRegion.id, _gameCore.ownKingdom.id, sliderValue);
        _gameCore.SelectedRegion = null;
        _gameCore.EndRegion = null;
        
    }
    
    public void ShowSlider()
    {
        sliderGameObject.transform.GetChild(1).GetComponent<Slider>().value = 1f;
        sliderGameObject.SetActive(true);
    }

    public void HideSlider()
    {
        sliderGameObject.SetActive(false);
    }

    public void BuildAvatars(Kingdom[] kingdoms)
    {
        foreach (var kingdom in kingdoms)
        {
            GameObject avatar = Instantiate(avatarPrefab, avatarPanel);
            avatar.transform.GetChild(0).GetComponent<Image>().color = kingdom.color;
            string name = kingdom.name;
            Player p = PhotonNetwork.PlayerList[kingdom.id];
            avatar.GetComponent<Button>().onClick.
                AddListener(delegate { _chat.OnTap(kingdom.hash); });
            avatar.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = name;
            avatars.Add(PhotonNetwork.PlayerList[kingdom.id].GetHash(), avatar);
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        avatars[otherPlayer.GetHash()].transform.GetChild(2).gameObject.SetActive(true);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        avatars[newPlayer.GetHash()].transform.GetChild(2).gameObject.SetActive(false);
    }

    public void LeaveGame()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            SceneManager.LoadScene("Menu");
        }
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("Menu");
    }
}