using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class UIController : MonoBehaviourPunCallbacks
{
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
}
