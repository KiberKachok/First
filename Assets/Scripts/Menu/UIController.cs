using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class UIController : MonoBehaviourPunCallbacks
{
    public TextMeshProUGUI versionText;

    void Start()
    {

    }

    void Update()
    {
        
    }

    public void OpenLink(string url)
    {
        Application.OpenURL(url);
    }

    [ContextMenu("OnVersionChanged")]
    public void OnVersionChanged()
    {
#if UNITY_EDITOR
        versionText.text = "v" + PlayerSettings.bundleVersion + " (" + PlayerSettings.Android.bundleVersionCode + ")";
#endif
    }
}
