using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;

public class SplashController : MonoBehaviour
{
    public string gameConfigLink;
    public string currentVersion;
    public string newestVersion;
    public int newestVersionCode;
    public int currentVersionCode;

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
            while (t < logoShowTime + logoShowDelay)
            {
                t += Time.deltaTime;
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
            //www.SendWebRequest();
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

        string[] data = rawData.Split('\n')[0].Split('-'); // {версия}-{название:ссылка|название:ссылка}
        newestVersionCode = Convert.ToInt32(data[0]);
        newestVersion = data[1];
        Dictionary<string, string> links = data[2]
            .Split('|')
            .Select(p => p.Split(':'))
            .Select(p => new KeyValuePair<string, string>(p[0], p[1]))
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        if (currentVersionCode >= newestVersionCode)
        {
            yield return ShowLoader();
            yield return StartLoad();
        }
        else
        {
            updatePanel.SetActive(true);
            updatePanel.GetComponent<TextMeshProUGUI>().text = "Доступно обновление! v" + newestVersion;
            
            foreach(var pair in links)
            {
                GameObject link = Instantiate(downloadLinkPrefab, updatePanel.transform);
                link.GetComponent<TextMeshProUGUI>().text = pair.Key;
                link.GetComponent<Button>().onClick.AddListener(delegate { Application.OpenURL(pair.Value); });
            }
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
}
