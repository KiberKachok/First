using System;
using System.Collections;
using System.Collections.Generic;
using Lean.Touch;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitsCounter : MonoBehaviour
{
    [SerializeField] private float horPadding = 0.1f;

    public CounterMode mode;
    private RectTransform _rectTransform;
    private RectTransform _imageTransform;
    private RectTransform _textTransform;
    private TextMeshProUGUI _text;

    public void SetText(string message)
    {
        _text.text = message;

        if (mode == CounterMode.RegionWater && message == "0")
        {
            _imageTransform.gameObject.SetActive(false);
            _textTransform.gameObject.SetActive(false);
        }
        else
        {
            _imageTransform.gameObject.SetActive(true);
            _textTransform.gameObject.SetActive(true);
        }

        StartCoroutine(RecalculateSize());
    }

    public void SetColor(Color color)
    {
        GetComponentInChildren<Image>().color = color;
    }

    IEnumerator RecalculateSize()
    {
        yield return null;
        _imageTransform.sizeDelta = new Vector2(_textTransform.sizeDelta.x + horPadding * 2, _imageTransform.sizeDelta.y);
        _rectTransform.sizeDelta = _imageTransform.sizeDelta;
    }

    //TODO: Оптимизировать получение LeanPinchCamera в UnitsCounter
    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _imageTransform = transform.GetChild(0).GetComponent<RectTransform>();
        _textTransform = transform.GetChild(1).GetComponent<RectTransform>();
        _text = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
    }
}

public enum CounterMode
{
    RegionWater,
    RegionLand,
    Units
}
