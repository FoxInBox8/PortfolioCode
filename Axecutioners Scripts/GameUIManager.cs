using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Reflection;

public class GameUIManager : MonoBehaviour
{
	// External objects
	public PlayerScript[] players;			// The two players' controllers
	public GameObject[] healthElements;		// The health bars for the players
	public GameObject[] pointElements;		// The point tracking icons
	public GameObject roundTimer;			// The timer at the top of the screen; counts down as game is played
	public GameObject countdownTimer;		// The timer that counts down before the round starts
	public GameObject roundIndicator;		// The text that displays what round it is before the round starts
	public GameObject pauseMenu;            // The popup menu that allows for input switching and pauses the game
	public FMODMusicManager fmodMusicManager; //FMOD Music Manager
	public FMODAudioManager fmodAudioManager;
	public GameObject necromancer;

	// Timing variables
	public float roundRemainingTime;		// The amonut of time before the round ends
	private float countdownRemainingTime;   // The amount of time the countdown has left

	// Other variables
	public bool isPaused;                   // Whether the game is paused

	//Networking variables
	public bool networkPause = false;
	public bool networkActive = false;

	//audio variables
	bool threePlayed;
	bool twoPlayed;
	bool onePlayed;
	bool fightPlayed;

	void Start()
	{
		fmodMusicManager = FMODMusicManager.instance;

		threePlayed = false;
		twoPlayed = false;
		onePlayed = false;
		fightPlayed = false;

		// Ensure the timer variables are set correctly and that the pause menu is off
		ResetTimers();
		PauseGame(false);
		// Ensure there is no player input and update the UI
		isPaused = true;
		UpdateTimer();
	}

	void FixedUpdate()
	{
		//timeScale does some weird stuff with networking so for now to pause over networks we need to use a bool
		if (networkPause == false)
		{
            // While the countdown is active
            if (countdownRemainingTime > -1)
            {
                // Update the countdown
                UpdateCountdown();
            }
            // During the actual game
            else
            {
                // Count down the timer
                UpdateTimer();
            }

            // Always update the health and point UI per player
            foreach (PlayerScript input in players)
            {
                if (input != null)
                {
                    UpdateHealthBar(input);
                    UpdatePoints(input);

					//update network UI with commands
					//if (input.GetComponent<PlayerScript>().networkActive)
					//{
     //                   input.GetComponent<NetworkPlayerController>().UpdatePoints(RoundManager.points[input.playerID - 1]);
     //               }
                }
            }
        }
	}

	// Returns the timing variables to their defaul values
	public void ResetTimers()
	{
		roundRemainingTime = RoundManager.roundLength;
		countdownRemainingTime = 4;
	}

	// Pauses the game (timescale = 0) and brings up the pause menu
	public void PauseGame(bool pause)
	{
		isPaused = pause;
		pauseMenu.SetActive(pause);
		Time.timeScale = pause ? 0 : 1;
	}
	//mirror does not like messing with timescale
	public void NetworkPause(bool pause)
    {
		isPaused = pause;
		pauseMenu.SetActive(pause);
	}

	// Toggles whether the game is paused; called by the input system
	public void TogglePause()
	{
		if (networkActive)
			NetworkPause(!isPaused);
		else
			PauseGame(!isPaused);
	}

	private void UpdateCountdown()
	{
		// Decrease the timer, includes decimal places
		countdownRemainingTime -= Time.fixedDeltaTime;
		// Create a floored int version for displaying to the screen and tracking display progress
		int cdRTInt = (int)Mathf.Floor(countdownRemainingTime);

		// While the timer is counting down
		if (cdRTInt > 0)
		{
			// Display the actual number (3...2...1...)
			countdownTimer.GetComponent<TMP_Text>().text = cdRTInt.ToString();
			// And display the round number retrieved from the RoundManager
			roundIndicator.GetComponent<TMP_Text>().text = "Round " + RoundManager.currentRound.ToString();

			//Play sounds
			if(cdRTInt == 3 && !threePlayed)
            {
				fmodAudioManager.PlayFMODOneShot("FightIntro3", Vector3.zero);
				threePlayed = true;
            }
			else if(cdRTInt == 2 && !twoPlayed)
            {
				fmodAudioManager.PlayFMODOneShot("FightIntro2", Vector3.zero);
				twoPlayed = true;
			}
			else if(cdRTInt == 1 && !onePlayed)
            {
				fmodAudioManager.PlayFMODOneShot("FightIntro1", Vector3.zero);
				onePlayed = true;
			}
		}
		// When the timer reaches 0
		else if (cdRTInt == 0)
		{
			//play sound
			if(!fightPlayed)
            {
				fmodAudioManager.PlayFMODOneShot("FightIntroFight", Vector3.zero);
				fightPlayed = true;
			}

			// Display "Fight!" prompt (input still paused)
			countdownTimer.GetComponent<TMP_Text>().text = "FIGHT!";
			// Turn off the round indicator
			roundIndicator.GetComponent<TMP_Text>().text = "";
		}
		// After one second, turn off the "Fight!" propmt (input will be useable now)
		else
		{
			countdownTimer.GetComponent<TMP_Text>().text = "";
			isPaused = false;
			necromancer.GetComponent<NecromancerScript>().CorrectRotation();

			if (networkActive)
				LobbyController.Instance.StartPlayers();
        }
	}

	private void UpdateTimer()
	{
		// If the game isn't paused
		if (!isPaused && players[0].HP > 0 && players[1].HP > 0)
		{
			// Decrease the timer, includes decimal places
			roundRemainingTime -= Time.fixedDeltaTime;
		}
		// Create a floored int version for displaying to the screen and tracking display progress
		int rRTInt = (int)Mathf.Floor(roundRemainingTime);

		//Music Variable
		fmodMusicManager.SecondsRemaining = rRTInt;


		// Update the timer visual
		roundTimer.GetComponent<TMP_Text>().text = rRTInt.ToString();
	}

	private void UpdateHealthBar(PlayerScript input)
	{
        // Loop through the health elements and activate/deactivate as necessary
        for (int i = 1; i < 4; ++i)
        {
            healthElements[input.playerID - 1].transform.GetChild(i).gameObject.SetActive(input.HP >= i);
        }
    }

	private void UpdatePoints(PlayerScript input)
	{
		// As the player ids are either 1 or 2, we have to subtract 1 to ensure we get either 0 or 1 for the index of the array
		int index = input.playerID - 1;
		if (RoundManager.points[index] < 4)
		{
			// Loop through the point indicators and activate/deactivate as necessary
			for (int i = 0; i < 3; ++i)
			{
				pointElements[index].transform.GetChild(i).gameObject.SetActive(RoundManager.points[index] >= i + 1);
			}
			// Deactivate the point counter number
			pointElements[index].transform.GetChild(3).gameObject.SetActive(false);
		}
		else
		{
			// Activate only the first point indicator
			pointElements[index].transform.GetChild(0).gameObject.SetActive(true);
			pointElements[index].transform.GetChild(1).gameObject.SetActive(false);
			pointElements[index].transform.GetChild(2).gameObject.SetActive(false);
			// Activate the point counter number
			pointElements[index].transform.GetChild(3).gameObject.SetActive(true);
			// The display for the point counter will be either 4x or x4 depending on the player, so figure that out here
			string pointCount = input.playerID == 1 ? "x" + RoundManager.points[index].ToString() : RoundManager.points[index].ToString() + "x";
			pointElements[index].transform.GetChild(3).gameObject.GetComponent<TMP_Text>().text = pointCount;
		}
	}
}