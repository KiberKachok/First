using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class RoomLayout : MonoBehaviourPunCallbacks
{
    [SerializeField] private RectTransform roomPanelRect;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private float slideSpeed = 120f;

    private Vector2 _destination;
    [ReadOnly, SerializeField] private Vector2 _leftPosition;
    [ReadOnly, SerializeField] private Vector2 _rightPosition;
    private float _maxAlpha;
    private RoomPanel _roomPanel;
    public static RoomLayout main;
    
    void Start()
    {
        main = this;
        _roomPanel = FindObjectOfType<RoomPanel>();
        backgroundImage.enabled = true;
        _rightPosition = roomPanelRect.anchoredPosition;
        _leftPosition = new Vector2(0, 0);
        _destination = _rightPosition;
        _maxAlpha = backgroundImage.color.a;
    }
    
    void Update()
    {
        float proportion = (roomPanelRect.sizeDelta.x - roomPanelRect.anchoredPosition.x) / roomPanelRect.sizeDelta.x;
        roomPanelRect.anchoredPosition = Vector2.MoveTowards(roomPanelRect.anchoredPosition, _destination, slideSpeed * Time.deltaTime);
        backgroundImage.color = new Color(backgroundImage.color.r, backgroundImage.color.g, backgroundImage.color.b, proportion * _maxAlpha);
    }

    public override void OnJoinedRoom()
    {
        ShowRoomPanel();
    }

    public override void OnLeftRoom()
    {
        HideRoomPanel();
    }

    public void ShowRoomPanel()
    {
        _destination = _leftPosition;
        backgroundImage.raycastTarget = true;
    }

    public void HideRoomPanel()
    {
        _destination = _rightPosition;
        backgroundImage.raycastTarget = false;
    }
}
