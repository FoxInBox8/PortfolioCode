using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoundManager : MonoBehaviour
{
	private static RoundManager roundManagerInstance;

	public FMODMusicManager fmodMusicManager;		//Fmod music manager

	public static int numRounds = 1;										// Number of rounds to play
	public static int currentRound = 1;										// What round you're currently on
	public static int roundLength = 50;										// The amonut of time in seconds the round will last
	public static InputScheme[] playerInputSchemes = new InputScheme[2];	// Keeps track of what input scheme the players are using between rounds
	public static int[] points = {0,0};										// How many points players 1 and 2 respectively have earned

	void Awake()
	{
		fmodMusicManager = FMODMusicManager.instance;

		// Ensure there is only one instance of this manager
		DontDestroyOnLoad(this);
		if (roundManagerInstance == null)
		{
			roundManagerInstance = this;
		}
		else
		{
			Object.Destroy(gameObject);
		}
	}

    private void Update()
    {
		if (fmodMusicManager)
		{
            //Update FMOD Manager music intensity
            if (SceneManager.GetActiveScene() != SceneManager.GetSceneByName("PlayScene") &&
                SceneManager.GetActiveScene() != SceneManager.GetSceneByName("DungeonPlayScene") &&
                SceneManager.GetActiveScene() != SceneManager.GetSceneByName("ThroneRoomPlayScene") &&
                SceneManager.GetActiveScene() != SceneManager.GetSceneByName("AIScene"))
            {
                fmodMusicManager.Intensity = 0;
            }
            else if (currentRound == 1)
            {
                fmodMusicManager.Intensity = 1;
            }
            else if (points[0] == numRounds - 1 || points[1] == numRounds - 1)
            {
                fmodMusicManager.Intensity = 3;
            }
            else
            {
                fmodMusicManager.Intensity = 2;
            }
        }
	}
}