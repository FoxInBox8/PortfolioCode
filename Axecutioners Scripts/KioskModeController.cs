using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KioskModeController : MonoBehaviour
{
    [SerializeField]
    private int kioskModeTimeSeconds, kioskModeRounds;

    private float idleCounter = 0;
    private bool countTimer = true;

    private const string PLAY_SCENE = "PlayScene", DUNGEON_SCENE = "DungeonPlayScene", THRONE_SCENE = "ThroneRoomPlayScene", MENU_SCENE = "MainMenu";

    private void Start()
    {
        // Make sure game starts running
        Time.timeScale = 1;

        // Need to persist between scenes
        DontDestroyOnLoad(this);

        // Subscribe to event
        SceneManager.sceneLoaded += sceneChanged;
    }

    // Unsubscribe to event on deletion
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= sceneChanged;
    }

    private void Update()
    {
        // If in main menu, count time idle until we reach kiosk mode time 
        if(countTimer)
        {
            idleCounter = Input.anyKey ? 0 : idleCounter + Time.deltaTime;

            if (idleCounter >= kioskModeTimeSeconds)
            {
                // Reset timer
                idleCounter = 0;

                // Load random scene for kiosk mode
                switch (UnityEngine.Random.Range(0, 3))
                {
                    case 0:
                        SceneManager.LoadSceneAsync(PLAY_SCENE);
                        break;
                    case 1:
                        SceneManager.LoadSceneAsync(DUNGEON_SCENE);
                        break;
                    case 2:
                        SceneManager.LoadSceneAsync(THRONE_SCENE);
                        break;
                    default:
                        SceneManager.LoadSceneAsync(PLAY_SCENE);
                        break;
                }
            }
        }

        // If not in main menu, return to main menu when any key pressed
        else if (Input.anyKey)
        {
            SceneManager.LoadSceneAsync(MENU_SCENE);
        }
    }

    private void sceneChanged(Scene scene, LoadSceneMode mode)
    {
        // If in a play scene, turn into kiosk mode
        if (scene.name == PLAY_SCENE || scene.name == DUNGEON_SCENE || scene.name == THRONE_SCENE)
        {
            // Get players, make them AIs, and randomize them
            PlayerScript[] players = FindObjectsOfType<PlayerScript>();

            foreach(PlayerScript p in players)
            {
                // Make sure they aren't already an AI
                if(!p.GetComponent<AIController>()) { p.gameObject.AddComponent<AIController>(); }

                p.GetComponent<AIController>().randomizeParameters();
            }

            RoundManager.numRounds = kioskModeRounds;

            // Stop counting timer
            countTimer = false;
        }

        // If in any other scene, destroy self
        else
        {
            Destroy(gameObject);
        }
    }
}
