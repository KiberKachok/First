﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using RealmsNetwork;

public class SplashController : MonoBehaviour
{
    public string gameConfigLink;
    public string currentVersion;
    public string newestVersion;
    public int newestVersionCode;
    public int currentVersionCode;

    public Image backgroundImage;
    public AnimationCurve cameraHightPerTime;

    public TextMeshProUGUI logoText;
    public float logoShowDelay = 2f;
    public float logoShowTime = 1f;
    public AnimationCurve logoShowSpeed;
    public AnimationCurve logoCharacterSpacingSpeed;

    public GameObject updatePanel;
    public GameObject downloadLinkPrefab;

    public GameObject connectionPanel;
    public TextMeshProUGUI connectionText;
    public string lostConnectionMessage;

    public GameObject loader;
    public float loadDelayTime = 0.6f;
    public float loadHideDelayTime = 0.3f;
    public float LoadHideTime = 0.3f;
    public float showLoaderTime = 0.3f;
    public RectTransform loaderTransform;
    public RectTransform loaderProgressTransform;
    public Image loaderImage;
    public Image loaderProgressImage;
    public AnimationCurve loadingSpeed;

    public Image Background;

    // Start is called before the first frame update
    void Start()
    {
        currentVersion = Application.version;
        StartCoroutine(Init());
        StartCoroutine(CameraUp());
    }

    IEnumerator CameraUp()
    {
        bool isActivation = !Convert.ToBoolean(PlayerPrefs.GetInt("isActivation"));
        if (isActivation)
        {
            float t = 0;
            float totalTime = logoShowTime + logoShowDelay;
            float startBackgroundAlpha = backgroundImage.color.a;
            while (t < logoShowTime + logoShowDelay)
            {
                t += Time.deltaTime;
                backgroundImage.color = new Color(backgroundImage.color.r, backgroundImage.color.g, backgroundImage.color.b, startBackgroundAlpha * t / totalTime);
                Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, cameraHightPerTime.Evaluate(t / totalTime), Camera.main.transform.position.z);
                yield return null;
            }
        }
        else
        {
            
        }
    }

    IEnumerator Init()
    {
        bool isActivation = !Convert.ToBoolean(PlayerPrefs.GetInt("isActivation"));
        if (isActivation)
        {
            logoText.color = new Color(logoText.color.r, logoText.color.g, logoText.color.b, 0);
            yield return new WaitForSeconds(logoShowDelay);
            float t = 0;
            while (t < logoShowTime)
            {
                t += Time.deltaTime;
                logoText.color = new Color(logoText.color.r, logoText.color.g, logoText.color.b, logoShowSpeed.Evaluate(t / logoShowTime));
                logoText.characterSpacing = logoCharacterSpacingSpeed.Evaluate(t / logoShowTime);
                yield return null;
            }
        }
        else
        {
            logoText.color = new Color(logoText.color.r, logoText.color.g, logoText.color.b, logoShowSpeed.Evaluate(1));
            logoText.characterSpacing = logoCharacterSpacingSpeed.Evaluate(1);
        }

        Debug.Log("Загрузка Лого - окончена");

        string rawData = "";

        while (rawData == string.Empty)
        {
            UnityWebRequest www = UnityWebRequest.Get(gameConfigLink);
            yield return www.SendWebRequest();
            rawData = www.downloadHandler.text;
            if (www.isNetworkError)
            {
                Debug.Log("Connection Error");
                connectionPanel.SetActive(true);
                connectionText.text = lostConnectionMessage;
                yield return new WaitForSeconds(0.5f);
            }
        }

        connectionPanel.SetActive(false);
        string[] data = rawData.Split('\n').ToArray();
        newestVersionCode = Convert.ToInt32(data[0]);
        newestVersion = data[1];
        string serverIp = data[2];
        Debug.Log(newestVersionCode + "\n" + newestVersion);

        if (currentVersionCode >= newestVersionCode)
        {
            yield return ShowLoader();
            yield return StartLoad();
            //yield return HideLoader();
        }
        else
        {
            updatePanel.SetActive(true);
            updatePanel.GetComponent<TextMeshProUGUI>().text = "Доступно обновление! v" + newestVersion;
        }
    }

    IEnumerator ShowLoader()
    {
        yield return new WaitForSeconds(loadDelayTime);

        float loaderImageStartAlpha = loaderImage.color.a;
        float loaderProgressImageStartAlpha = loaderProgressImage.color.a;

        loaderImage.color = new Color(loaderImage.color.r, loaderImage.color.g, loaderImage.color.b, 0f);
        loaderProgressImage.color = new Color(loaderProgressImage.color.r, loaderProgressImage.color.g, loaderProgressImage.color.b, 0f);

        loader.SetActive(true);

        float t = 0;
        while (t < showLoaderTime)
        {
            t += Time.deltaTime;
            float progress = Mathf.Clamp(t / showLoaderTime, 0f, 1f);

            loaderImage.color = new Color(loaderImage.color.r, loaderImage.color.g, loaderImage.color.b, progress * loaderImageStartAlpha);
            loaderProgressImage.color = new Color(loaderProgressImage.color.r, loaderProgressImage.color.g, loaderProgressImage.color.b, progress * loaderProgressImageStartAlpha);

            yield return null;
        }
    }

    IEnumerator HideLoader()
    {
        yield return new WaitForSeconds(loadHideDelayTime);

        float loaderImageStartAlpha = loaderImage.color.a;
        float loaderProgressImageStartAlpha = loaderProgressImage.color.a;

        loaderImage.color = new Color(loaderImage.color.r, loaderImage.color.g, loaderImage.color.b, 0f);
        loaderProgressImage.color = new Color(loaderProgressImage.color.r, loaderProgressImage.color.g, loaderProgressImage.color.b, 0f);

        loader.SetActive(true);

        float t = 0;
        while (t < showLoaderTime)
        {
            t += Time.deltaTime;
            float progress = 1 - Mathf.Clamp(t / showLoaderTime, 0f, 1f);

            loaderImage.color = new Color(loaderImage.color.r, loaderImage.color.g, loaderImage.color.b, progress * loaderImageStartAlpha);
            loaderProgressImage.color = new Color(loaderProgressImage.color.r, loaderProgressImage.color.g, loaderProgressImage.color.b, progress * loaderProgressImageStartAlpha);

            yield return null;
        }

        loader.SetActive(false);
        SceneManager.LoadScene("Menu");
    }

    IEnumerator StartLoad()
    {
        float t = 0;
        Vector2 progressMaxSize = loaderTransform.sizeDelta;
        float loadingProgress = 0;

        while (loadingProgress < 1)
        {
            loadingProgress += loadingSpeed.Evaluate(loadingProgress) * Time.deltaTime * 7;
            loadingProgress = Mathf.Clamp(loadingProgress, 0, 1);

            loaderProgressTransform.sizeDelta = new Vector2(progressMaxSize.x * loadingProgress, progressMaxSize.y);

            t += Time.deltaTime;
            yield return null;
        }

        SceneManager.LoadScene("Menu");
    }

    [ContextMenu("OnVersionChanged")]
    public void OnVersionChanged()
    {
#if UNITY_EDITOR
        currentVersionCode = PlayerSettings.Android.bundleVersionCode;
#endif
    }

    [ContextMenu("ClearPrefs")]
    public void ClearPrefs()
    {
        PlayerPrefs.DeleteAll();
    }

    public void OpenLink(string url)
    {
        Application.OpenURL(url);
    }
}
