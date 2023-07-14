using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    // Used so we don't fade in buttons more than once
    [HideInInspector]
    public bool buttonsFadedIn = false;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}