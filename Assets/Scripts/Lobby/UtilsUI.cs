using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UtilsUI : MonoBehaviour
{
    public void ToStartScreen()
    {
        SceneManager.LoadScene("Start");
    }
}
