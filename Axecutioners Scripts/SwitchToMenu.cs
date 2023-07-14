using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchToMenu : MonoBehaviour
{
    private float animationCounter = 0;

    void Update()
    {
        animationCounter += Time.deltaTime;

        if (animationCounter >= 4)
        {
            SceneManager.LoadSceneAsync("MainMenu");
        }
    }
}