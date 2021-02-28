using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIUtils : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        PlayerPrefs.SetInt("isActivation", 1);
    }
}
