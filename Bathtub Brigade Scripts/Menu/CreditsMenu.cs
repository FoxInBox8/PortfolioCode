using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsMenu : MonoBehaviour
{
    // Used to go back to main menu
    public void changeScene(string sceneName) {
        SceneManager.LoadScene(sceneName);
    }
}