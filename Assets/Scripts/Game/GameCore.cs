using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameCore : MonoBehaviourPunCallbacks, IPunObservable
{
    public Dictionary<string, Kingdom> playersKingdoms = new Dictionary<string, Kingdom>();
    public Kingdom ownKingdom;
    public Kingdom[] kingdoms;
    public Region[] regions;
    public Region[] lands;
    public Region[] waters;
    public List<UnitsController> unitsControllers = new List<UnitsController>();

    [ShowInInspector]
    public Region SelectedRegion
    {
        get => _selectedRegion;
        set
        {
            if (value == null)
            {
                _selectedRegion.material.SetInt("isSelected", 0);
            }
            else
            {
                value.material.SetInt("isSelected", 1);
            }
            _selectedRegion = value;
            _lineDrawer.BuildLines();
        }
    }
    private Region _selectedRegion;
    
    [ShowInInspector]
    public Region EndRegion
    {
        get => _endRegion;
        set
        {
            if (value == null)
            {
                _endRegion.material.SetInt("isSelected", 0);
                _guiController.HideSlider();
            }
            else
            {
                value.material.SetInt("isSelected", 1);
                _guiController.ShowSlider();
            }
            _endRegion = value;
            _lineDrawer.BuildLines();
        }
    }
    private Region _endRegion;

    public GameObject unitsPrefab;

    public int roomSeed;
    public Palette palette;
    public GameMode gameMode;

    private GUIController _guiController;
    private Chat _chat;
    private LineDrawer _lineDrawer;
    public static GameCore main;

    public GameObject capitalMarkPrefab;
    
    void Awake()
    {
        _guiController = FindObjectOfType<GUIController>();
        _chat = FindObjectOfType<Chat>();
        _lineDrawer = FindObjectOfType<LineDrawer>();
        gameMode = PhotonNetwork.InRoom ? GameMode.Online : GameMode.Offline;
        main = this;
    }
    
    void Start()
    {
        Player[] _players = PhotonNetwork.PlayerList;
        List<Kingdom> _kingdoms = new List<Kingdom>();
        List<Region> _regions = new List<Region>();
        List<Region> _mixedRegions = lands.OrderBy(a => Guid.NewGuid()).ToList();
        for (int i = 0; i < _players.Length; i++)
        {
            Kingdom k = new Kingdom(i, _players[i].GetName(), palette.GetColor(i));
            Region region = _mixedRegions[i % _mixedRegions.Count];
            _kingdoms.Add(k);
            _regions.Add(region);
            playersKingdoms.Add(_players[i].GetHash(), k);
            if (_players[i].GetHash() == PhotonNetwork.LocalPlayer.GetHash())
            {
                ownKingdom = k;
            }
        }
        kingdoms = _kingdoms.ToArray();
        if (PhotonNetwork.IsMasterClient)
        {
            for(int i = 0; i < kingdoms.Length; i++)
            {
                _regions[i].kingdom = kingdoms[i];
                _regions[i].IsCapital = true;
            }
            photonView.RPC("SpawnKingdoms", RpcTarget.All, string.Join("-", _regions.Select(p => p.id)), string.Join("-", kingdoms.Select(p => p.id)));
        }
        _guiController.BuildAvatars(kingdoms);
    }

    [PunRPC]
    public void SpawnKingdoms(string regData, string kingdomData)
    {
        Region[] _regions = regData.Split('-').Select(p => Convert.ToInt32(p)).Select(p => regions[p]).ToArray();
        Kingdom[] _kingdoms = kingdomData.Split('-').Select(p => Convert.ToInt32(p)).Select(p => kingdoms[p]).ToArray();

        for(int i = 0; i < _regions.Length; i++)
        {
            if (kingdoms[i] == ownKingdom)
            {
                Camera.main.transform.position = new Vector3(_regions[i].transform.position.x, Camera.main.transform.position.y, _regions[i].transform.position.z);
            }
        }
    }

    public void CaptureKingdom(Kingdom targetKingdom, Kingdom kingdom)
    {
        for(int i = 0; i < regions.Length; i++)
        {
            if(regions[i].kingdom == targetKingdom)
            {
                regions[i].kingdom = kingdom;
            }
        }
        regions.Where(p => p.kingdom != null).Where(p => p.kingdom.id == targetKingdom.id).Select(p => p.kingdom = kingdom);
        photonView.RPC("DestroyKingdomUnits", RpcTarget.All, targetKingdom.id);
    }

    [PunRPC]
    public void DestroyKingdomUnits(int kingdomId)
    {
        Kingdom kingdom = kingdoms[kingdomId];
        UnitsController[] ucs = unitsControllers.Where(p => p.kingdom == kingdom).ToArray();
        for(int i = 0; i < ucs.Length; i++)
        {
            unitsControllers.Remove(ucs[i]);
            Destroy(ucs[i].gameObject);
        }
    }

    private void Update()
    {
        if(_selectedRegion && _selectedRegion.kingdom != ownKingdom)
        {
            SelectedRegion = null;
        }

        if (PhotonNetwork.IsMasterClient)
        {
            foreach (var region in regions)
            {
                region.Recalculate();
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            List<string> capitals = new List<string>();
            List<string> data = new List<string>();
            foreach (var region in regions)
            {
                if (region.IsCapital)
                {
                    capitals.Add(region.id.ToString());
                }
                data.Add(region.ToStringFull());
            }
            string message = string.Join("-", capitals) + "|" + string.Join("-", data);
            stream.SendNext(message);
        }
        else
        {
            string message = (string)stream.ReceiveNext();
            string[] cdata = message.Split('|').ToArray();
            int[] capitals = cdata[0].Split('-').Select(p => Convert.ToInt32(p)).ToArray();
            string[] data = cdata[1].Split('-');
            for (int i = 0; i < regions.Length; i++)
            {
                string[] d = data[i].Split(':');
                regions[i].Units = Convert.ToInt32(d[1]);
                if (d[0] == "#")
                {
                    regions[i].kingdom = null;
                }
                else
                {
                    regions[i].kingdom = kingdoms[Convert.ToInt32(d[0])];
                }
                regions[i].IsCapital = capitals.Contains(regions[i].id);
            }
        }
    }


    public void OnTap(Vector3 pos)
    {
        if (_chat.isSenderOpened())
        {
            _chat.HideSender();
            return;
        }
        
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            Region rg = hit.transform.GetComponent<Region>();

            if (SelectedRegion)
            {
                if (EndRegion)
                {
                    SelectedRegion = null;
                    EndRegion = null;
                }
                else
                {
                    if (SelectedRegion.neighbours.Contains(rg))
                    {
                        EndRegion = rg;
                    }
                    else
                    {
                        SelectedRegion = null;
                    }
                }
            }
            else
            {
                if (rg.kingdom == ownKingdom)
                {
                    SelectedRegion = rg;
                }
            }
        }
    }

    [PunRPC]
    public void SendUnits(int fromId, int toId, int kingdomId, float percent)
    {
        Region from = regions[fromId];
        Region to = regions[toId];
        Kingdom kingdom = kingdoms[kingdomId];
        
        if (PhotonNetwork.IsMasterClient)
        {
            int units = Mathf.Clamp(Mathf.RoundToInt(from.Units * percent), 0, from.Units);
            from.Units -= units;
            photonView.RPC("SpawnUnits", RpcTarget.All, fromId, toId, kingdom.id, units);
            PhotonNetwork.SendAllOutgoingCommands();
        }
        else
        {
            photonView.RPC("SendUnits", RpcTarget.MasterClient, fromId, toId, kingdom.id, percent);
            PhotonNetwork.SendAllOutgoingCommands();
        }
    }

    [PunRPC]
    public void SpawnUnits(int fromId, int toId, int kingdomId, int units)
    {
        Region from = regions[fromId];
        Region to = regions[toId];
        Kingdom kingdom = kingdoms[kingdomId];

        if (units > 0)
        {
            GameObject unitsObject = Instantiate(unitsPrefab, from.transform.position, Quaternion.Euler(270 + 180, 0, -90));
            TextMeshPro t = unitsObject.transform.GetChild(0).GetComponent<TextMeshPro>();
            t.text = units.ToString();
            t.color = kingdom.color;
            UnitsController unitsController = unitsObject.GetComponent<UnitsController>();
            unitsController.Units = units;
            unitsController.from = from;
            unitsController.to = to;
            unitsController.kingdom = kingdom;
            unitsControllers.Add(unitsController);
        }
    }

    [ContextMenu("SetIds")]
    public void SetIds()
    {
        for (int i = 0; i < regions.Length; i++)
        {
            regions[i].id = i;
        }
    }

    public enum GameMode
    {
        Online,
        Offline
    }
}
