using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIRandomizer : MonoBehaviour
{
    private void Start()
    {
        // Get all AIs and randomize their parameters
        AIController[] AIs = FindObjectsOfType<AIController>();

        foreach(AIController ai in AIs)
        {
            ai.randomizeParameters();
        }

        // Set max rounds absurdly high so that fighting continues effectively indefinitely
        RoundManager.numRounds = int.MaxValue;
    }
}