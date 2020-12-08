using System;
using System.Collections;
using System.Collections.Generic;
using Lean.Touch;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitsCounterGUI : MonoBehaviour
{
    [SerializeField] private float horPadding = 0.1f;
    [SerializeField] AnimationCurve scaleCurve;

    public CellType cellType;
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private RectTransform imageTransform;
    [SerializeField] private RectTransform textTransform;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private LeanPinchCamera leanPinchCamera;

    public void SetText(string message)
    {
        text.text = message;

        if (message == "0" && cellType == CellType.Water)
        {
            imageTransform.gameObject.SetActive(false);
            textTransform.gameObject.SetActive(false);
        }
        else
        {
            imageTransform.gameObject.SetActive(true);
            textTransform.gameObject.SetActive(true);
        }

        StartCoroutine(RecalculateSize());
    }

    IEnumerator RecalculateSize()
    {
        yield return null;
        imageTransform.sizeDelta = new Vector2(textTransform.sizeDelta.x + horPadding * 2, imageTransform.sizeDelta.y);
        rectTransform.sizeDelta = imageTransform.sizeDelta;
    }

    //TODO: Оптимизировать получение LeanPinchCamera в UnitsCounterGUI
    private void Start()
    {
        leanPinchCamera = Camera.main.GetComponent<LeanPinchCamera>();
    }

    private void LateUpdate()
    {
        float s = scaleCurve.Evaluate(leanPinchCamera.Zoom / leanPinchCamera.ClampMax);
        rectTransform.localScale = new Vector3(s, s, s);
    }
}
