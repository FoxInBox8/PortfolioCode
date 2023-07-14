using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TutorialPlayerScript : MonoBehaviour
{
    //Audio
    public FMOD.Studio.EventInstance walkSFX;                   // Walk SFX (All looping sounds need instance variables)

    // ---------------------- Constants ----------------------
    public int playerID;										// The ID of the player (usually 1 or 2)
	public int HP = 3;											// The max HP, defaults to 3
	private float moveSpeed = 1f;								// Horizontal movement speed
	private float gravity = -0.1f;								// Gravity, used to keep the player on the ground
	private Vector3 maxSpeed = new Vector3(1.5f, 5f, 0f);		// The maximum speed the player can move
	private float acceleration = 0.5f;							// Acceleration coefficient
	private float knockbackDampening = 0.99f;					// The rate at which the player slows down after being hit
	private float hurtboxHeight = 1.25f;						// Height of the player hurtbox by default
	private Vector3 hurtboxCenter = new Vector3(0f, 0.625f, 0f);// Position of the player hurtbox (relative to player) by default

	// ---------------------- External objects ----------------------
	// Managers
	public Animator animator;									// The animator for the player
	public GameObject cameraMover;                              // Object used to move the camera to keep the players in the center and for zooming
    public FMODAudioManager fmodAudioManager;                   // For playing sounds
    // Particles
    public GameObject hitParticles;								// Particles to create when the player is hit
	public GameObject blockParticles;							// Particles to create when the player achieves full block
	// Prefabs
	public GameObject hitBox;   								// The gameobject that is created when attacking
	public GameObject guardBreakBox;                            // The gameobject that is created when guard breaking
	public GameObject trail1;                                   // The gameobject that renders a trail during attack swings   - Griffin
	public GameObject trail2;                                   // The gameobject that renders a trail during attack swings   - Griffin
	public GameObject trail3;                                   // The gameobject that renders a trail during attack swings   - Griffin
	public GameObject trail4;                                   // The gameobject that renders a trail during attack swings   - Griffin
	public GameObject trail5;                                   // The gameobject that renders a trail during attack swings   - Griffin
    // Other
	public GameObject target;									// The object the player should be facing
	public Rigidbody playerRB;									// Rigidbody of the player, used for setting velocity

	// ---------------------- Attack variables ----------------------
	private float attackPower;									// Updated by input package, used to check if player is attacking
	private int attackTimer;									// Timer for attack to synchronize events
	private int targetDirection;								// The direction the hitboxes should be created
	// Timings
	private int[] lightAttackTiming = {							// Timing for when to perform each section of the light attack
		20,		// Squish player hurtbox
		45,		// Spawn attack hitbox
		60,		// Unsquish player hurtbox
		87		// Move to next state
	};
	private int[] midAttackTiming = {							// Timing for when to perform each section of the mid attack
		45,		// Spawn attack hitbox
		105		// Move to next state
	};
	private int[] heavyAttackTiming = {							// Timing for when to perform each section of the heavy attack
		30,		// Start moving up
		35,		// Stop moving up
		55,		// Spawn attack hitbox
		100,	// Start falling
		140		// Move to next state
	};
	// Values
	private AttackValues lightAttackValues = new AttackValues(	// Values for the light attack
		0,							// Type of hitbox (attack)
		new Vector2(1.8f, 0.1f),	// Hitbox offset from player
		new Vector3(1f, 1f, 1f),	// Hitbox scale
		30,							// Stun time
		1,							// Damage
		new Vector3(4f, 2f, 0f),	// Launch power
		0.1f						// Lifespan
	);
	private AttackValues midAttackValues = new AttackValues(	// Values for the mid attack
		0,							// Type of hitbox (attack)
		new Vector2(1.7f, 1.0f),	// Hitbox offset from player
		new Vector3(1f, 1f, 1f),	// Hitbox scale
		45,							// Stun time
		2,							// Damage
		new Vector3(7f, 2f, 0f),	// Launch power
		0.1f						// Lifespan
	);
	private AttackValues heavyAttackValues = new AttackValues(	// Values for the heavy attack
		0,							// Type of hitbox (attack)
		new Vector2(1.5f, 0f),		// Hitbox offset from player
		new Vector3(1f, 1f, 1f),	// Hitbox scale
		60,							// Stun time
		3,							// Damage
		new Vector3(10f, 2f, 0f),	// Launch power
		0.1f						// Lifespan
	);

	// ---------------------- Dash variables ----------------------
	private float dashing;										// Updated by input package, used to check if player is dashing
	private int dashTimer;										// Timer for attack to synchronize events
	private float dashDirection;								// The direction of the dash, only set when entering the state to ensure it does not change during the dash
	private Vector3 dashSpeed = new Vector3(3.5f, 5f, 0f);		// The speed of the dash itself
	private int[] dashTiming = {								// Timing for when to perform each section of the dash
		3,		// Start the dash movement (begin lag)
		12,		// Stop the dash movement (dash length)
		15		// Move to the next state (end lag)
	};

	// ---------------------- Player State & Input Variables ----------------------
	public PlayerState currentState;							// The current state of the player, used by the state machine
	public InputScheme inputScheme;								// The input scheme (keyboard, gamepad, etc.) the player is using
	public InputDevice inputDevice;								// The input device the player is currently using
	private Vector2 moveDirection;								// Updated by input package, vec2 holding the input from keyboard for movement. Ranges from -1 to 1 for x and y
	private Vector3 velocity;									// A vector to hold the velocity of the player because the rigidbody velocity is weird
	private bool grounded;										// If the player is on the ground
	private AnimatorClipInfo[] animatorinfo;					// Stores information about animation clips to ensure that one time animations play properly - Griffin

	// ---------------------- Miscellaneous Timers ----------------------
	private int hitStunTimer;									// The amount of time the player stays in the hitstun state after being hit
	private int sceneTransitionTimer;							// Timer to determine when to transition to game over/next round
	public bool finishedMovement = true;						// If the player has completed the movement part of the tutorial
	public bool finishedDashing = true;							// If the player has completed the dashing part of the tutorial
    private int walkSoundBufferTimer;                           // Timer to smooth the stopping of walk sfx

    // ---------------------- Debug ----------------------
    private bool logDebugMessages = false;						// Turn on to display state change messages
	


	// ---------------------- Functions ----------------------
	// Called once at the start
	void Start()
    {				
		finishedMovement = true;
	    finishedDashing = true;

        walkSoundBufferTimer = 6;

        // Start the player off idle
        EnterState(PlayerState.IDLE);
		if (playerID == 2)
		{
			// Reflect player 2 about the x axis
			this.gameObject.transform.localScale = new Vector3(-1f, 1f, 1f);
		}

		inputScheme = InputScheme.KEYBOARD;
		inputDevice = Keyboard.current;
		// Update the device they are using for the PlayerInput
		this.gameObject.transform.GetComponent<PlayerInput>().SwitchCurrentControlScheme("KeyboardMouse", Keyboard.current, Mouse.current);

        if (fmodAudioManager)
        {
            //Create FMOD Event Instance of all Looping SFX and attach to player
            walkSFX = fmodAudioManager.CreateFMODEventInstance("PlayerWalk");
            FMODUnity.RuntimeManager.AttachInstanceToGameObject(walkSFX, gameObject.transform);
        }
    }

	// Called every frame
	void FixedUpdate()
	{
		// Move the camera to keep the players in frame
		cameraMover.transform.position = new Vector3(playerRB.transform.position.x, cameraMover.transform.position.y, cameraMover.transform.position.z);
		// Update the target position to face the other player
		targetDirection = (target.transform.position.x > this.transform.position.x) ? 1 : -1;
        // Added to try and correct errors in animation rotation - Griffin
        // Player 1 faces -45 and player 2 45
        this.gameObject.transform.eulerAngles = new Vector3(0, 200f * playerID - 300f, 0);

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
			
			case PlayerState.MOVEMENT:
                walkSoundBufferTimer = 6;

                // Update the tutorial step
                if (!finishedMovement)
				{
					finishedMovement = true;
					TutorialUIManager.advanceStep();
				}
				// Apply gravity (just in case)
				velocity.y = calculateGravity(velocity.y);

				// Add a little velocity to the amount needed to move, accelerating the player
				if (Mathf.Abs(velocity.x) < maxSpeed.x)
				{
					// Added a (* -1) because I reversed the scene and had to reverse movement accordingly - Griffin
					velocity.x += acceleration * moveSpeed * moveDirection.x * -1;
				}
				// Check to make sure you're going the same direction and are still pressing a button
				if (Mathf.Sign(moveDirection.x) != -Mathf.Sign(velocity.x))
				{
					// If input has reversed, start going the other direction
					velocity.x *= -1;
				}
				setRBVelocity(velocity);
				break;
			
			case PlayerState.AERIAL:
				// Apply gravity
				velocity.y = calculateGravity(velocity.y);
				setRBVelocity(velocity);
				break;
			
			case PlayerState.HITSTUN:
				// Slowly decellerate the player over time
				velocity.y = calculateGravity(velocity.y);
				velocity.x *= knockbackDampening;
				setRBVelocity(velocity);
				// Decrease the hit stun timer
				hitStunTimer--;
				break;
			
			case PlayerState.ATTACK_LIGHT:
				// Count up timer
				attackTimer++;

				if (attackTimer == 35)
				{
					toggleTrails(true);
				}
				else if (attackTimer == 60)
				{
					toggleTrails(false);
				}

				// Squish player hurtbox
				if (attackTimer == lightAttackTiming[0])
				{
					gameObject.GetComponent<CapsuleCollider>().height = hurtboxHeight * 0.5f;
					gameObject.GetComponent<CapsuleCollider>().center = new Vector3(hurtboxCenter.x, gameObject.GetComponent<CapsuleCollider>().radius, hurtboxCenter.z);
				}

				// Spawn attack hitbox
				if (attackTimer == lightAttackTiming[1])
				{
					createAttack(lightAttackValues);
				}

				// Unsquish player hurtbox
				if (attackTimer == lightAttackTiming[2])
				{
					gameObject.GetComponent<CapsuleCollider>().height = hurtboxHeight;
					gameObject.GetComponent<CapsuleCollider>().center = hurtboxCenter;
				}
				break;
			
			case PlayerState.ATTACK_MEDIUM:
				// Count up timer
				attackTimer++;

				if (attackTimer == 35)
				{
					toggleTrails(true);
				}
				else if (attackTimer == 60)
				{
					toggleTrails(false);
				}

				// Spawn attack hitbox
				if (attackTimer == midAttackTiming[0])
				{
					createAttack(midAttackValues);
				}
				break;
			
			case PlayerState.ATTACK_HEAVY:
				// Count up timer
				attackTimer++;

				if (attackTimer == 50)
				{
					toggleTrails(true);
				}
				else if (attackTimer == 85)
				{
					toggleTrails(false);
				}

				// When the player begins jumping
				if (attackTimer == heavyAttackTiming[0])
				{
					// Set the player velocity to launch them upwards
					velocity = new Vector3(0f, 3f, 0f);
				}

				// When the player has reached the top of the jump
				if (attackTimer == heavyAttackTiming[1])
				{
					// Stop the player from accelerating and moving
					velocity = Vector3.zero;
				}

				// Actually spawn the hitbox
				if (attackTimer == heavyAttackTiming[2])
				{
					createAttack(heavyAttackValues);
				}

				// Make the player fall down again
				if (attackTimer >= heavyAttackTiming[3])
				{
					velocity.y = calculateGravity(velocity.y);
				}
				
				// Apply the calculated velocity to the player
				setRBVelocity(velocity);
				break;
			
			case PlayerState.DASH:
				if (!finishedDashing && finishedMovement)
				{
					finishedDashing = true;
					TutorialUIManager.advanceStep();
				}
				// Count up timer
				dashTimer++;

				// Move the player according to the direction of the dash
				if (dashTimer >= dashTiming[0] && dashTimer <= dashTiming[1])
				{
					velocity.y = calculateGravity(velocity.y);
					velocity.x = dashSpeed.x * dashDirection;

                    //Play dash SFX
                    fmodAudioManager.PlayFMODOneShot("PlayerDash", transform.position);
                }
				
				// Slow down the player after the dash is over
				if (dashTimer > dashTiming[1])
				{
					velocity *= 0.5f;
				}
				setRBVelocity(velocity);
				break;
		}

        //decrease walk sound buffer timer
        walkSoundBufferTimer--;

        //if buffer timer = 0, stop walk sfx
        FMOD.Studio.PLAYBACK_STATE walkstop_playbackState;
        walkSFX.getPlaybackState(out walkstop_playbackState);
        if (walkstop_playbackState.Equals(FMOD.Studio.PLAYBACK_STATE.PLAYING) && walkSoundBufferTimer <= 0)
        {
            walkSFX.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
    }

	// Determine when the state should change. Called every frame
	private void ChangeState()
	{
		switch (currentState)
		{
			case PlayerState.IDLE:
				if (moveDirection.x != 0)
				{
					EnterState(PlayerState.MOVEMENT);
				}
				if (!grounded)
				{
					EnterState(PlayerState.AERIAL);
				}
				checkAttacks();
				break;
			
			case PlayerState.MOVEMENT:
				if (moveDirection.x == 0)
				{
					EnterState(PlayerState.IDLE);
				}
				if (!grounded)
				{
					EnterState(PlayerState.AERIAL);
				}
				if (dashing > 0)
				{
					EnterState(PlayerState.DASH);
				}
				checkAttacks();
				break;
			
			case PlayerState.AERIAL:
				if (grounded)
				{
					EnterState(PlayerState.IDLE);
				}
				break;
			
			case PlayerState.HITSTUN:
				if (hitStunTimer <= 0)
				{
					if (grounded)
					{
						EnterState(PlayerState.IDLE);
					}
					else
					{
						EnterState(PlayerState.AERIAL);
					}
				}
				break;
			
			case PlayerState.ATTACK_LIGHT:
				if (attackTimer == lightAttackTiming[3])
				{
					EnterState(PlayerState.IDLE);
				}
				break;
			
			case PlayerState.ATTACK_MEDIUM:
				if (attackTimer == midAttackTiming[1])
				{
					EnterState(PlayerState.IDLE);
				}
				break;
			
			case PlayerState.ATTACK_HEAVY:
				if (attackTimer == heavyAttackTiming[4])
				{
					EnterState(PlayerState.IDLE);
				}
				break;
			
			case PlayerState.DASH:
				if (dashTimer == dashTiming[2])
				{
					EnterState(PlayerState.IDLE);
				}
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
				
				case PlayerState.MOVEMENT:
				
					break;
				
				case PlayerState.AERIAL:
                    FMOD.Studio.PLAYBACK_STATE walk_playbackState;
                    walkSFX.getPlaybackState(out walk_playbackState);
                    if (walk_playbackState.Equals(FMOD.Studio.PLAYBACK_STATE.STOPPED))
                    {
                        walkSFX.start();
                    }
                    else if (walk_playbackState.Equals(FMOD.Studio.PLAYBACK_STATE.STOPPING))
                    {
                        walkSFX.start();
                    }
                    break;
				
				case PlayerState.HITSTUN:

					// Play hitstun animation if not dead
					if(HP > 0)
					{
						animator.Play("Base Layer.Hitstun", 0, 0);
					}
					
					break;
				
				case PlayerState.ATTACK_LIGHT:

					velocity = Vector3.zero;
					setRBVelocity(velocity);
					attackTimer = 0;

                    fmodAudioManager.PlayFMODOneShot("PlayerAttackLow", transform.position, 0.6f);
                    break;
				
				case PlayerState.ATTACK_MEDIUM:

					velocity = Vector3.zero;
					setRBVelocity(velocity);
					attackTimer = 0;
                    fmodAudioManager.PlayFMODOneShot("PlayerAttackMid", transform.position, 0.6f);
                    break;
				
				case PlayerState.ATTACK_HEAVY:

					//animator.applyRootMotion = false;
					velocity = Vector3.zero;
					setRBVelocity(velocity);
					attackTimer = 0;

                    fmodAudioManager.PlayFMODOneShot("PlayerAttackHeavy", transform.position, 1f);
                    break;
				
				case PlayerState.DASH:
					dashTimer = 0;
					dashDirection = -Mathf.Sign(moveDirection.x);

                    FMOD.Studio.PLAYBACK_STATE dash_playbackState;
                    walkSFX.getPlaybackState(out dash_playbackState);
                    if (dash_playbackState.Equals(FMOD.Studio.PLAYBACK_STATE.STOPPED))
                    {
                        walkSFX.start();
                    }
                    else if (dash_playbackState.Equals(FMOD.Studio.PLAYBACK_STATE.STOPPING))
                    {
                        walkSFX.start();
                    }

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
			case PlayerState.IDLE:

				break;
			
			case PlayerState.MOVEMENT:
			
				break;
			
			case PlayerState.AERIAL:

				break;
			
			case PlayerState.HITSTUN:

				break;
			
			case PlayerState.ATTACK_LIGHT:
				// Ensure the player hurtbox is returned to normal
				gameObject.GetComponent<CapsuleCollider>().center = hurtboxCenter;
				gameObject.GetComponent<CapsuleCollider>().height = hurtboxHeight;

				break;
			
			case PlayerState.ATTACK_MEDIUM:

				break;
			
			case PlayerState.ATTACK_HEAVY:
				//animator.applyRootMotion = true;

				break;
		}
	}

	/*
	 * Determine what animation should be playing
	 * To do: Add enum for animation IDs
	 * 
	 * GRIFFIN'S NOTE:
	 * I restructured the animator pretty heavily. The only animations that use 'animID' are animations that loop.
	 * If you need to call an animation that only plays one time, such as any of the attacks, use 'animator.Play()'
	 * 
	 * animID reference:
	 * 0 = Idle
	 * 1 = Hitstun
	 * 2 = Block
	 * 3 = Walk (Front)
	 * 4 = Walk (Back)
	 */
	private void UpdateAnimation()
	{
		switch (currentState)
		{
			case PlayerState.IDLE:
				animator.SetInteger("animID", 0);
				break;
			
			case PlayerState.MOVEMENT:
				// Player 1: Left (negative x) = 4, Right (positive x) = 3
				// Player 2: Left = 3, Right = 4
				animator.SetInteger("animID", Mathf.Sign(moveDirection.x) == 1 ? 2 + playerID : 5 - playerID);
				break;
			
			case PlayerState.AERIAL:
				animator.SetInteger("animID", 0);
				break;
			
			case PlayerState.HITSTUN:
				// If the player was just in block and got guardbroken, play custom animation
				if (animator.GetInteger("animID") == 2)
				{
					animator.Play("Base Layer.Blockstun", 0, 0);
					animator.SetInteger("animID", 0);
				}
				else
				{
					animator.SetInteger("animID", 1);
				}
				break;
			
			case PlayerState.ATTACK_LIGHT:
				animatorinfo = animator.GetCurrentAnimatorClipInfo(0);

				if (animatorinfo[0].clip.name != "Low")
				{
					animator.Play("Base Layer.Low", 0, 0);
				}
				break;
			
			case PlayerState.ATTACK_MEDIUM:
				animatorinfo = animator.GetCurrentAnimatorClipInfo(0);

				if (animatorinfo[0].clip.name != "Mid")
				{
					animator.Play("Base Layer.Mid", 0, 0);
				}
				break;
			
			case PlayerState.ATTACK_HEAVY:
				animatorinfo = animator.GetCurrentAnimatorClipInfo(0);

				if (animatorinfo[0].clip.name != "Heavy")
				{
					animator.Play("Base Layer.Heavy", 0, 0);
				}
				break;
			
			case PlayerState.DASH:
				switch(dashDirection)
				{
					case 1:
						if(playerID == 1)
						{
							animator.Play("Base Layer.Dashback", 0, 0);
						}
						else
						{
							animator.Play("Base Layer.dashfront", 0, 0);
						}
						break;
					case -1:
						if (playerID == 1)
						{
							animator.Play("Base Layer.dashfront", 0, 0);
						}
						else
						{
							animator.Play("Base Layer.Dashback", 0, 0);
						}
						break;
				}
				break;
		}
	}

	// Helper function to check the attacks and changes to the appropriate state
	private void checkAttacks()
	{
		if (attackPower == 1)
		{
			//EnterState(PlayerState.GUARDBREAK);
		}
		if (attackPower == 2)
		{
			EnterState(PlayerState.ATTACK_LIGHT);
		}
		if (attackPower == 3)
		{
			EnterState(PlayerState.ATTACK_MEDIUM);
		}
		if (attackPower == 4)
		{
			EnterState(PlayerState.ATTACK_HEAVY);
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
		playerRB.velocity = clampVector3(v, currentState == PlayerState.DASH ? dashSpeed : maxSpeed);
	}
	
	// Spawns an hitbox
	public void createAttack(AttackValues av)
	{
		// Create the hitbox at the specified position
		GameObject prefabHitbox = av.type == 0 ? hitBox : guardBreakBox;
		GameObject bonk = Instantiate(prefabHitbox, new Vector3(this.gameObject.transform.position.x + (targetDirection * av.offset.x),
																this.gameObject.transform.position.y + av.offset.y,
																this.gameObject.transform.position.z),
																Quaternion.identity);
		// Resize the hitbox to the apporpriate size
		bonk.transform.localScale = Vector3.Scale(bonk.transform.localScale, av.hitboxScale);
		// Set the values for damage etc.
		bonk.GetComponent<HitBoxScript>().stunTime = av.stunTime;
		bonk.GetComponent<HitBoxScript>().damage = av.damage;
		bonk.GetComponent<HitBoxScript>().launchPower = new Vector3(targetDirection * av.launchPower.x, av.launchPower.y, av.launchPower.z);
		// Destroy the hitbox after the specified delay
		Destroy(bonk, av.lifespan);
	}

	// Called when the player collides with a hitbox and should take damage
	private void hitPlayer(GameObject hitbox)
	{
		// Deal damage to the player
		HP -= hitbox.GetComponent<HitBoxScript>().damage;
		// Zoom in the camera and slow down the game
		StartCoroutine(slowGame(hitbox.GetComponent<HitBoxScript>().damage));
		// Spawn the blood particles
		GameObject bloodSpray = Instantiate(hitParticles, gameObject.transform.position, Quaternion.identity);
		Destroy(bloodSpray, 1f);
		// Play the hit sound

		// Play the death animation if the player has died
		if (HP <= 0)
		{
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
		}
		else
		{
			// Launch the player based on the hitbox
			velocity = hitbox.GetComponent<HitBoxScript>().launchPower;
		}

		// Set the hitstun timer and update the state
		hitStunTimer = hitbox.GetComponent<HitBoxScript>().stunTime;
		EnterState(PlayerState.HITSTUN);
	}

	// Called when the player colides with a hitbox while they are blocking
	private void hitPlayerBlocking(GameObject hitbox)
	{
		// Spawn the blood particles
		GameObject blockTrigger = GameObject.Instantiate(blockParticles, gameObject.transform.position, Quaternion.identity);
		Destroy(blockTrigger, 1f);
		// Set the hitstun timer and update the state
		hitStunTimer = 5 + 3 * hitbox.GetComponent<HitBoxScript>().damage;
		EnterState(PlayerState.HITSTUN);
	}

	// Checks if the player collides with a higbox
	public void OnTriggerEnter(Collider trigger)
	{
		// If the player collides with a hitbox
		if (trigger.gameObject.CompareTag("hitbox"))
		{
			// If they are in the heavy attack and get hit by the light attack within the first few frames of the attack, ignore the damage
			if (currentState == PlayerState.ATTACK_HEAVY)
			{
				if (trigger.gameObject.GetComponent<HitBoxScript>().damage != 1 || attackTimer <= 50)
				{
					hitPlayer(trigger.gameObject);
				}
			}
			else
			{
				// If none of the above conditions are met, hit the player
				hitPlayer(trigger.gameObject);
			}
		}
	}

	// Slows down time and zooms in the camera, for effect
	public IEnumerator slowGame(float slowTime)
	{
		Time.timeScale = 0.1f;
		cameraMover.transform.position = new Vector3(cameraMover.transform.position.x, 0f, -1.5f);
		yield return new WaitForSeconds(slowTime * Time.timeScale);
		Time.timeScale = 1.0f;
		cameraMover.transform.position = new Vector3(cameraMover.transform.position.x, 0f, 0f);
	}

	// Toggles the trails on the axe head
	void toggleTrails(bool active)
	{
		trail1.SetActive(active);
		trail2.SetActive(active);
		trail3.SetActive(active);
		trail4.SetActive(active);
		trail5.SetActive(active);
	}

	// Prints the object and state, along with a message; for debugging purposes, turn on/off with logDebugMessages
	private void debugMessage(string message)
	{
		if (logDebugMessages)
		{
			Debug.Log(this.gameObject.name + " | " + this.currentState + ":\n" + message);
		}
	}

	// Input methods
	// Gets input from input system. Called when movement input
	public void Move(InputAction.CallbackContext context)
	{
		moveDirection = context.ReadValue<Vector2>();
	}

	// Gets input from input system. Called when attack input
	// 1 = guardbreak, 2 = light, 3 = mid, 4 = heavy
	public void Attack(InputAction.CallbackContext context)
	{
		attackPower = context.ReadValue<float>();
	}

	// Gets input from input system. Called when block input
	public void Block(InputAction.CallbackContext context)
	{
		//blocking = context.ReadValue<float>();
	}

	// Gets input from input system. Called when dash input
	public void Dash(InputAction.CallbackContext context)
	{
		dashing = context.ReadValue<float>();
	}

    public void exit(InputAction.CallbackContext context)
    {
		SceneManager.LoadSceneAsync("MainMenu");
    }
}