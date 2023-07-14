using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    [SerializeField]
    private string menuMusic, level1, level2, level3, level4, level5, tutorial;

    private AudioManager am;

    private const int TUTORIAL_INDEX = 3, LEVEL1_INDEX = 4, LEVEL2_INDEX = 5, LEVEL3_INDEX = 6, LEVEL4_INDEX = 7, LEVEL5_INDEX = 8;
    private const string MAIN_MENU_SCENE = "Main-Menu";

    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);

        am = FindObjectOfType<AudioManager>();

        // Subscribe to sceneLoaded event
        SceneManager.sceneLoaded += reload;

        // If starting in menu, play menu music
        if(SceneManager.GetActiveScene().name == MAIN_MENU_SCENE)
        {
            am.play(menuMusic);
        }
    }

    public void win()
    {
        //am.fadeIn(levelMelody, 0.1f, 1f);
    }

    public void levelStart()
    {
       // am.fadeIn(levelMelody, 1f, 1f);
    }

    // When scene changes, prepare to get new audio manager and reload music
    // Can't do this now because audio manager in scene has not yet loaded
    private void reload(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(startMusic());
    }

    private IEnumerator startMusic()
    {
        // Wait for everything to load
        yield return new WaitForEndOfFrame();

        am = FindObjectOfType<AudioManager>();

        switch(SceneManager.GetActiveScene().buildIndex)
        {
            case TUTORIAL_INDEX:
                am.play(tutorial);
                break;

            case LEVEL1_INDEX:
                am.play(level1);
                break;

            case LEVEL2_INDEX:
                am.play(level2);
                break;

            case LEVEL3_INDEX:
                am.play(level3);
                break;

            case LEVEL4_INDEX:
                am.play(level4);
                break;

            case LEVEL5_INDEX:
                am.play(level5);
                break;

            default:
                am.play(menuMusic);
                break;
        }
    }
}