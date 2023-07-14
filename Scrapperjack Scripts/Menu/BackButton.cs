using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackButton : MonoBehaviour
{
    [SerializeField]
    private string mainMenu;

    // Used by back button in settings/credits
    public void goBack()
    {
        SceneManager.LoadSceneAsync(mainMenu);
    }
}