using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Tip : MonoBehaviour
{
    
    //TODO: Make curve of position
    [SerializeField] private Transform tipTransform;
    [SerializeField] private float speed = 20f;
    [SerializeField] private float height = 10;
    private Vector3 startPosition;
    private Vector3 endPosition;
    
    private static Tip main;
    
    public Coroutine routine;
    private TextMeshProUGUI textComp;

    void Start()
    {
        main = this; 
        textComp = tipTransform.GetComponent<TextMeshProUGUI>();
        startPosition = tipTransform.position;
        endPosition = new Vector3(startPosition.x, startPosition.y - height, startPosition.z);
        textComp.color = new Color(textComp.color.r, textComp.color.g, textComp.color.b, 0);
    }

    private void show(string text)
    {
        if(routine != null) StopCoroutine(routine);
        textComp.text = text;
        routine = StartCoroutine(ShowRoutine());
    }

    private void hide()
    {
        if(routine != null) StopCoroutine(routine);
        routine = StartCoroutine(HideRoutine());
    }

    IEnumerator ShowRoutine()
    {
        tipTransform.position = startPosition;
        while (tipTransform.position != endPosition)
        {
            tipTransform.position = Vector3.Lerp(tipTransform.position, endPosition, speed * Time.deltaTime);
            
            float targetDistanceProportion = 1f - (tipTransform.position.y - endPosition.y) / (startPosition.y - endPosition.y);
            textComp.color = new Color(textComp.color.r, textComp.color.g, textComp.color.b, targetDistanceProportion);
            
            yield return null;
        }
    }
    
    IEnumerator HideRoutine()
    {
        tipTransform.position = endPosition;
        while (tipTransform.position != startPosition)
        {
            tipTransform.position = Vector3.Lerp(tipTransform.position, startPosition, speed * Time.deltaTime);
            
            float targetDistanceProportion = 1 - (tipTransform.position.y - endPosition.y) / (startPosition.y - endPosition.y);
            textComp.color = new Color(textComp.color.r, textComp.color.g, textComp.color.b, targetDistanceProportion);
            
            yield return null;
        }
    }
    
    public static void Show(string text)
    {
        main.show(text);
    }
    
    public static void Hide()
    {
        main.hide();
    }
}
