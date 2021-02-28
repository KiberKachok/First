using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using RealmsNetwork;

public class GUIController : NetworkedMonoBehaviour
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
        int units = Mathf.Clamp(Mathf.RoundToInt(_gameCore.SelectedRegion.Units * sliderValue), 0, _gameCore.SelectedRegion.Units);
        Client.main.SendUnits(_gameCore.SelectedRegion, _gameCore.EndRegion, _gameCore.SelectedRegion.kingdom.id, units);
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
}