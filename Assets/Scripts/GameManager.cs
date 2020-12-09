using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Sirenix.Serialization;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviourPunCallbacks
{
    public Team ownTeam;
    public List<Race> races = new List<Race>();
    public List<Team> teams = new List<Team>();
    public List<Region> regions = new List<Region>();
    public static GameManager main;
    public Race neutral;

    private void Awake()
    {
        PhotonPeer.RegisterType(typeof(Race), 255, SerializeRace, DeserializeRace);
        PhotonPeer.RegisterType(typeof(Region), 254, SerializeRegion, DeserializeRegion);
        main = this;
    }

    private void Start()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            Player[] _players = PhotonNetwork.CurrentRoom.Players.Values.ToArray();
            
            Race[] _races = new Race[_players.Length];
            for (int i = 0; i < _players.Length; i++) _races[i] = races[Random.Range(0, races.Count)];

            Region[] _regions = new Region[_races.Length];
            List<Region> ableRegions = regions.GetRange(0, regions.Count);
            for (int i = 0; i < _races.Length; i++)
            {
                //_regions[i] = ableRegions.Where(p => p.Team == null && p.mainRace == _races[i]).ToArray()[0];
                ableRegions.Remove(_regions[i]);
            }
            
            photonView.RPC("Initialize", RpcTarget.All, _players, _races, _regions);
        }
    }

    private void Update()
    {
        List<int> units = new List<int>();
        foreach (var region in regions)
        {
            units.Add(region.RecalculateUnits());
        }

        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("SetUnits", RpcTarget.Others, units);   
        }
    }

    [PunRPC]
    public void SetUnits(List<int> units)
    {
        for (int i = 0; i < regions.Count; i++)
        {
            regions[i].Units = units[i];
        }
    }

    [PunRPC]
    public void Initialize(Player[] _players, Race[] _races, Region[] _regions)
    {
        for (int i = 0; i < _players.Length; i++)
        {
            Team team = ScriptableObject.CreateInstance<Team>();
            team.Init(_players[i], _races[i]);
            teams.Add(team);
            _regions[i].Team = teams[i];
            if (PhotonNetwork.LocalPlayer == _players[i]) ownTeam = team;
        }
    }
    
    #region Utils
    
    [ContextMenu("FindRegions")]
    public void FindRegions()
    {
        regions = FindObjectsOfType<Region>().ToList();
    }
    
    [ContextMenu("InitStatic")]
    public void InitStatic()
    {
        main = this;
    }
    
    #endregion

    #region NetworkSerializer

    object DeserializeRace(byte[] data)
    {
        string s = System.Text.Encoding.UTF8.GetString(data, 0, data.Length);
        Race race = races.Find(p => p.header == s);
        return race;
    }

    byte[] SerializeRace(object race)
    {
        byte[] data = System.Text.Encoding.UTF8.GetBytes(((Race)race).header);
        return data;
    }
    
    object DeserializeRegion(byte[] data)
    {
        string s = System.Text.Encoding.UTF8.GetString(data, 0, data.Length);
        Region region = regions.Where(p => p.GetInstanceID().ToString() == s).ToArray()[0];
        return region;
    }

    byte[] SerializeRegion(object region)
    {
        byte[] data = System.Text.Encoding.UTF8.GetBytes(((Region)region).GetInstanceID().ToString());
        return data;
    }

    #endregion
}
