using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DummyState
{
	LOW,
	MID,
	HIGH,
	DEAD
}

public class DummyController : MonoBehaviour
{
	// ---------------------- Constants ----------------------
	private float gravity = -0.1f;								// Gravity, used to keep the player on the ground
	private Vector3 maxSpeed = new Vector3(1.5f, 5f, 0f);		// The maximum speed the player can move

	// ---------------------- External objects ----------------------
	// Managers
	public Animator animator;                                   // The animator for the player
    public FMODAudioManager fmodAudioManager;                   // For playing sounds
    // Particles
    public GameObject hitParticles;                             // Particles to create when the player is hit
	public ParticleSystem lowBlood, midBlood;
	public GameObject blockParticles;							// Particles to create when the player achieves full block
	// Other
	public GameObject target;									// The object the player should be facing
	public Rigidbody playerRB;									// Rigidbody of the player, used for setting velocity

	// ---------------------- Player State & Input Variables ----------------------
	public PlayerState currentState;							// The current state of the player, used by the state machine
	public DummyState dummyState;								// The state the dummy is in
	private Vector3 velocity;									// A vector to hold the velocity of the player because the rigidbody velocity is weird
	private bool grounded;										// If the player is on the ground
	private AnimatorClipInfo[] animatorinfo;					// Stores information about animation clips to ensure that one time animations play properly - Griffin
	private int targetDirection;								// The direction the hitboxes should be created

	// ---------------------- Miscellaneous Timers ----------------------
	private int hitStunTimer;									// The amount of time the player stays in the hitstun state after being hit
	
	// ---------------------- Debug ----------------------
	private bool logDebugMessages = false;						// Turn on to display state change messages



	// ---------------------- Functions ----------------------
	// Called once at the start
	void Start()
	{
		// Start the player off idle
		EnterState(PlayerState.IDLE);
		// Reflect about the x axis
		this.gameObject.transform.localScale = new Vector3(1f, 1f, -1f);
		// Set the dummy state to the appropriate value
		dummyState = DummyState.LOW;
	}

	// Called every frame
	void FixedUpdate()
	{
		// Update the target position to face the other player
		targetDirection = (target.transform.position.x > this.transform.position.x) ? 1 : -1;
		// Added to try and correct errors in animation rotation - Griffin
		// Player 1 faces -45 and player 2 45
		this.gameObject.transform.eulerAngles = new Vector3(0, 200f * 2 - 300f, 0);


		// Run the game
		// Update the player grounded state
		checkGrounded();
		// Run the state machine
		ChangeState();
		UpdateState();
	}

	// Perform the logic for the state. Called every frame
	void UpdateState()
	{
		switch (currentState)
		{
			case PlayerState.IDLE:
				// Velocity is set to 0 upon entering the state
				// Apply gravity (just in case)
				velocity.y = calculateGravity(velocity.y);
				setRBVelocity(velocity);
				break;
			
			case PlayerState.HITSTUN:
				hitStunTimer--;
				break;
			
			default:
				break;
		}
	}

	// Determine when the state should change. Called every frame
	private void ChangeState()
	{
		switch (currentState)
		{
			case PlayerState.HITSTUN:
				if (hitStunTimer <= 0)
				{
					EnterState(PlayerState.IDLE);
				}
				break;
			
			default:
				break;
		}
	}

	// Called upon entering a state
	private void EnterState(PlayerState newState)
	{
		if (newState != currentState)
		{
			ExitState();
			currentState = newState;
			debugMessage("Enter State");
			switch (newState)
			{
				case PlayerState.IDLE:
					velocity = Vector3.zero;
					break;
					
				case PlayerState.HITSTUN:
					hitStunTimer = 60;
					break;

				default:
					break;
			}

			UpdateAnimation();
		}
	}

	// Called upon exiting a state
	private void ExitState()
	{
		debugMessage("Exit State");
		switch (currentState)
		{
			case PlayerState.HITSTUN:
				if (dummyState != DummyState.DEAD)
					dummyState++;
				TutorialUIManager.advanceStep();
				break;
			
			default:
				break;
		}
	}
	
