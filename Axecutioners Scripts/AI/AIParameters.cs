using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Holds parameters for AI
[System.Serializable]
public class AIParameters
{
    [Range(0, 1), Header("Spacing"), Tooltip("Determines how much variance the AI will have in its spacing quality. Lower = better spacing.")]
    public float spacingVariance;

    [Header("Preferred attacks"), Tooltip("Determines how likely the AI is to use the low attack. Higher = more likely.")]
    public int lowAttackChance;

    [Tooltip("Determines how likely the AI is to use the mid attack. Higher = more likely.")]
    public int midAttackChance;

    [Tooltip("Determines how likely the AI is to use the low attack. Higher = more likely.")]
    public int highAttackChance;

    [Header("Reaction controls"), Tooltip("Base time for the AI to react to the player's actions.")]
    public float baseReactionTime;

    [Tooltip("Determines how much variance the AI will have in its reaction time. Lower = more consistent.")]
    public float reactionTimeVariance;

    [Range(0, 1), Tooltip("Determines how likely the AI is to correctly react to the player�s actions. Higher = more accurate.")]
    public float reactionAccuracy;

    [Range(0, 1), Header("Strategy"), Tooltip("Determines how frequently the AI attacks when it gets in range of the player. Higher = more likely.")]
    public float attackChance;

    [Range(0, 1), Tooltip("Determines if the AI prefers to dash away from incoming attacks or counter them. Higher = prefers dash.")]
    public float reactionDashChance;

    [Tooltip("How far does the AI retreat from the player before it starts to advance again?")]
    public float stopRetreatingDistance;

    [Range(0, 1), Header("Dash controls"), Tooltip("Determines how likely the AI is to dash forwards. Higher = more likely")]
    public float dashForwardChance;

    [Range(0, 1), Tooltip("Determines how likely the AI is to dash backwards. Higher = more likely")]
    public float dashBackChance;
}