using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private KeyCode reloadSceneKey, pauseKey, quitKey;

    private PlayerScript playerScript;
    
    [SerializeField]
    GameObject pauseScreen;

    private void Start()
    {
        // Lock cursor to window
        Cursor.lockState = CursorLockMode.Locked;

        playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
    }

    private void Update()
    {
        // Disable game on death
        if(playerScript.currentHealth == 0)
        {
            Time.timeScale = 0;
        }

        // Reload scene
        if(Input.GetKeyDown(reloadSceneKey))
        {
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.name);

            // Dying sets timescale to 0 so we need to set it back to one in case the player died
            Time.timeScale = 1;
        }

        // Pause
        if(Input.GetKeyDown(pauseKey))
        {
            Time.timeScale = (Time.timeScale == 0) ? 1 : 0;

            pauseScreen.SetActive(!pauseScreen.activeSelf);
        }

        // Quit
        if (Input.GetKeyDown(quitKey))
        {
            Application.Quit();
        }
    }
}