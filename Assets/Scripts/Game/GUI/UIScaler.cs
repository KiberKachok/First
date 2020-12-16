using System;
using System.Collections;
using System.Collections.Generic;
using Lean.Touch;
using UnityEngine;

public class UIScaler : MonoBehaviour
{
    [SerializeField] private AnimationCurve scaleCurve;
    [SerializeField] private float scaleFactor = 8;

    private RectTransform _rectTransform;
    private LeanPinchCamera _leanPinchCamera;

    void Awake()
    {
        _leanPinchCamera = Camera.main.GetComponent<LeanPinchCamera>();
        _rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        float s = scaleCurve.Evaluate(_leanPinchCamera.Zoom / _leanPinchCamera.ClampMax);
        s *= scaleFactor;
        _rectTransform.localScale = new Vector3(s, s, s);
    }
}