	private void UpdateAnimation()
	{
		switch (currentState)
		{
			case PlayerState.IDLE:
				animator.Play("Base Layer.Idle", 0, 0);
				animator.SetInteger("animID", 0);
				break;
			
			case PlayerState.HITSTUN:
				break;
			
			default:
				break;
		}
	}

	// Check if the player is on the ground. Called every frame
	private void checkGrounded()
	{
		RaycastHit hit;
		// Raycast directly below the player
		if (Physics.Raycast(playerRB.transform.position, Vector3.down, out hit, 0.05f))
		{
			// Check if the ground is there and update grounded appropriately
			grounded = hit.transform.gameObject.CompareTag("Ground");
		}
		else
		{
			grounded = false;
		}
	}

	// Calculate the given change in y veocity based on gravity
	private float calculateGravity(float y)
	{
		// Apply gravity to the player and stop if they've hit the ground
		if (!grounded)
		{
			if (y > -maxSpeed.y)
			{
				y += gravity;
			}
		}
		else
		{
			y = Mathf.Max(0f, y);
		}
		return y;
	}
	
	// Clamp each element of a vec3 piecewise with another vec3
	private Vector3 clampVector3(Vector3 a, Vector3 b)
	{
		Vector3 c = a;
		c.x = Mathf.Clamp(c.x, -Mathf.Abs(b.x), Mathf.Abs(b.x));
		c.y = Mathf.Clamp(c.y, -Mathf.Abs(b.y), Mathf.Abs(b.y));
		c.z = Mathf.Clamp(c.z, -Mathf.Abs(b.z), Mathf.Abs(b.z));
		return c;
	}

	// Set the velocity of the rigidbody without exceeding the max speed
	private void setRBVelocity(Vector3 v)
	{
		// Cap the speed of the RB by the max speed, unless the player is dashing, the cap at dash speed
		playerRB.velocity = clampVector3(v, maxSpeed);
	}

	// Called when the player collides with a hitbox and should take damage
	private void hitPlayer(GameObject hitbox, bool useLowBlood = false)
	{
        fmodAudioManager.PlayFMODOneShot("PlayerHit", transform.position);

        // Spawn the blood particles
        if (useLowBlood)
		{
			lowBlood.Play();
		}

		else
		{
			midBlood.Play();
		}

		// Play the correct animation based on what attack it was
		switch(hitbox.GetComponent<HitBoxScript>().damage)
		{
			case 1:
				animator.Play("Base Layer.Death_Low", 0, 0f);
				break;
			case 2:
				animator.Play("Base Layer.Death_Mid", 0, 0f);
				break;
			case 3:
				animator.Play("Base Layer.Death_High", 0, 0f);
				break;
		}

		EnterState(PlayerState.HITSTUN);
	}

	// Checks if the player collides with a higbox
	public void OnTriggerEnter(Collider trigger)
	{
		if (TutorialUIManager.step >= 3)
		{
			// If the player collides with a hitbox
			if (trigger.gameObject.CompareTag("hitbox"))
			{
				switch (dummyState)
				{
					case DummyState.LOW:
						if (trigger.GetComponent<HitBoxScript>().damage == 1)
							hitPlayer(trigger.gameObject, true);
						break;
					case DummyState.MID:
						if (trigger.GetComponent<HitBoxScript>().damage == 2)
							hitPlayer(trigger.gameObject);
						break;
					case DummyState.HIGH:
						if (trigger.GetComponent<HitBoxScript>().damage == 3)
							hitPlayer(trigger.gameObject);
						break;
				}
			}
		}
	}

	// Prints the object and state, along with a message; for debugging purposes, turn on/off with logDebugMessages
	private void debugMessage(string message)
	{
		if (logDebugMessages)
		{
			Debug.Log(this.gameObject.name + " | " + this.currentState + ":\n" + message);
		}
	}
}