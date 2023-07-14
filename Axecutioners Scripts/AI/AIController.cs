using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AIController : MonoBehaviour
{
	private enum AttackTypes
	{
		INVALID_ATTACK = 1,
		LOW_ATTACK,
		MID_ATTACK,
		HIGH_ATTACK,
	}

	[SerializeField, Tooltip("AI Parameters")]
	private AIParameters parameters;

    private AttackTypes currentAttack;						 // The attack that the AI currently wants to hit
	private readonly List<AttackTypes> attackList = new();   // List of attacks that AI pulls from to choose its current attack

	private float spacingModifier;      // Modifies the distance between the AI and the opponent to alter how good the AI's spacing is

	private bool canReact = false,              // Controls if the AI is allowed to react to an attack
				 reactionInterrupted = false,   // Used to cancel reactions when something reactable gets interrupted
				 advancing = true;				// Controls if the AI is moving towards or away from the opponent

	private PlayerScript controller,	// The player under control of the AI
						 opponent;      // The player under control of the opponent

    private Transform selfDistancePoint, opponentDistancePoint;	// Custom spacing points

    private const int NO_ACTION = 0,			// PlayerScript does nothing when attackPower or dashing = 0
					  START_DASH = 1,			// PlayerScript treats a 1 as start dashing
					  DISTANCE_POINT_INDEX = 0;	// Index of distance points in scene

	private const float LIGHT_ATTACK_RANGE = 1.83f,	// Range of light attack
						MID_ATTACK_RANGE = 1.65f,	// Range of mid attack
						HEAVY_ATTACK_RANGE = 1.2f,	// Range of heavy attack
						GUARD_BREAK_RANGE = .55f;	// Range of guard break

	// Range of all attacks, such that their index matches their AttackType enum value, with a leading 0 for padding
	private readonly float[] ATTACK_RANGES = new float[] { 0, GUARD_BREAK_RANGE, LIGHT_ATTACK_RANGE, MID_ATTACK_RANGE, HEAVY_ATTACK_RANGE };

	private void Start()
	{
		// Get self and opponent
		PlayerScript[] players = FindObjectsOfType<PlayerScript>();

        if (players[0].gameObject == gameObject)
        {
			controller = players[0];
			opponent = players[1];

            //players[1].gameObject.GetComponent<PlayerInput>().defaultActionMap = "Player3";
            //Debug.Log(players[1].gameObject.GetComponent<PlayerInput>().defaultActionMap);
        }

        else
        {
			controller = players[1];
			opponent = players[0];

            //players[0].gameObject.GetComponent<PlayerInput>().defaultActionMap = "Player3";
            //Debug.Log(players[0].gameObject.GetComponent<PlayerInput>().defaultActionMap);
        }

        // Get distance points
        selfDistancePoint = transform.GetChild(DISTANCE_POINT_INDEX);
		opponentDistancePoint = opponent.transform.GetChild(DISTANCE_POINT_INDEX);

		// Initialize parameters
		initialize();

		// Subscribe to events
		PlayerScript.attackStarted += reactToAttack;
		PlayerScript.someoneHit += someoneHit;
	}

	// Unsubscribe to events when destroyed
	private void OnDestroy()
	{
		PlayerScript.attackStarted -= reactToAttack;
		PlayerScript.someoneHit -= someoneHit;
	}

	// FixedUpdate instead of Update so that we stay in step with PlayerController
	private void FixedUpdate()
	{
        // Make sure we do anything unless we want to
        controller.AISetAttacking(NO_ACTION);
        controller.AISetDashing(NO_ACTION);
        controller.AIMovement(Vector2.zero);

        // Do nothing if we can't act or if opponent dead
        if (!isActionable() || opponent.HP <= 0) { return; }

		// Cache this now
		float distanceToOpponent = Vector3.Distance(selfDistancePoint.position, opponentDistancePoint.position) * spacingModifier;

		// If we have something to react to and we haven't been interrupted, react
		if(canReact && !reactionInterrupted)
		{
			// Determine if we should react correctly
			if (parameters.reactionAccuracy >= Random.Range(0f, 1f))
			{
				// Randomly determine if we should counterattack or dash back
				if (parameters.reactionDashChance <= Random.Range(0f, 1f))
				{
					// Determine counter
					currentAttack = getAttackCounter(opponent.currentState);

					// Attack
					controller.AISetAttacking((int)currentAttack);
				}

				else
				{
                    // Dash back
                    controller.AIMovement(-createMovementVector());
                }
			}

			// Reset and return
			resetParameters();
			canReact = reactionInterrupted = false;
			return;
		}

		// Need to reset both of these in case only one was true
		canReact = reactionInterrupted = false;

		// Move towards opponent
		if (advancing)
		{
			// If we are close enough to opponent to attack, decide if we go for it or back off
			if(distanceToOpponent < ATTACK_RANGES[(int)currentAttack])
			{
				// Determine if we attack
				if(parameters.attackChance >= Random.Range(0f, 1f))
				{
                    // Attack
                    controller.AISetAttacking((int)currentAttack);

					// Reset
					resetParameters();
				}

				// Stop advancing and set start point
				else
				{
					advancing = false;

                    // Decide if we dash or walk backwards
                    if (parameters.dashBackChance >= Random.Range(0f, 1f))
                    {
                        // Need to set movement in right direction
                        controller.AIMovement(-createMovementVector());
                        controller.AISetDashing(START_DASH);
                    }
				}
			}

			// Move towards opponent
			else
			{
				controller.AIMovement(createMovementVector());
			}
		}

		else
		{
			// Move away from opponent
			controller.AIMovement(-createMovementVector());

			// Stop retreating when we're far enough away from opponent
			if (distanceToOpponent > parameters.stopRetreatingDistance)
			{
                advancing = true;

                // Decide if we dash or walk forwards
                if (parameters.dashForwardChance >= Random.Range(0f, 1f))
				{
                    // Need to set movement in right direction
                    controller.AIMovement(createMovementVector());
                    controller.AISetDashing(START_DASH);
				}
			}
		}
    }

	// Initialize AI with given parameters
    private void initialize()
    {
		// Make sure there's nothing in the attack list
		attackList.Clear();

        // Populate attack list
        for (int i = 0; i < parameters.lowAttackChance; i++)
        {
            attackList.Add(AttackTypes.LOW_ATTACK);
        }

        for (int i = 0; i < parameters.midAttackChance; i++)
        {
            attackList.Add(AttackTypes.MID_ATTACK);
        }

        for (int i = 0; i < parameters.highAttackChance; i++)
        {
            attackList.Add(AttackTypes.HIGH_ATTACK);
        }

        // Log error if AI list is empty
        if (attackList.Count == 0) { Debug.LogError("Error: AI has no attacks in its list!"); }

        // Set initial parameters
        resetParameters();
    }

    // Set parameters that decide what the AI does
    private void resetParameters()
	{
		spacingModifier = Random.Range(1 - parameters.spacingVariance, 1 + parameters.spacingVariance);
		currentAttack = attackList[Random.Range(0, attackList.Count)];
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

			// Need a default or we get an error
			default:
				Debug.LogWarning("Tried to get attack counter for something that wasn't an attack: " + attack);
				return AttackTypes.INVALID_ATTACK;
		}
	}

	// Wait for random amount of time before reacting
	private IEnumerator reactToAttack(GameObject attacker, float extraDelay)
	{
		// Ignore our own attacks
		if (attacker == gameObject) { yield break; }

		yield return new WaitForSeconds(extraDelay + parameters.baseReactionTime + Random.Range(-parameters.reactionTimeVariance, parameters.reactionTimeVariance));

		canReact = true;
	}

	// Actions taken when someone gets hit
	private void someoneHit(GameObject caller)
	{
        // If we hit the opponent, stop attacking
        if (caller != gameObject)
		{
			controller.AISetAttacking(NO_ACTION);
		}

        // Ignore any earlier reactions
        reactionInterrupted = true;

        // Reset parameters
        resetParameters();
    }

	// When we touch a wall, we can no longer move backwards, so start advancing
	private void OnCollisionStay(Collision collision)
	{
		if (collision.gameObject.CompareTag("Wall"))
		{
			advancing = true;
		}
	}

	// Simple check to see if AI can even act
	private bool isActionable()
	{
		return controller.currentState == PlayerState.IDLE || controller.currentState == PlayerState.MOVEMENT;
	}

	// Set new parameters and re-initialize with them
	public void setParameters(AIParameters newParameters)
	{
		parameters = newParameters;

		initialize();
	}

	// Randomize all parameters
	public void randomizeParameters()
    {
		// Make sure parameters exist
		parameters ??= new AIParameters();

		// Randomize parameters
		parameters.spacingVariance = Random.Range(0f, 1f);

		parameters.lowAttackChance = Random.Range(0, 10);
		parameters.midAttackChance = Random.Range(0, 10);
		parameters.highAttackChance = Random.Range(0, 10);

		parameters.baseReactionTime = Random.Range(0f, 1f);
		parameters.reactionTimeVariance = Random.Range(0f, .5f);
		parameters.reactionAccuracy = Random.Range(0f, 1f);

		parameters.attackChance = Random.Range(0f, 1f);
		parameters.stopRetreatingDistance = Random.Range(1.25f, 3);

		parameters.dashForwardChance = Random.Range(0f, 1f);
		parameters.dashBackChance = Random.Range(0f, 1f);

		// Load new parameters
		initialize();
    }

	// Create vector where x always points towards opponent
	private Vector2 createMovementVector()
	{
		return new Vector2((opponent.transform.position.x > transform.position.x) ? -1 : 1, 0);
	}
}