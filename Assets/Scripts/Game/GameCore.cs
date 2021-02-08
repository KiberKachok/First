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
using System.Globalization;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameCore : MonoBehaviourPunCallbacks, IPunObservable
{
    public Dictionary<string, Kingdom> playersKingdoms = new Dictionary<string, Kingdom>();
    public Kingdom ownKingdom;
    public Kingdom[] kingdoms;
    public Region[] regions;
    public Region[] lands;
    public Region[] waters;
    public List<UnitsController> unitsControllers = new List<UnitsController>();
    public static string ownHash;
    public string kingdomsData = "None";
    public bool isCamerAligned = false;

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

    public bool isImitatorMode = false;
    public int botsCount = 10;
    public Region[] botStartRegions;

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
        bool isActivation = !Convert.ToBoolean(PlayerPrefs.GetInt("isActivation"));

        ownHash = PhotonNetwork.LocalPlayer.GetHash();

        if (!isImitatorMode)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                Player[] _players = PhotonNetwork.PlayerList;
                List<Kingdom> _kingdoms = new List<Kingdom>();
                List<Region> _regions = new List<Region>();
                List<Region> _mixedRegions = lands.OrderBy(a => Guid.NewGuid()).ToList();

                int kingdomsCount = _players.Length;
                int[] ids = new int[kingdomsCount];
                string[] names = new string[kingdomsCount];
                string[] hashes = new string[kingdomsCount];

                for (int i = 0; i < kingdomsCount; i++)
                {
                    ids[i] = i;
                    names[i] = _players[i].GetName();
                    hashes[i] = _players[i].GetHash();

                    Kingdom k = new Kingdom(i, _players[i].GetName(), _players[i].GetHash(), palette.GetColor(i));
                    Region region = _mixedRegions[i % _mixedRegions.Count];
                    _kingdoms.Add(k);
                    _regions.Add(region);
                    playersKingdoms.Add(_players[i].GetHash(), k);
                    if (k.hash == PhotonNetwork.LocalPlayer.GetHash())
                    {
                        ownKingdom = k;
                    }
                }
                kingdoms = _kingdoms.ToArray();

                for (int i = 0; i < kingdoms.Length; i++)
                {
                    _regions[i].kingdom = kingdoms[i];
                    _regions[i].IsCapital = true;
                }

                string idsStr = string.Join(":", ids);
                string namesStr = string.Join(":", names);
                string hashesStr = string.Join(":", hashes);
                string kingdomsDataStr = kingdomsCount.ToString() + "|" + idsStr + "|" + namesStr + "|" + hashesStr;

                photonView.RPC("SetKingdomsData", RpcTarget.AllBuffered, kingdomsDataStr);
                photonView.RPC("SpawnKingdoms", RpcTarget.All, string.Join("-", _regions.Select(p => p.id)), string.Join("-", kingdoms.Select(p => p.id)));
            }
        }
        else
        {
            List<Kingdom> _kingdoms = new List<Kingdom>();
            List<Region> _regions = new List<Region>();
            List<Region> _mixedRegions = lands.OrderBy(a => Guid.NewGuid()).ToList();
            int kingdomsCount = botsCount;

            for (int i = 0; i < kingdomsCount; i++)
            {
                Kingdom k = new Kingdom(i, i.ToString(), i.ToString(), palette.GetColor(i));
                Region mainRegion;
                if (isActivation)
                {
                    mainRegion = botStartRegions[i];
                }
                else
                {
                    mainRegion = _mixedRegions[i % _mixedRegions.Count];
                }
                _mixedRegions.Remove(mainRegion);

                mainRegion.kingdom = k;
                mainRegion.IsCapital = true;
                mainRegion.Units = Random.Range(5, 16);
                mainRegion.currentUnits = Random.Range(0f, 1f);

                int provincesCount = Random.Range(0, 1);
                List<Region> _mixedNeighbours = mainRegion.neighbours.OrderBy(a => Guid.NewGuid()).ToList();

                for (int j = 0; j < provincesCount; j++)
                {
                    Region targetRegion = _mixedNeighbours[j];
                    if (targetRegion.kingdom == null && targetRegion.cellType == Region.CellType.Land)
                    {
                        _mixedRegions.Remove(targetRegion);
                        targetRegion.kingdom = k;
                        targetRegion.Units = Random.Range(2, 10);
                        targetRegion.currentUnits = Random.Range(0f, 1f);
                    }
                }

                _kingdoms.Add(k);
            }

            kingdoms = _kingdoms.ToArray();
            StartImitator();
        }
    }

    public void StartImitator()
    {
        for (int i = 0; i < kingdoms.Length; i++)
        {
            Imitator imitator = gameObject.AddComponent<Imitator>();
            imitator.kingdom = kingdoms[i];
            imitator.gameCore = this;
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient && !isImitatorMode)
        {
            photonView.RPC("SetKingdomsData", newPlayer, kingdomsData);
            SendUnitsData(newPlayer);
        }
    }

    public void SendUnitsData(Player newPlayer)
    {
        //fromId:toId:kingdomId:units:percents|
        if (unitsControllers.Count > 0)
        {
            string[] data = new string[unitsControllers.Count];
            for (int i = 0; i < unitsControllers.Count; i++)
            {
                UnitsController u = unitsControllers.ElementAt(i);
                data[i] = u.from.id + ":" + u.to.id + ":" + u.kingdom.id + ":" + u.Units + ":" + u.percentsToDest.ToString("F3", CultureInfo.InvariantCulture);
            }
            photonView.RPC("GetUnitsData", newPlayer, string.Join("|", data));
        }
    }

    [PunRPC]
    public void GetUnitsData(string dataStr)
    {
        string[] data = dataStr.Split('|').ToArray();
        for (int i = 0; i < data.Length; i++)
        {
            string[] mdata = data[i].Split(':').ToArray();
            Region from = regions[Convert.ToInt32(mdata[0])];
            Region to = regions[Convert.ToInt32(mdata[1])];
            Kingdom kingdom = kingdoms[Convert.ToInt32(mdata[2])];
            int units = Convert.ToInt32(mdata[3]);
            float percents = Convert.ToSingle(mdata[4], CultureInfo.InvariantCulture);
            Vector3 startPos = (to.transform.position - from.transform.position) * percents + from.transform.position;

            GameObject unitsObject = Instantiate(unitsPrefab, startPos, Quaternion.Euler(270 + 180, 0, -90));
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

    [PunRPC]
    public void SetKingdomsData(string kData)
    {
        if (kingdomsData == "None")
        {
            kingdomsData = kData;
            SetKingdoms(kingdomsData);
        }
    }

    public void SetKingdoms(string kingdomsData)
    {
        // id:id:id:id|name:name:name:name|hash:hash:hash:hash
        Debug.Log("Королевства получены");
        string[] data = kingdomsData.Split('|').ToArray();
        int kCount = Convert.ToInt32(data[0]);
        string idsStr = data[1];
        string namesStr = data[2];
        string hashesStr = data[3];

        List<Kingdom> _kingdoms = new List<Kingdom>();

        int kingdomsCount = kCount;
        int[] ids = idsStr.Split(':').Select(p => Convert.ToInt32(p)).ToArray();
        string[] names = namesStr.Split(':').ToArray();
        string[] hashes = hashesStr.Split(':').ToArray();

        for(int i = 0; i < kingdomsCount; i++)
        {
            int id = ids[i];
            string name = names[i];
            string hash = hashes[i];

            Kingdom k = new Kingdom(id, name, hash, palette.GetColor(id));
            _kingdoms.Add(k);

            if (hash == PhotonNetwork.LocalPlayer.GetHash())
            {
                ownKingdom = k;
            }
        }

        kingdoms = _kingdoms.ToArray();
        _guiController.BuildAvatars(kingdoms);
    }

    [PunRPC]
    public void SpawnKingdoms(string regData, string kingdomData)
    {
        Region[] _regions = regData.Split('-').Select(p => Convert.ToInt32(p)).Select(p => regions[p]).ToArray();
        Kingdom[] _kingdoms = kingdomData.Split('-').Select(p => Convert.ToInt32(p)).Select(p => kingdoms[p]).ToArray();

        for (int i = 0; i < _regions.Length; i++)
        {
            if (kingdoms[i].hash == ownKingdom.hash)
            {
                Camera.main.transform.position = new Vector3(_regions[i].transform.position.x, Camera.main.transform.position.y, _regions[i].transform.position.z);
            }
        }
    }

    [ContextMenu("Align Camera")]
    public void AlignCamera()
    {
        if(ownKingdom != null)
        {
            Region[] ownRegions = lands.Where(p => p.kingdom != null && p.kingdom.hash == ownKingdom.hash && p.IsCapital).ToArray();
            if(ownRegions.Length > 0)
            {
                Region target = ownRegions[0];
                Camera.main.transform.position = new Vector3(target.transform.position.x, Camera.main.transform.position.y, target.transform.position.z);

            }
        }
    }

    public void CaptureKingdom(Kingdom targetKingdom, Kingdom kingdom)
    {
        for(int i = 0; i < regions.Length; i++)
        {
            if(regions[i].kingdom != null && regions[i].kingdom.hash == targetKingdom.hash)
            {
                regions[i].kingdom = kingdom;
            }
        }
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
        if (_selectedRegion && _selectedRegion.kingdom != null && _selectedRegion.kingdom.hash != ownHash)
        {
            SelectedRegion = null;
        }

        if (PhotonNetwork.IsMasterClient || isImitatorMode)
        {
            foreach (var region in regions)
            {
                region.Recalculate();
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (!isImitatorMode)
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

                if (!isCamerAligned) //Выравнивание камеры после переподключения
                {
                    AlignCamera();
                    isCamerAligned = true;
                }
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
                if (rg.kingdom != null && rg.kingdom.hash == ownHash)
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
