using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GamePanel : MonoBehaviourPunCallbacks
{
    public GameObject roomPanel;
    public GameObject roomMainPanel;
    private Vector2 _roomWayPoints;
    private RectTransform _roomPanelRectTransform;
    public SlidingMode slidingMode = SlidingMode.SlidingOut;
    public float slidingSpeed = 3f;
    public Image backgroundImage;
    public float maxBackgroundAlpha;
    public TextMeshProUGUI openRoomsText;
    public TextMeshProUGUI playersCountText;
    public Transform roomScrollViewTransform;
    public Transform playerScrollViewTransform;
    public GameObject roomPrefab;
    public GameObject playerPrefab;
    public Color activeButtonColor;
    public Color inactiveButtonColor;
    public Button startGameButton;
    public TextMeshProUGUI playerStatisticsCountText;

    public HorizontalLayoutGroup nicknamePanel;
    public GameObject profilePanel;
    public TextMeshProUGUI nicknameText;
    public TMP_InputField nicknameInputField;
    public Button setNicknameButton;

    public GameObject openRoomsLabel;
    public GameObject yourStartedRoomsLabel;
    public GameObject otherStartedRoomsLabel;

    public Network _network;

    void Start()
    {
        _roomPanelRectTransform = roomMainPanel.GetComponent<RectTransform>();
        _roomWayPoints = new Vector2(0f, _roomPanelRectTransform.sizeDelta.x);
        maxBackgroundAlpha = backgroundImage.color.a;
    }
    
    void Update()
    {
        if (slidingMode != SlidingMode.Stopped)
        {
            Vector2 target = new Vector2(slidingMode == SlidingMode.SlidingIn ? _roomWayPoints.x : _roomWayPoints.y, 0);
            Vector2 current = _roomPanelRectTransform.anchoredPosition;
            
            float alpha = maxBackgroundAlpha * (1 - current.x / _roomWayPoints.y);
            var color = backgroundImage.color;
            color = new Color(color.r, color.g, color.b, alpha);
            backgroundImage.color = color;
            
            _roomPanelRectTransform.anchoredPosition = Vector2.MoveTowards(current, target, slidingSpeed * Time.deltaTime);

            if (slidingMode == SlidingMode.SlidingOut && current == target)
            {
                slidingMode = SlidingMode.Stopped;
                roomPanel.SetActive(false);
            }
        }
    }

    public void FixedUpdate()
    {
        playerStatisticsCountText.text =
            "В лобби: " + PhotonNetwork.CountOfPlayersOnMaster + "\n" +
            "В игре: " + PhotonNetwork.CountOfPlayersInRooms;
    }

    public void CreateRoom()
    {
        if (PhotonNetwork.InRoom) PhotonNetwork.LeaveRoom();
        _network.CreateRoom();
    }
    
    public void LeaveRoom()
    {
        slidingMode = SlidingMode.SlidingOut;
        _network.LeaveRoom();
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room");
        roomPanel.SetActive(true);
        slidingMode = SlidingMode.SlidingIn;
        UpdateStartButton();
    }

    public override void OnEnable()
    {
        _network.onRoomsUpdate.AddListener(OnNetworkRoomsUpdate);
        _network.onRoomPlayersUpdate.AddListener(OnRoomPlayersUpdate);
       base.OnEnable();
    }

    public override void OnDisable()
    {
        _network.onRoomsUpdate.RemoveListener(OnNetworkRoomsUpdate);
        _network.onRoomPlayersUpdate.RemoveListener(OnRoomPlayersUpdate);
        base.OnDisable();
    }

    public void OnRoomPlayersUpdate()
    {
        DrawPlayers();
    }

    private void OnNetworkRoomsUpdate(List<RoomInfo> rooms)
    {
        DrawRooms(rooms);
    }

    public void DrawPlayers()
    {
        foreach(Transform child in playerScrollViewTransform)
        {
            Destroy(child.gameObject);
        }

        List<Player> players = PhotonNetwork.PlayerList.ToList();
        playersCountText.text = "игроков: " + PhotonNetwork.PlayerList.Length + "/∞";
        for (int i = 0; i < players.Count; i++)
        {
            GameObject playerObject = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity, playerScrollViewTransform);
            playerObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = players.ElementAt(i).GetName();
        }
    }

    private void DrawRooms(List<RoomInfo> rooms)
    {
        List<RoomInfo> openRooms = rooms.Where(p => p.IsOpen && p.PlayerCount > 0 && !p.CustomProperties.ContainsKey("Hashes")).ToList();
        List<RoomInfo> startedRooms = rooms.Where(p => p.IsOpen && p.PlayerCount > 0 && p.CustomProperties.ContainsKey("Hashes")).ToList();
        List<RoomInfo> yourStartedRooms = startedRooms.Where(p => (p.CustomProperties["Hashes"] as string).Contains(PhotonNetwork.LocalPlayer.GetHash())).ToList();
        List<RoomInfo> otherStartedRooms = startedRooms.Where(p => !(p.CustomProperties["Hashes"] as string).Contains(PhotonNetwork.LocalPlayer.GetHash())).ToList();

        foreach (Transform child in roomScrollViewTransform)
        {
            Destroy(child.gameObject);
        }

        if(yourStartedRooms.Count == 0 && openRooms.Count == 0)
        {
            GameObject roomsLabel = Instantiate(openRoomsLabel, Vector3.zero, Quaternion.identity, roomScrollViewTransform);
            roomsLabel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "открытые комнаты: " + openRooms.Count;
        }

        if(yourStartedRooms.Count > 0)
        {
            GameObject roomsLabel = Instantiate(yourStartedRoomsLabel, Vector3.zero, Quaternion.identity, roomScrollViewTransform);
            roomsLabel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "ваши комнаты: " + yourStartedRooms.Count;

            foreach(RoomInfo roomInfo in yourStartedRooms)
            {
                GameObject roomObject = Instantiate(roomPrefab, Vector3.zero, Quaternion.identity, roomScrollViewTransform);
                roomObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = roomInfo.CustomProperties["Nicknames"] as string;
                roomObject.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = roomInfo.PlayerCount + "/∞";
                roomObject.GetComponent<Button>().onClick.AddListener(delegate { _network.JoinRoom(roomInfo.Name); });
            }
        }

        if (openRooms.Count > 0)
        {
            GameObject roomsLabel = Instantiate(openRoomsLabel, Vector3.zero, Quaternion.identity, roomScrollViewTransform);
            roomsLabel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "открытые комнаты: " + openRooms.Count;

            foreach (RoomInfo roomInfo in openRooms)
            {
                GameObject roomObject = Instantiate(roomPrefab, Vector3.zero, Quaternion.identity, roomScrollViewTransform);
                roomObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = roomInfo.CustomProperties["Nicknames"] as string;
                roomObject.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = roomInfo.PlayerCount + "/∞";
                roomObject.GetComponent<Button>().onClick.AddListener(delegate { _network.JoinRoom(roomInfo.Name); });
            }
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        UpdateStartButton();
    }

    public void UpdateStartButton()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            startGameButton.interactable = true;
            startGameButton.GetComponent<Image>().color = activeButtonColor;
            startGameButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "начать";
        }
        else
        {
            startGameButton.interactable = false;
            startGameButton.GetComponent<Image>().color = inactiveButtonColor;
            startGameButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "ожидайте";
        }
    }

    public override void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
    {
        playerStatisticsCountText.text =
            "В лобби: " + PhotonNetwork.CountOfPlayersOnMaster + "\n" +
            "В игре: " + PhotonNetwork.CountOfPlayersInRooms;
    }

    public enum SlidingMode
    {
        SlidingIn,
        SlidingOut,
        Stopped
    }

    public void EditNickname()
    {
        profilePanel.SetActive(true);
        nicknameText.text = PhotonNetwork.LocalPlayer.GetName();
        nicknameInputField.text = PhotonNetwork.LocalPlayer.GetName();
        OnNicknameInputChange(PhotonNetwork.LocalPlayer.GetName());
    }

    public void OnNicknameInputChange(string message)
    {
        if(message.Length > 0)
        {
            setNicknameButton.interactable = true;
        }
        else
        {
            setNicknameButton.interactable = false;
        }
    }

    public void SetNickname()
    {
        string newName = nicknameInputField.text;
        _network.SetName(newName);
        nicknameText.text = newName;
        StartCoroutine(UpdateNicknameBorder());
        profilePanel.SetActive(false);
    }

    IEnumerator UpdateNicknameBorder()
    {
        nicknamePanel.enabled = false;
        yield return null;
        nicknamePanel.enabled = true;
    }
}
