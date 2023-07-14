using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class Timer : MonoBehaviour
{
    [SerializeField]
    private TMP_Text timer;

    [SerializeField]
    private float startTimeInSeconds, lowTimeWarning;

    private float currentTime;
    private bool playingLowSound = false, playingDeathSound = false;

    private GameManager gm;
    private AudioManager am;

    private const int MINUTES_WIDTH = 1, SECONDS_WIDTH = 2, MS_WIDTH = 3, SECONDS_IN_MINUTE = 60, MS_IN_SECOND = 1000;

    private void Start()
    {
        // Start at max time
        currentTime = startTimeInSeconds;

        timer.text = getTimeText();
        
        gm = FindObjectOfType<GameManager>();
        am = FindObjectOfType<AudioManager>();
    }

    private void Update()
    {
        // Disable tmer text when game paused
        // TODO: put this in GameManager (doesn't work there for some reason)
        timer.gameObject.SetActive(!gm.isPaused);

        // Reduce time
        currentTime -= Time.deltaTime;
        timer.text = getTimeText();

        // Play warning sound when time low
        if (currentTime <= lowTimeWarning && currentTime > 0 && !playingLowSound)
        {
            am.play("Timer_Low");

            playingLowSound = true;
        }

        // Stop all time when time runs out
        else if (currentTime < 0f && playingLowSound)
        {
            am.stopAll(new string[] { "Timer_Death" });
            playingLowSound = false;
            Debug.Log(playingDeathSound);
        }
        if (!playingDeathSound && currentTime < 0f)
        {
            am.play("Timer_Death");
            playingDeathSound = true;

        }

        // Lose when time runs out
        if (currentTime < 0)
        {
            timer.text = "Out of time!";
            Debug.Log(playingDeathSound);
            gm.playerLost();
            
        }
    }

    private string getTimeText()
    {
        // Convert current time to string by checking(time / 60) : (time % 60), padded for leading zeroes
        return ((int)currentTime / SECONDS_IN_MINUTE).ToString().PadLeft(MINUTES_WIDTH, '0') + ':' + ((int)currentTime % SECONDS_IN_MINUTE).ToString().PadLeft(SECONDS_WIDTH, '0');
    }

    public string getTimeTaken()
    {
        // Calculate time taken
        float timeTaken = startTimeInSeconds - currentTime;

        // Minutes is time / 60, seconds is time % 60, ms is decimal of time taken, truncated, all padded for leading zeroes
        string output = ((int)timeTaken / SECONDS_IN_MINUTE).ToString().PadLeft(MINUTES_WIDTH, '0') + "m " +
                        ((int)timeTaken % SECONDS_IN_MINUTE).ToString().PadLeft(SECONDS_WIDTH, '0') + "s " +
                        ((int)((timeTaken - (int)timeTaken) * MS_IN_SECOND)).ToString().PadLeft(MS_WIDTH, '0') + "ms";

        return output;
    }
}