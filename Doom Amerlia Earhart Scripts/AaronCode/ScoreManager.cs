using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using System;

public class ScoreManager : MonoBehaviour
{
    private TMP_Text scoreText;

    private const int SMILING_OPEN = 0x1F600, SMILING_CLOSED = 0x1F601, CRYING_LAUGHING = 0x1F602, SMILING_SWEATING = 0x1F605;
    private const int SCREAMING = 0x1F606, WINKING_TONGUE = 0x1F609, BLUSHING = 0x1F60A, CLOSED_TONGUE = 0x1F60B, HEART_EYES = 0x1F60D, SUNGLASSES = 0x1F60E;

    private readonly int[] EMOJI_LIST = new int[10] { SMILING_OPEN, SMILING_CLOSED, CRYING_LAUGHING, SMILING_SWEATING, SCREAMING, WINKING_TONGUE, BLUSHING, CLOSED_TONGUE, HEART_EYES, SUNGLASSES };

    private int currentScore = 0;

    private void Start()
    {
        scoreText = GetComponent<TMP_Text>();

        scoreText.text = char.ConvertFromUtf32(SMILING_OPEN);
    }

    public void increaseScore(int score)
    {
        currentScore += score;
        scoreText.text = string.Empty;

        // For each digit in the score, convert the digit from char to int, then get the emoji at that int's index and add it to the score text
        foreach(char c in currentScore.ToString().ToCharArray())
        {
            scoreText.text += char.ConvertFromUtf32(EMOJI_LIST[int.Parse(c.ToString())]);
        }
    }
}