using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class AIManager : MonoBehaviour
{
    [SerializeField]
    private GameObject roundsControls, AIControls, easyButton, roundsButton;

    [SerializeField]
    public AIParameters easyAI, mediumAI, hardAI;

    private bool createImpossibleAI = false;
    private AIParameters chosenDifficulty;

    private const string PLAY_SCENE = "PlayScene", DUNGEON_SCENE = "DungeonPlayScene", THRONE_SCENE = "ThroneRoomPlayScene";

    private void Start()
    {
        // Need to persist between scenes
        DontDestroyOnLoad(this);

        // Subscribe to event
        SceneManager.sceneLoaded += loadAI;

        // If in the AI scene, make sure the rounds controls are loaded first
        if(roundsControls)
        {
            enterRoundsControls(roundsButton);
        }
    }

    // Unsubscribe to event on deletion
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= loadAI;
    }

    // When scene loads, load AI parameters
    private void loadAI(Scene scene, LoadSceneMode mode)
    {
        // If not in AI scene, destroy self
        if (scene.name != PLAY_SCENE && scene.name != DUNGEON_SCENE && scene.name != THRONE_SCENE)
        {
            Destroy(gameObject);
            return;
        }

        // Find player 2, make them an AI, and set their parameters
        PlayerScript[] players = FindObjectsOfType<PlayerScript>();

        foreach(PlayerScript p in players)
        {
            if(p.playerID != 2) { continue; }

            // Impossible AI uses a separate script, so if we want it to be impossible, add that script
            if(createImpossibleAI)
            {
                p.gameObject.AddComponent<ImpossibleAI>();
            }

            // Otherwise, just use the normal AI script
            else
            {
                p.gameObject.AddComponent<AIController>();
                p.GetComponent<AIController>().setParameters(chosenDifficulty);
            }

            return;
        }
    }

    // Set parameters to load
    public void loadSceneWithAI(string difficulty)
    {
        // Select appropriate difficulty
        // Get first letter, make sure it's lowercase, and use that for switch statement
        switch (difficulty.ToLower().ToCharArray()[0])
        {
            case 'e':
                chosenDifficulty = easyAI;
                break;

            case 'm':
                chosenDifficulty = mediumAI;
                break;

            case 'h':
                chosenDifficulty = hardAI;
                break;

            case 'i':
                createImpossibleAI = true;
                break;

            default:
                Debug.LogError("Unknown difficulty chosen for AI!");
                break;
        }

        // Load random scene
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

    // Turn on rounds controls and set correct button
    public void enterRoundsControls(GameObject button)
    {
        roundsControls.SetActive(true);
        AIControls.SetActive(false);

        EventSystem.current.SetSelectedGameObject(button);
    }

    // Turn on AI controls and set correct button
    public void enterAIControls()
    {
        roundsControls.SetActive(false);
        AIControls.SetActive(true);

        EventSystem.current.SetSelectedGameObject(easyButton);
    }
}