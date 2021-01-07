using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Random = UnityEngine.Random;

public class Pick : MonoBehaviourPunCallbacks
{
    public Race[] _racesPerPlayer;
    [SerializeField] private GameObject pickPanel;
    [SerializeField] private GameObject timeLoader;
    [SerializeField] private GameObject timeLoaderChild;
    private GameManager _gameManager;
    public float time = 15f;
    
    
    private void Start()
    {
        _racesPerPlayer = new Race[PhotonNetwork.PlayerList.Length];
        _gameManager = GetComponent<GameManager>();
        _gameManager.avatarController.InstantiateAvatars();
        StartCoroutine(PickTimeout());
    }

    public void ChooseRace(Race race)
    {
        photonView.RPC("OnPlayersChosenRaces", RpcTarget.All, new Player[]{PhotonNetwork.LocalPlayer}, new Race[]{race});
    }

    [PunRPC]
    public void OnPlayersChosenRaces(Player[] players, Race[] races)
    {
        for (int i = 0; i < players.Length; i++)
        {
            Player player = players[i];
            Race race = races[i];
            
            _racesPerPlayer[player.ActorNumber - 1] = race;
            _gameManager.avatarController.SetPlayerColor(player, _gameManager.GetPlayerColor(player));
            _gameManager.avatarController.SetPlayerIcon(player, race.icon);
            if (PhotonNetwork.IsMasterClient && _racesPerPlayer.Count(p => p == null) == 0)
            {
                photonView.RPC("EndPick", RpcTarget.All);
            }
        }
    }

    [PunRPC]
    public void EndPick()
    {
        pickPanel.SetActive(false);
        if (PhotonNetwork.IsMasterClient)
        {
            _gameManager.StartGame(PhotonNetwork.PlayerList, _racesPerPlayer);   
        }
    }

    IEnumerator PickTimeout()
    {
        float t = time;
        
        float loaderMax =  timeLoader.GetComponent<RectTransform>().sizeDelta.x;
        RectTransform loaderRect = timeLoaderChild.GetComponent<RectTransform>();
        
        while (t > 0)
        {
            t -= Time.deltaTime;
            loaderRect.sizeDelta = new Vector2((t / time) * loaderMax, loaderRect.sizeDelta.y);
            yield return null;
        }

        if (PhotonNetwork.IsMasterClient)
        {
            List<int> ids = new List<int>();

            for (int i = 0; i < _racesPerPlayer.Length; i++)
            {
                if (_racesPerPlayer[i] == null)
                {
                    ids.Add(i + 1);
                }
            }

            Player[] players = PhotonNetwork.PlayerList.Where(p => ids.Contains(p.ActorNumber)).ToArray();
            Race[] races = players.Select(p => _gameManager.races[Random.Range(0, _gameManager.races.Count)]).ToArray();
            
            photonView.RPC("OnPlayersChosenRaces", RpcTarget.All, players, races);
        }
    }
}
