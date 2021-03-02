using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Sirenix.OdinInspector;
using UnityEngine.Networking;

namespace RealmsNetwork
{
    public partial class Client : SerializedMonoBehaviour
    {
        string gameConfigLink;
        public string adress;

        EventBasedNetListener listener;
        NetManager client;
        NetPeer server;

        public string version;
        public string hash;
        public string nickname;

        public int playersInGameCount = 0;
        public int playersInRoomsCount = 0;

        public Dictionary<string, Room> rooms = new Dictionary<string, Room>();
        public Dictionary<string, Player> roomPlayers = new Dictionary<string, Player>(); //Игроки вашей комнаты
        public Room room;
        public List<Kingdom> roomKingdoms = new List<Kingdom>();

        ConnectionState connectionState = ConnectionState.Disconnected;
        public ConnectionState ConnectionState
        {
            get
            {
                return connectionState;
            }
            set
            {
                connectionState = value;
                OnConnectionStateUpdate(value);
            }
        }

        public static Client main = null;
        private void Awake()
        {
            if (main != this)
            {
                if (main != null)
                {
                    Destroy(gameObject);
                    return;
                }
                DontDestroyOnLoad(gameObject);
                main = this;
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            version = Application.version;
            //string hashTmp = PlayerPrefs.GetString("hash");
            //if(hashTmp == string.Empty)
            //{
            //    hash = Guid.NewGuid().ToString().Substring(0, 8);
            //    PlayerPrefs.SetString("hash", hash);
            //}
            hash = Guid.NewGuid().ToString().Substring(0, 8);

            listener = new EventBasedNetListener();
            listener.NetworkErrorEvent += (endPoint, socketError) => OnNetworkError(endPoint, socketError);
            listener.NetworkReceiveEvent += (peer, reader, deliveryMethod) => OnNetworkReceive(peer, reader, deliveryMethod);
            listener.PeerConnectedEvent += (peer) => OnPeerConnected(peer);
            listener.PeerDisconnectedEvent += (peer, disconnectInfo) => OnPeerDisconnected(peer, disconnectInfo);
            Connect();
        }

        public void Connect()
        {
            client = new NetManager(listener);
            client.Start();
            NetDataWriter connectionData = new NetDataWriter();
            connectionData.Put(version);
            connectionData.Put(hash);
            connectionData.Put(nickname);
            client.Connect(adress, 9050, connectionData);
            Debug.Log("Client started!");
            ConnectionState = ConnectionState.Connecting;
        }

        // Update is called once per frame
        void Update()
        {
            client.PollEvents();
        }

        public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        {
            Debug.Log($"Network error: {socketError}");
        }

        public void OnNetworkReceive(NetPeer peer, NetDataReader data, DeliveryMethod deliveryMethod)
        {
            MessageType messageType = (MessageType)data.GetByte();

            switch (messageType)
            {
                case MessageType.LobbyStatisticsUpdate:
                    {
                        playersInGameCount = data.GetInt();
                        playersInRoomsCount = data.GetInt();
                        foreach (var i in callbacksTargets) i.OnLobbyStatisticsUpdate(playersInGameCount, playersInRoomsCount);
                    }
                    break;

                case MessageType.OnJoinedRoom:
                    {
                        string name = data.GetString();
                        if (rooms.ContainsKey(name))
                            room = rooms[name];
                        foreach (var i in callbacksTargets) i.OnJoinedRoom(room);
                    }
                    break;

                case MessageType.OnLeftRoom:
                    {
                        room = null;
                        roomKingdoms = new List<Kingdom>();
                        roomPlayers = new Dictionary<string, Player>();

                        foreach (var i in callbacksTargets) i.OnLeftRoom();
                        if (SceneManager.GetActiveScene().name != "Menu")
                        {
                            SceneManager.LoadScene("Menu");
                        }
                    }
                    break;

                case MessageType.RoomListUpdate:
                    {
                        string[] roomsNames = data.GetStringArray();
                        string[] roomsPlayersNames = data.GetStringArray();
                        int[] roomsCurrentPlayersCount = data.GetIntArray();
                        int[] roomsMaxPlayersCount = data.GetIntArray();
                        bool[] roomsIsGameStarted = data.GetBoolArray();
                        string[] roomsHashes = data.GetStringArray();

                        string info = $"rooms ({roomsNames.Length}): \n";

                        for (int i = 0; i < roomsNames.Length; i++)
                        {
                            string roomName = roomsNames[i];
                            string playersNames = roomsPlayersNames[i];
                            int currentPlayersCount = roomsCurrentPlayersCount[i];
                            int maxPlayersCount = roomsMaxPlayersCount[i];
                            bool isGameStarted = roomsIsGameStarted[i];
                            string hashes = roomsHashes[i];

                            Room room = null;

                            if (rooms.ContainsKey(roomName))
                            {
                                room = rooms[roomName];
                                room.name = roomName;
                                room.playersNames = playersNames;
                                room.currentPlayersCount = currentPlayersCount;
                                room.maxPlayersCount = maxPlayersCount;
                                room.isGameStarted = isGameStarted;
                                room.hashes = hashes;
                            }
                            else
                            {
                                room = new Room(roomName, playersNames, currentPlayersCount, maxPlayersCount, isGameStarted, hashes);
                                rooms.Add(roomName, room);
                            }

                            info += $"{roomName} {currentPlayersCount.ToString()} / {maxPlayersCount.ToString()} \n";
                        }

                        string[] roomsNamesToDelete = rooms.Where(p => !roomsNames.Contains(p.Value.name)).Select(p => p.Value.name).ToArray();
                        for (int i = 0; i < roomsNamesToDelete.Length; i++)
                            rooms.Remove(roomsNamesToDelete[i]);
                        foreach (var i in callbacksTargets) i.OnRoomListUpdate(rooms);
                    }
                    break;

                case MessageType.OnRoomPlayersListUpdate:
                    {
                        string[] nicknames = data.GetStringArray();
                        string[] hashes = data.GetStringArray();

                        roomPlayers = new Dictionary<string, Player>();
                        for (int i = 0; i < nicknames.Length; i++)
                        {
                            Player player = new Player(nicknames[i], hashes[i]);
                            roomPlayers.Add(player.hash, player);
                        }
                        foreach (var i in callbacksTargets) i.OnRoomPlayersListUpdate(roomPlayers);
                    }
                    break;

                case MessageType.OnGameStarted:
                    {
                        if (SceneManager.GetActiveScene().name != "Game")
                        {
                            SceneManager.LoadScene("Game");
                        }
                    }
                    break;

                case MessageType.GameMetaDataUpdate:
                    {
                        string[] rulersHashes = data.GetStringArray();
                        string[] rulersNicknames = data.GetStringArray();

                        List<Kingdom> kingdoms = new List<Kingdom>();
                        for (int i = 0; i < rulersHashes.Length; i++)
                        {
                            Kingdom kingdom = new Kingdom(i, rulersNicknames[i], rulersHashes[i], palette.GetColor(i));
                            kingdoms.Add(kingdom);
                        }
                        roomKingdoms = kingdoms;
                        foreach (var i in callbacksTargets) i.OnGameMetaDataUpdate(roomKingdoms);
                    }
                    break;

                case MessageType.GameDataUpdate:
                    {
                        foreach (var i in callbacksTargets) i.OnGameDataUpdate(data);
                    }
                    break;

                case MessageType.OnSendUnits:
                    {
                        int fromId = data.GetInt();
                        int toId = data.GetInt();
                        int kingdomId = data.GetInt();
                        int units = data.GetInt();
                        foreach (var i in callbacksTargets) i.OnSendUnits(fromId, toId, kingdomId, units);
                    }
                    break;

                case MessageType.OnMessageReceived:
                    {
                        string senderHash = data.GetString();
                        string message = data.GetString();
                        foreach (var i in callbacksTargets) i.OnMessageReceived(senderHash, message);
                    }
                    break;

                case MessageType.OnKingdomCaptured:
                    {
                        int kingdomId = data.GetInt();
                        foreach (var i in callbacksTargets) i.OnKingdomCaptured(kingdomId);
                    }
                    break;
            }
        }

        public void OnPeerConnected(NetPeer peer)
        {
            server = peer;
            ConnectionState = ConnectionState.Connected;
            Debug.Log($"Connected to server: {peer.EndPoint}");
        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            Debug.Log($"Disconnected from server: {disconnectInfo.Reason}");
            room = null;
            roomKingdoms = new List<Kingdom>();
            roomPlayers = new Dictionary<string, Player>();
            rooms = new Dictionary<string, Room>();
            playersInGameCount = 0;
            playersInRoomsCount = 0;
            ConnectionState = ConnectionState.Disconnected;

            if (SceneManager.GetActiveScene().name != "Menu")
            {
                SceneManager.LoadScene("Menu");
            }

            Connect();
        }

        public void OnApplicationQuit()
        {
            client.Stop();
        }
    }

    public enum MessageType
    {
        LobbyStatisticsUpdate,
        OnRoomPlayersListUpdate,
        GameMetaDataUpdate,
        GameDataUpdate,
        RoomListUpdate,

        LeaveRoomRequest,
        CreateRoomRequest,
        JoinRoomRequest,
        StartGameRequest,
        ChangeNicknameRequest,
        SendUnitsRequest,
        SendMessageRequest,

        OnJoinedRoom,
        OnLeftRoom,
        OnMessageReceived,
        OnGameStarted,
        OnSendUnits,
        OnKingdomCaptured
    }

    public enum ConnectionState
    {
        Connecting,
        Connected,
        Disconnected
    }
}
