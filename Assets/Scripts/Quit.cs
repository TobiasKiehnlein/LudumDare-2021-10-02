using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Random = System.Random;

public class Quit : MonoBehaviour
{
#if (UNITY_WEBGL)
    [DllImport("__Internal")]
    private static extern void QuitGameJS();
#endif
    
    public void QuitGame()
    {
#if (UNITY_EDITOR)
        UnityEditor.EditorApplication.isPlaying = false;
#elif (UNITY_STANDALONE)
    Application.Quit();
#elif (UNITY_WEBGL)
    QuitGameJS();
#endif
    }
}