using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RealmsNetwork;
using System.Linq;
using TMPro;

public class UIRooms : NetworkedMonoBehaviour
{
    public Transform roomsScrollViewTransform;
    public GameObject roomPrefab;
    public GameObject openRoomsLabelPrefab;
    public GameObject otherRoomsLabelPrefab;
    public GameObject yourRoomsLabelPrefab;
    public GameObject noRoomsLabelPrefab;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnRoomListUpdate(Dictionary<string, Room> roomsDict)
    {
        foreach(Transform child in roomsScrollViewTransform)
            Destroy(child.gameObject);

        string hash = Client.main.hash;
        Room[] rooms = roomsDict.Values.ToArray();

        Room[] openRooms = rooms.Where(room => !room.isGameStarted).ToArray(); //Новые комнаты
        Room[] yourRooms = rooms.Except(openRooms).Where(room => room.hashes.Contains(hash)).ToArray(); //Ваши комнаты, игра уже идёт
        Room[] otherRooms = rooms.Except(openRooms).Except(yourRooms).ToArray(); //Комнаты для наблюдения

        //Ваши
        //Открытые
        //Наблюдать

        if(yourRooms.Length > 0)
        {
            GameObject yourRoomsLabel = Instantiate(yourRoomsLabelPrefab, roomsScrollViewTransform);
            yourRoomsLabel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"продолжить игру: {yourRooms.Length}";

            for (int i = 0; i < yourRooms.Length; i++)
            {
                DrawRoom(yourRooms[i]);
            }
        }

        if (openRooms.Length == 0)
        {
            GameObject openRoomsLabel = Instantiate(openRoomsLabelPrefab, roomsScrollViewTransform);
            openRoomsLabel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"открытые комнаты: 0";
            GameObject noRoomsLabel = Instantiate(noRoomsLabelPrefab, roomsScrollViewTransform);
        }
        else
        {
            GameObject openRoomsLabel = Instantiate(openRoomsLabelPrefab, roomsScrollViewTransform);
            openRoomsLabel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"открытые комнаты: {openRooms.Length}";

            for (int i = 0; i < openRooms.Length; i++)
            {
                DrawRoom(openRooms[i]);
            }
        }

        if(otherRooms.Length > 0)
        {
            GameObject openRoomsLabel = Instantiate(openRoomsLabelPrefab, roomsScrollViewTransform);
            openRoomsLabel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"наблюдать: {otherRooms.Length}";

            for (int i = 0; i < otherRooms.Length; i++)
            {
                DrawRoom(otherRooms[i]);
            }
        }
    }

    public void DrawRoom(Room room)
    {
        GameObject roomObject = Instantiate(roomPrefab, roomsScrollViewTransform);
        roomObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"{room.currentPlayersCount}/{room.maxPlayersCount}";
        roomObject.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = $"{room.playersNames}";
        roomObject.GetComponent<Button>().onClick.AddListener(delegate { Client.main.JoinRoom(room.name); });
    }
}
