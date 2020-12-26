using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScreen : MonoBehaviour
{
    [SerializeField] private RectTransform loadScreen;
    [SerializeField] private TextMeshProUGUI loadScreenText;

    public string[] quotes;
    
    public void Multiplayer()
    {
        LoadScene("Menu");
    }

    public void LoadScene(string sceneName)
    {
        loadScreen.gameObject.SetActive(true);
        //loadScreenText.text = quotes[Random.Range(0, quotes.Length)];
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            loadScreenText.text = Math.Truncate(asyncLoad.progress * 100) + "%";
            yield return null;
        }
    }
}
