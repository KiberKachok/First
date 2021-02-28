using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RealmsNetwork;
using System.Linq;
using TMPro;


public class UIGamePanel : NetworkedMonoBehaviour
{
    public float slidingSpeed = 5;
    SlidingMode slidingMode = SlidingMode.SlidingOut;

    public GameObject roomPanel;
    public RectTransform roomContentPanelRectTransform;
    public GameObject background;
    public Image backgroundImage;
    public float maxBackgroundAlpha;
    private Vector2 roomWayPoints;

    // Start is called before the first frame update
    void Start()
    {
        roomWayPoints = new Vector2(0f, roomContentPanelRectTransform.sizeDelta.x);
        maxBackgroundAlpha = backgroundImage.color.a;
    }

    // Update is called once per frame
    void Update()
    {
        //float direction = slidingDirection == SlidingDirection.SlidingOut ? 1 : -1;
        //Vector2 target = new Vector2(slidingDirection == SlidingDirection.SlidingOut ? 0 : roomContentPanelRectTransform.localScale.x, roomContentPanelRectTransform.anchoredPosition.y);
        //roomContentPanelRectTransform.anchoredPosition = Vector2.MoveTowards(roomContentPanelRectTransform.anchoredPosition, target, slidingSpeed * Time.deltaTime);

        //if (roomContentPanelRectTransform.anchoredPosition == target)
        //{
        //    if (slidingDirection == SlidingDirection.SlidingIn)
        //    {

        //    }
        //    else
        //    {
        //        roomPanel.SetActive(false);
        //    }
        //}


        Vector2 target = new Vector2(slidingMode == SlidingMode.SlidingIn ? roomWayPoints.x : roomWayPoints.y, 0);
        Vector2 current = roomContentPanelRectTransform.anchoredPosition;

        float alpha = maxBackgroundAlpha * (1 - current.x / roomWayPoints.y);
        var color = backgroundImage.color;
        color = new Color(color.r, color.g, color.b, alpha);
        backgroundImage.color = color;

        roomContentPanelRectTransform.anchoredPosition = Vector2.MoveTowards(current, target, slidingSpeed * Time.deltaTime);

        if (slidingMode == SlidingMode.SlidingOut && current == target)
        {
            roomPanel.SetActive(false);
        }
    }

    public void SlideIn()
    {
        slidingMode = SlidingMode.SlidingIn;
        roomPanel.SetActive(true);
    }

    public void SlideOut()
    {
        slidingMode = SlidingMode.SlidingOut;
    }

    enum SlidingMode
    {
        SlidingIn,
        SlidingOut
    }

    public override void OnJoinedRoom(Room room)
    {
        SlideIn();
    }

    public override void OnLeftRoom()
    {
        SlideOut();
    }
}
