using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpossibleAI : MonoBehaviour
{
    private enum AttackTypes
    {
        INVALID_ATTACK = 1,
        LOW_ATTACK,
        MID_ATTACK,
        HIGH_ATTACK,
    }

    private bool canCounterAttack = false;
    private PlayerScript controller, opponent;

    // PlayerScript does nothing when attackPower = 0
    private const int NO_ACTION = 0;

    private void Start()
    {
        // Get self and opponent
        PlayerScript[] players = FindObjectsOfType<PlayerScript>();

        if (players[0].gameObject == gameObject)
        {
            controller = players[0];
            opponent = players[1];
        }

        else
        {
            controller = players[1];
            opponent = players[0];
        }

        // Subscribe to event
        PlayerScript.attackStarted += reactToAttack;
    }

    // Unsubscribe to event when destroyed
    private void OnDestroy()
    {
        PlayerScript.attackStarted -= reactToAttack;
    }

    // FixedUpdate instead of Update so that we stay in step with PlayerController
    private void FixedUpdate()
    {
        // Do nothing if we can't act or if opponent dead
        if (!isActionable() || opponent.HP <= 0)
        {
            controller.AIMovement(Vector2.zero);
            controller.AISetAttacking(NO_ACTION);

            return;
        }

        // If we can counterattack, do so
        if(canCounterAttack)
        {
            controller.AISetAttacking((int)getAttackCounter(opponent.currentState));

            canCounterAttack = false;
        }

        // Otherwise just walk towards opponent
        else
        {
            controller.AISetAttacking(NO_ACTION);
            controller.AIMovement(createMovementVector());
        }
    }

    // Determine the appropriate counter to an attack
    private AttackTypes getAttackCounter(PlayerState attack)
    {
        switch (attack)
        {
            case PlayerState.ATTACK_LIGHT:
                return AttackTypes.HIGH_ATTACK;

            case PlayerState.ATTACK_MEDIUM:
                return AttackTypes.LOW_ATTACK;

            case PlayerState.ATTACK_HEAVY:
                return AttackTypes.MID_ATTACK;

            // This should never happen
            default:
                Debug.LogWarning("Tried to get attack counter for something that wasn't an attack: " + attack);
                return AttackTypes.INVALID_ATTACK;
        }
    }

    // If opponent attacks, immediately counterattack
    private IEnumerator reactToAttack(GameObject attacker, float extraDelay)
    {
        // Ignore our own attacks
        if (attacker == gameObject) { yield break; }

        canCounterAttack = true;
    }

    // Simple check to see if AI can act
    private bool isActionable()
    {
        return controller.currentState == PlayerState.IDLE || controller.currentState == PlayerState.MOVEMENT;
    }

    // Create vector where x always points towards opponent
    private Vector2 createMovementVector()
    {
        return new Vector2((opponent.transform.position.x > transform.position.x) ? -1 : 1, 0);
    }
}