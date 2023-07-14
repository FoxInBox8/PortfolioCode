using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Cinemachine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private string nextLevel;

    [SerializeField]
    private GameObject pausePanel, winPanel, lossPanel, settingsPanel, resumeButton, nextLevelButton, settingsButton;

    [SerializeField]
    private Slider healthBar;

    [SerializeField]
    private bool timerDisabled;

    public bool isPaused { get; private set; } = false;

    private Timer timer;
    private TMP_Text timerText;
    private Settings settings;
    private CinemachineFreeLook cam;
    private AudioManager am;
    private PlayerControls playerInput;

    private const int TIMER_TEXT_INDEX = 1;

    private void Start()
    {
        // Make sure nothing active that shouldn't be
        pausePanel.SetActive(false);
        winPanel.SetActive(false);
        lossPanel.SetActive(false);
        settingsPanel.SetActive(false);

        playerInput = FindObjectOfType<PlayerScript>().getInput();
        settings = FindObjectOfType<Settings>();
        cam = FindObjectOfType<CinemachineFreeLook>();
        am = FindObjectOfType<AudioManager>();
        timer = FindObjectOfType<Timer>();

        // Lock cursor to window
        Cursor.lockState = CursorLockMode.Locked;

        // Get timer text
        timerText = winPanel.transform.GetChild(TIMER_TEXT_INDEX).GetComponent<TMP_Text>();

        // Make sure we don't start paused
        Time.timeScale = 1;

        updateSettings();
    }

    private void Update()
    {
        // Debug commands for demo night
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            SceneManager.LoadSceneAsync("Level1");
        }

        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SceneManager.LoadSceneAsync("Level2");
        }

        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SceneManager.LoadSceneAsync("Level3");
        }

        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SceneManager.LoadSceneAsync("Level4");
        }

        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            SceneManager.LoadSceneAsync("Level5");
        }

        // Do nothing if win screen active
        if (winPanel.activeSelf) { return; }

        // If player lost, restart on left click/a pressed
        if (lossPanel.activeSelf && playerInput.Player.Restart.WasPerformedThisFrame())
        {
            restart();
        }

        // Toggle pausing when key pressed and player has not lost
        if(playerInput.Player.Pause.WasPerformedThisFrame() && !lossPanel.activeSelf)
        {
            // Return to pause menu if in settings menu
            if(settingsPanel.activeSelf)
            {
                exitSettings();
            }

            else if (isPaused)
            {
                unpauseGame();
            }

            else
            {
                pauseGame();
            }
        }
    }

    private void pauseGame()
    {
        isPaused = true;
        
        // Stop game
        Time.timeScale = 0;

        // Disable game UI
        healthBar.gameObject.SetActive(false);
        pausePanel.SetActive(true);

        // Enable cursor
        Cursor.lockState = CursorLockMode.None;

        am.pauseAll(new string[] {"Music"});
        am.play("Pause");

        // Disable camera
        cam.m_XAxis.m_MaxSpeed = 0;
        cam.m_YAxis.m_MaxSpeed = 0;

        // Set selected button
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(resumeButton);
    }

    private void unpauseGame()
    {
        isPaused = false;

        am.play("Unpause");
        am.unpauseAll();

        // Resume game
        Time.timeScale = 1;

        // Enable game UI
        healthBar.gameObject.SetActive(true);
        pausePanel.SetActive(false);

        // Disable cursor
        Cursor.lockState = CursorLockMode.Locked;
        
        // Enable camera
        cam.m_XAxis.m_MaxSpeed = settings.getSetting(Settings.SETTINGS_TYPE.X_SENSITIVITY);
        cam.m_YAxis.m_MaxSpeed = settings.getSetting(Settings.SETTINGS_TYPE.Y_SENSITIVITY);
    }

    public void updateSettings()
    {
        settings.saveSettings();

        // Update volume
        am.setGlobalVolume();
    }

    public void playerWon()
    {
        pauseGame();

        // Make sure pause panel not active
        pausePanel.SetActive(false);

        winPanel.SetActive(true);

        am.stopAll(new string[] { "Level_Music_Melody", "Level_Music_Beat" });

        // Set selected button
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(nextLevelButton);

        // Only display time taken if timer enabled
        if(!timerDisabled)
        {
            timerText.text = "Time taken: " + timer.getTimeTaken();
        }
    }

    public void playerLost()
    {
        pauseGame();

        am.stopAll(new string[] { "Level_Music_Melody", "Level_Music_Beat", "Timer_Death", "Scrapper_Fall_Death" });

        // Make sure pause panel not active
        pausePanel.SetActive(false);

        lossPanel.SetActive(true);
    }

    // Used by resume button
    public void resume()
    {
        unpauseGame();

        pausePanel.SetActive(false);

        // Lock cursor to window
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Used by restart button
    public void restart()
    {
        unpauseGame();

        // Reload scene
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadSceneAsync(currentScene.name);
    }

    // Used by settings button
    public void enterSettings()
    {
        // Enable settings UI
        pausePanel.SetActive(false);
        settingsPanel.SetActive(true);

        // Always start with camera panel open
        settings.openCameraPanel();
    }

    // Used by settings back button
    public void exitSettings()
    {
        // Enable pause UI
        pausePanel.SetActive(true);
        settingsPanel.SetActive(false);

        // Use new settings
        updateSettings();

        am.play("BackSelect");

        // Set selected button
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(settingsButton);
    }

    // Used by next level button
    public void loadNextLevel()
    {
        // Unpause
        Time.timeScale = 1;

        SceneManager.LoadSceneAsync(nextLevel);
    }

    // Used by exit button
    public void exit()
    {
        Application.Quit();
    }
}