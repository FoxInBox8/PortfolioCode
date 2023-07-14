using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private string firstLevel, settings, credits, tutorial;

    // Used by start button
    public void play()
    {
        SceneManager.LoadSceneAsync(firstLevel);
    }

    // Used by settings button
    public void loadSettings()
    {
        SceneManager.LoadSceneAsync(settings);
    }

    // Used by tutorial button
    public void loadTutorial()
    {
        SceneManager.LoadSceneAsync(tutorial);
    }

    // Used by credits button
    public void loadCredits()
    {
        SceneManager.LoadSceneAsync(credits);
    }

    // Used by exit button
    public void exit()
    {
        Application.Quit();
    }
}