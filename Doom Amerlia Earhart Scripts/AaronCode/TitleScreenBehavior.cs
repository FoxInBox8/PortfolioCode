using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreenBehavior : MonoBehaviour
{
    
    public void PlayGame() 
    {
        SceneManager.LoadScene("ilanScene(Level Sketch)");
    }

    public void Settings() 
    {

    }

    public void QuitGame() 
    {
        Application.Quit();

        Debug.Log("Game Quit");
    }
}
