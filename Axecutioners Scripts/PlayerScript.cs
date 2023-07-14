using Mirror;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Processors;
using UnityEngine.SceneManagement;

public enum PlayerState
{
	IDLE,
	MOVEMENT,
	AERIAL,
	HITSTUN,
	ATTACK_LIGHT,
	ATTACK_MEDIUM,
	ATTACK_HEAVY,
	DASH
}

// Used by the createAttack function to create attacks with designer-specified values
// I made this to get rid of the magic numbers, Aaron. You happy now?
public struct AttackValues
{
	public int type;				// Type of attack: 0=attack, 1=guard break
	public Vector2 offset;			// Offset from player
	public Vector3 hitboxScale;		// Size of the hitbox
	public int stunTime;			// Length of time the player is in the hitstun state
	public int damage;				// Amount of damage the attack does
	public Vector3 launchPower;		// Amount of velocity applied to the player upon being hit
	public float lifespan;			// How long the hitbox lasts

	public AttackValues(int _type, Vector2 _offset, Vector3 _hitboxScale, int _stunTime, int _damage, Vector3 _launchPower, float _lifespan)
	{
		type = _type;
		offset = _offset;
		hitboxScale = _hitboxScale;
		stunTime = _stunTime;
		damage = _damage;
		launchPower = _launchPower;
		lifespan = _lifespan;
	}
}

public class PlayerScript : MonoBehaviour
{
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
	public string nextScene;
	private int sceneTransitionTime = 225; // What scene will be loaded after the player dies
	private int playReviveAnimation = 100;

    // ---------------------- External objects ----------------------
    //Audio
    public FMOD.Studio.EventInstance walkSFX;					// Walk SFX (All looping sounds need instance variables)
	// Managers
	public GameUIManager uiManager;								// Manages the UI of the game screen
	public AudioManager audioManager;                           // For playing sounds
	public FMODAudioManager fmodAudioManager;                   // For playing sounds
	public FMODMusicManager fmodMusicManager;					// For handling music updates

	public Animator animator;									// The animator for the player
	public GameObject cameraMover;								// Object used to move the camera to keep the players in the center and for zooming
	// Particles
	public ParticleSystem blockParticles;						// Particles to create when the player achieves full block
	public GameObject healthParticles;							// Particles to create when the player gets healed by the zone
	public ParticleSystem lowBlood;								// Particles to create when the player gets hit without blocking
	public ParticleSystem midBlood;								// Particles to create when the player gets hit without blocking
	public ParticleSystem dust1;								// Particles for dust while walking - Talyn
	public ParticleSystem dust2;								// Particles for dust while walking - Talyn
	public ParticleSystem wood1;								// Particles for wood splintering from floor or guard break - Talyn
	public ParticleSystem wood2;                                // Particles for wood splintering from floor or guard break - Talyn
	public ParticleSystem revive1;                              // Particle for when a player starts to get revived - Talyn
	public GameObject reviveObject;								// Object to move so the particles show up properly for dismemberments - Talyn
	// Prefabs
	public GameObject hitBox;   								// The gameobject that is created when attacking
	public GameObject guardBreakBox;							// The gameobject that is created when guard breaking
	public GameObject currHitbox;								// The hitbox currently being used in attacks				  - Griffin
	public GameObject lowDis;									// The gameobject that plays the low dismemberment animation  - Griffin
	public GameObject midDis;									// The gameobject that plays the mid dismemberment animation  - Griffin
	public GameObject highDis;									// The gameobject that plays the high dismemberment animation - Griffin
	public GameObject trail1;                                   // The gameobject that renders a trail during attack swings   - Griffin
	public GameObject trail2;                                   // The gameobject that renders a trail during attack swings   - Griffin
	public GameObject trail3;                                   // The gameobject that renders a trail during attack swings   - Griffin
	public GameObject trail4;                                   // The gameobject that renders a trail during attack swings   - Griffin
	public GameObject trail5;                                   // The gameobject that renders a trail during attack swings   - Griffin
	// Other
	public GameObject target;									// The object the player should be facing
	public Rigidbody playerRB;                                  // Rigidbody of the player, used for setting velocity
	public Transform crownPoint;								// Where the crown goes once it's been collected
	public GameObject necromancer;

	// ---------------------- Attack variables ----------------------
	private float attackPower;									// Updated by input package, used to check if player is attacking
	private int attackTimer;									// Timer for attack to synchronize events
	private int targetDirection;								// The direction the hitboxes should be created
	// Timings
	private int[] lightAttackTiming = {							// Timing for when to perform each section of the light attack
		20,		// Squish player hurtbox
		35,		// Enable attack hitbox and trail
		60,		// Unsquish player hurtbox, disable attack hitbox and trail
		82		// Move to next state
	};
	private int[] midAttackTiming = {							// Timing for when to perform each section of the mid attack
		35,		// Enable attack hitbox and trail
		60,     // Disable attack hitbox and trail
		104		// Move to next state
	};
	private int[] heavyAttackTiming = {							// Timing for when to perform each section of the heavy attack
		50,     // Enable attack hitbox and trail
		30,		// Start moving up
	    35,		// Stop moving up
		75,		// Disable attack hitbox and trail
		100,	// Start falling
		124		// Move to next state
	};
	// Values
	public AttackValues lightAttackValues = new AttackValues(	// Values for the light attack
		0,							// Type of hitbox (attack)
		new Vector2(1.9f, 0.1f),	// Hitbox offset from player
		new Vector3(3f, 2f, 3f),	// Hitbox scale
		30,							// Stun time
		1,							// Damage
		new Vector3(4f, 2f, 0f),	// Launch power
		0.1f						// Lifespan
	);
	public AttackValues midAttackValues = new AttackValues(	// Values for the mid attack
		0,							// Type of hitbox (attack)
		new Vector2(1.25f, 1.2f),	// Hitbox offset from player
		new Vector3(4f, 2f, 4f),	// Hitbox scale
		45,							// Stun time
		2,							// Damage
		new Vector3(7f, 2f, 0f),	// Launch power
		0.1f						// Lifespan
	);
	public AttackValues heavyAttackValues = new AttackValues(	// Values for the heavy attack
		0,							// Type of hitbox (attack)
		new Vector2(1.25f, 0f),		// Hitbox offset from player
		new Vector3(3f, 5f, 5f),	// Hitbox scale
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
		8,		// Start the dash movement (begin lag)
		17,		// Stop the dash movement (dash length)
		20		// Move to the next state (end lag)
	};

	// ---------------------- Zone Variables ----------------------
	private bool inZone;										// If the player is in the zone, set by the zone itself
	private int zoneTimer;										// How long the player has been in the zone
	private int maxZoneTime = 25;                              // The amount of time needed to be in the zone for something to happen
	private bool isCrowned = false;
	public GameObject crownZone;

	// ---------------------- AI Variables ----------------------
	public delegate IEnumerator AttackEvent(GameObject caller, float extraDelay);	// Event type declaration
	public static event AttackEvent attackStarted;              // Event used by AI for when player starts attacking
	public delegate void DefenseEvent(GameObject caller);		// Event type declaration
	public static event DefenseEvent someoneHit;				// Event used by AI for when someone gets hit

	// ---------------------- Player State & Input Variables ----------------------
	public PlayerState currentState;							// The current state of the player, used by the state machine
	public InputScheme inputScheme;								// The input scheme (keyboard, gamepad, etc.) the player is using
	public InputDevice inputDevice;                             // The input device the player is currently using
    private Vector2 moveDirection;								// Updated by input package, vec2 holding the input from keyboard for movement. Ranges from -1 to 1 for x and y
	private Vector3 velocity;									// A vector to hold the velocity of the player because the rigidbody velocity is weird
	private bool grounded;										// If the player is on the ground
	private AnimatorClipInfo[] animatorinfo;                    // Stores information about animation clips to ensure that one time animations play properly - Griffin

	// ---------------------- Screen Shake Variables ----------------------
	private bool isShaking = false;								// Whether the camera should be shaking or not
	private float duration = 0f;								// How long the screen should shake for
	private float magnitude = 0.7f;								// How much the screen should shake
	private float dampingSpeed = 1f;							// How quickly the shake should settle
	private Vector3 initialPosition;							// The initial position for the camera to return to

	// ---------------------- Miscellaneous Timers ----------------------
	private int hitStunTimer;									// The amount of time the player stays in the hitstun state after being hit
	private int sceneTransitionTimer;                           // Timer to determine when to transition to game over/next round
	private int walkSoundBufferTimer;							// Timer to smooth the stopping of walk sfx
	private int timeInAerial;									// Timer to leave aerial state if you're in it for too long
	private int maxAerialTime = 50;								// Maximum time you're allowed to be in aerial

	// ---------------------- Debug ----------------------
	private bool logDebugMessages = false;                      // Turn on to display state change messages

	// ---------------------- Networking ----------------------
	public bool networkActive;                          //if set to true, then we are in multiplayer and commands/RPCs will be called from player actions
	public bool networkPause;                           //if set to true, don't run update() for players or managers
	public int networkStunTime, networkDamage;			//hitbox data that is received through the network
	public Vector3 networkLaunchPower;                  //hitbox data that is received through the networkpartial
	public bool dead = false;

    // ---------------------- Functions ----------------------
    // Called once at the start
    void Start()
	{
		fmodMusicManager = FMODMusicManager.instance;
		//if (networkActive)
		//    if (!gameObject.GetComponent<NetworkPlayerController>().isOwned)
		//        return;

		networkActive = false;
		networkPause = false;

		walkSoundBufferTimer = 6;

        // Start the player off idle
        EnterState(PlayerState.IDLE);

        if (networkActive == false && fmodAudioManager)
		{
            //Create FMOD Event Instance of all Looping SFX and attach to player
            walkSFX = fmodAudioManager.CreateFMODEventInstance("PlayerWalk");
            FMODUnity.RuntimeManager.AttachInstanceToGameObject(walkSFX, gameObject.transform);
        }

        if (playerID == 2)
		{
			// Reflect player 2 about the x axis
			this.gameObject.transform.localScale = new Vector3(-1f, 1f, 1f);
		}

		//Reset HP Music Variable On Start
		switch (playerID)
		{
			case 1:
				fmodMusicManager.P1Health = HP;
				break;
			case 2:
				fmodMusicManager.P2Health = HP;
				break;
			default:
				break;
		}
	}

	public void ResetTimers()
	{
		hitStunTimer = 0;
		sceneTransitionTimer = 0;
		walkSoundBufferTimer = 6;
        zoneTimer = 0;
    }

	// Called every frame
	void FixedUpdate()
	{
		if (networkPause == false)
		{
			// Move the camera to keep the players in frame
			if (target && cameraMover)
			{
				cameraMover.transform.position = new Vector3((playerRB.transform.position.x + target.transform.position.x) * 0.5f,
														  cameraMover.transform.position.y,
														  cameraMover.transform.position.z);
				// Update the target position to face the other player
				targetDirection = (target.transform.position.x > this.transform.position.x) ? 1 : -1;
			}

			// Shake the camera
			if (isShaking)
			{
				if (duration > 0f)
				{
					cameraMover.transform.position = initialPosition + UnityEngine.Random.insideUnitSphere * magnitude;

					duration -= Time.deltaTime * dampingSpeed;
				}
				else 
				{
					cameraMover.transform.position = initialPosition;
					isShaking = false;
				}
			}

			// Added to try and correct errors in animation rotation - Griffin
			// Player 1 faces -100 and player 2 100
			this.gameObject.transform.eulerAngles = new Vector3(0, 200f * playerID - 300f, 0);

			// Run the game
			if (uiManager)
			{
                if (!uiManager.isPaused)
                {
                    // If the player is alive
                    if (HP > 0)
                    {
                        // Update the player grounded state
                        checkGrounded();
                        // Run the zone logic
                        runZone();
                        // Run the state machine
                        ChangeState();
                        UpdateState();

                        // If the timer runs out of time
                        if (uiManager.roundRemainingTime <= 1)
                        {
                            // Either go to the next round or go to the end scene
                            if (networkActive)
                                LobbyController.Instance.restart = true;
                            advanceRound();
                        }
                    }
                    else
                    {
						if (networkActive && dead == false)
						{
							dead = true;

							gameObject.GetComponent<NetworkPlayerController>().IsDead();
						}

						// Make sure gravity exists
						checkGrounded();
						velocity.y = calculateGravity(velocity.y);
						setRBVelocity(velocity);
						
                        // Wait a bit
                        sceneTransitionTimer++;
                        if (sceneTransitionTimer == playReviveAnimation)
                        {
                            if (playerID == 1)
                            {
								necromancer.GetComponent<NecromancerScript>().ReviveRight();
                            }
                            else
                            {
								necromancer.GetComponent<NecromancerScript>().ReviveLeft();
                            }
                        }

                        if (sceneTransitionTimer == sceneTransitionTime)
                        {
                            // Update the score (the other player gets the point)
                            RoundManager.points[2 - playerID]++;

                            // Either go to the next round or go to the end scene
                            if (networkActive)
								LobbyController.Instance.restart = true;
                            advanceRound();
                        }
                    }
                }
            }
		}
	}

	//networking
	public void ResetCamera()
    {
		cameraMover.transform.position = new Vector3((playerRB.transform.position.x + target.transform.position.x) * 0.5f,
														  cameraMover.transform.position.y,
														  cameraMover.transform.position.z);
		// Update the target position to face the other player
		targetDirection = (target.transform.position.x > this.transform.position.x) ? 1 : -1;
	}

	// Perform the logic for the state. Called every frame
	void UpdateState()
	{
		switch (currentState)
		{
			case PlayerState.IDLE:
				// Velocity is set to 0 upon entering the state
				// Apply gravity (just in case)
				animator.SetInteger("animID", 0);
				velocity.y = calculateGravity(velocity.y);
				setRBVelocity(velocity);

				//decrease walk sound buffer timer
				break;
			
			case PlayerState.MOVEMENT:
				//Reset walksoundbuffertimer
				walkSoundBufferTimer = 6;

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
					UpdateAnimation();
				}
				setRBVelocity(velocity);
				break;
			
			case PlayerState.AERIAL:
				// Apply gravity
				velocity.y = calculateGravity(velocity.y);
				setRBVelocity(velocity);
				timeInAerial++;
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

				// Squish player hurtbox
				if (attackTimer == lightAttackTiming[0])
				{
					gameObject.GetComponent<BoxCollider>().size = new Vector3(1f, hurtboxHeight * 0.4f, 1f);
					gameObject.GetComponent<BoxCollider>().center = new Vector3(hurtboxCenter.x, gameObject.GetComponent<BoxCollider>().size.y / 2f, hurtboxCenter.z);
				}

				// Spawn attack hitbox
				if (attackTimer == lightAttackTiming[1])
				{
					createAttack(lightAttackValues);
					toggleTrails(true);
				}

				// Unsquish player hurtbox
				if (attackTimer == lightAttackTiming[2])
				{
					gameObject.GetComponent<BoxCollider>().size   = new Vector3(1f, hurtboxHeight, 1f);
					gameObject.GetComponent<BoxCollider>().center = hurtboxCenter;

					currHitbox.SetActive(false);
					toggleTrails(false);
				}
				break;
			
			case PlayerState.ATTACK_MEDIUM:
				// Count up timer
				attackTimer++;

				// Spawn attack hitbox
				if (attackTimer == midAttackTiming[0])
				{
					createAttack(midAttackValues);
					toggleTrails(true);
				}

				if(attackTimer == midAttackTiming[1])
                {
					currHitbox.SetActive(false);
					toggleTrails(false);
				}
				break;
			
			case PlayerState.ATTACK_HEAVY:
				// Count up timer
				attackTimer++;

				// Actually spawn the hitbox
				if (attackTimer == heavyAttackTiming[0])
				{
					createAttack(heavyAttackValues);
					toggleTrails(true);
				}

				// When the player begins jumping
				if (attackTimer == heavyAttackTiming[1])
				{
					// Set the player velocity to launch them upwards
					velocity = new Vector3(0f, 2f, 0f);
				}

				// When the player has reached the top of the jump
				if (attackTimer == heavyAttackTiming[2])
				{
					// Stop the player from accelerating and moving
					velocity = Vector3.zero;
				}

				if(attackTimer == heavyAttackTiming[3])
				{
					currHitbox.SetActive(false);
					toggleTrails(false);
				}

				if(attackTimer == 60)
				{
					wood1.Play(true);
				}

				// Make the player fall down again
				if (attackTimer >= heavyAttackTiming[4])
				{
					velocity.y = calculateGravity(velocity.y);
				}
				
				// Apply the calculated velocity to the player
				setRBVelocity(velocity);
				break;
			
			case PlayerState.DASH:
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
				if (timeInAerial > maxAerialTime)
				{
					grounded = true;
					gameObject.transform.position = new Vector3(gameObject.transform.position.x, 0.00999999f, gameObject.transform.position.z);
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
				if (attackTimer == midAttackTiming[2])
				{
					EnterState(PlayerState.IDLE);
				}
				break;
			
			case PlayerState.ATTACK_HEAVY:
				if (attackTimer == heavyAttackTiming[5])
				{
					EnterState(PlayerState.AERIAL);
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
	public void EnterState(PlayerState newState)
	{
		if (newState != currentState)
		{
			if (dust1 && dust2)
			{
                dust1.Stop();
                dust2.Stop();
            }
			
			ExitState();
			currentState = newState;
			debugMessage("Enter State");
			switch (newState)
			{
				case PlayerState.IDLE:
					velocity = Vector3.zero;
					animator.applyRootMotion = false;

					break;
				
				case PlayerState.MOVEMENT:
					//audioManager.Play("Walking");
					FMOD.Studio.PLAYBACK_STATE walk_playbackState;
					walkSFX.getPlaybackState(out walk_playbackState);
					if(walk_playbackState.Equals(FMOD.Studio.PLAYBACK_STATE.STOPPED))
					{
						walkSFX.start();
					}
					else if(walk_playbackState.Equals(FMOD.Studio.PLAYBACK_STATE.STOPPING))
                    {
						walkSFX.start();
					}

                    if (dust1 && dust2)
					{
                        dust1.Play(true);
                        dust2.Play(true);
                    }
					break;
				
				case PlayerState.AERIAL:
					timeInAerial = 0;
					animator.applyRootMotion = false;
					break;
				
				case PlayerState.HITSTUN:
					toggleTrails(false);

					animator.applyRootMotion = false;
					// Play hitstun animation if not dead
					if (HP > 0)
					{
						animator.Play("Base Layer.HitstunStartup", 0, 0);
					}
					
					break;
				
				case PlayerState.ATTACK_LIGHT:

					// Alert AI that attack has started
					if(attackStarted != null)
					{
						StartCoroutine(attackStarted.Invoke(gameObject, 0));
					}

					velocity = Vector3.zero;
					setRBVelocity(velocity);
					attackTimer = 0;
					//audioManager.PlayDelayed("LightAttack", 0.6f);
					fmodAudioManager.PlayFMODOneShot("PlayerAttackLow", transform.position, 0.6f);
					break;
				
				case PlayerState.ATTACK_MEDIUM:

					// Alert AI that attack has started
					if (attackStarted != null)
					{
						StartCoroutine(attackStarted.Invoke(gameObject, 0));
					}

					velocity = Vector3.zero;
					setRBVelocity(velocity);
					//audioManager.PlayDelayed("MidAttack", 0.6f);
					fmodAudioManager.PlayFMODOneShot("PlayerAttackMid", transform.position, 0.6f);
					attackTimer = 0;
					break;
				
				case PlayerState.ATTACK_HEAVY:

					// Alert AI that attack has started
					if (attackStarted != null)
					{
						StartCoroutine(attackStarted.Invoke(gameObject, 0));

						// Since the heavy attack has so much startup, give the AI an extra chance to react
						StartCoroutine(attackStarted.Invoke(gameObject, .25f));
					}

					animator.applyRootMotion = true;
					velocity = Vector3.zero;
					setRBVelocity(velocity);
					//audioManager.PlayDelayed("HeavyAttack", 1f);
					fmodAudioManager.PlayFMODOneShot("PlayerAttackHeavy", transform.position, 1f);
					attackTimer = 0;
					break;
				
				case PlayerState.DASH:
                    if (dust1 && dust2)
					{
                        dust1.Play(true);
                        dust2.Play(true);
                    }

					dashTimer = 0;
					dashDirection = -Mathf.Sign(moveDirection.x);
					//audioManager.Play("Walking");
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
				//FMOD.Studio.PLAYBACK_STATE walkstop_playbackState;
				//walkSFX.getPlaybackState(out walkstop_playbackState);
				//if (walkstop_playbackState.Equals(FMOD.Studio.PLAYBACK_STATE.PLAYING))
				//{
				//	walkSFX.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
				//}
				break;
			
			case PlayerState.AERIAL:
				grounded = true;
				break;
			
			case PlayerState.HITSTUN:

				break;
			
			case PlayerState.ATTACK_LIGHT:
				// Ensure the player hurtbox is returned to normal
				gameObject.GetComponent<BoxCollider>().center = hurtboxCenter;
				gameObject.GetComponent<BoxCollider>().size   = new Vector3(1f, hurtboxHeight, 1f);
				attackPower = -1;
				currHitbox.SetActive(false);
				toggleTrails(false);

				break;
			
			case PlayerState.ATTACK_MEDIUM:
				attackPower = -1;
				currHitbox.SetActive(false);
				toggleTrails(false);

				break;
			
			case PlayerState.ATTACK_HEAVY:
				//animator.applyRootMotion = true;
				attackPower = -1;
				currHitbox.SetActive(false);
				toggleTrails(false);

				break;
		}
	}

	public void ResetNetworkAnimation()
	{
		animator.Play("Base_Layer.Dashback");
		//animator.Play("Base_Layer.Idle");
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
	public void UpdateAnimation()
	{
		//if (networkActive)
		//	if (!gameObject.GetComponent<NetworkPlayerController>().isOwned)
		//		return;

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

	// Either reload the play scene or go to the end scene
	public void advanceRound()
	{
        // If the max number of wins has been reached, go to the end scene
        // Otherwise, reset the ui and reload the current scene
        if (RoundManager.points[target.GetComponent<PlayerScript>().playerID - 1] < RoundManager.numRounds)
		{
            // This is added just in case this scrip is used in a scene that does not have a UI manager
            if (uiManager)
			{
				uiManager.ResetTimers();
			}
			
			RoundManager.playerInputSchemes[playerID - 1] = inputScheme;
			RoundManager.playerInputSchemes[target.GetComponent<PlayerScript>().playerID - 1] = target.GetComponent<PlayerScript>().inputScheme;

            //if multiplayer is active, call an RPC to server
            //else do normal death
            if (networkActive)
			{
				Debug.Log("Restart network scene");
                gameObject.GetComponent<NetworkPlayerController>().RestartGame();
            }
            else
			{
                RoundManager.currentRound++;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
		}
		else
		{
            RoundManager.currentRound = 1;

			//if multiplayer is active, call an RPC to server
			//else do normal end scene
			if (networkActive)
			{
				//networkActive = false;
                gameObject.GetComponent<NetworkPlayerController>().EndGame(nextScene);
            }
			else
				SceneManager.LoadScene(nextScene);
		}
	}

	// Helper function to check the attacks and changes to the appropriate state
	public void checkAttacks()
	{
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

	// Perform the logic of the zone
	private void runZone()
	{
		if (inZone && currentState != PlayerState.HITSTUN)
		{
			zoneTimer++;
			if (crownZone && zoneTimer >= maxZoneTime && !target.GetComponent<PlayerScript>().isCrowned)
			{
				//if multiplayer is active, then call a crown RPC
				//else do normal crown player
				if (networkActive)
                    gameObject.GetComponent<NetworkPlayerController>().Crown(SteamLobby.Instance.GetIndex(gameObject.GetComponent<NetworkIdentity>().assetId));
				else
					CrownPlayer();
			}
		}
	}
	public void CrownPlayer()
	{
        inZone = false;
        isCrowned = true;

        //Play crown get sfx
        fmodAudioManager.PlayFMODOneShot("PlayerCrownGet", transform.position);

        //Music variable
        //fmodMusicManager.PlayerHasCrown = 1;
        fmodMusicManager.DoHotZoneFadeOut();
        fmodMusicManager.DoPlayerCrown();

        crownZone.GetComponent<Crown>().DestroyZone(this);
    }

	// Check if the player is on the ground. Called every frame
	private void checkGrounded()
	{
        // Raycast directly below the player
        if (Physics.Raycast(playerRB.transform.position, Vector3.down, out RaycastHit hit, 0.05f))
		{
            // Check if the ground is there and update grounded appropriately
			if (hit.transform.gameObject.CompareTag("Ground"))
			{
				grounded = true;
				return;
			}
            
        }
        grounded = false;
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
		currHitbox = av.type == 0 ? hitBox : guardBreakBox;

		currHitbox.SetActive(true);
		HitBoxScript hbScript = currHitbox.GetComponent<HitBoxScript>();

		//ghetto way of transferring the hitbox info across the network
		if (networkActive && networkDamage != 0 && networkStunTime != 0 && networkLaunchPower != Vector3.zero)
		{
			hbScript.stunTime = networkStunTime;
			hbScript.damage = networkDamage;
			hbScript.launchPower = new Vector3(targetDirection * networkLaunchPower.x, networkLaunchPower.y, networkLaunchPower.z);
		}
		else
        {
			hbScript.stunTime = av.stunTime;
			hbScript.damage = av.damage;
			hbScript.launchPower = new Vector3(targetDirection * av.launchPower.x, av.launchPower.y, av.launchPower.z);
		}

		/*
		 * This has been commented out in favor of the new hitbox system.
		 * I didn't outright delete it in case we need to access it in the future.
		 * - Griffin
		 * 
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
		*/
	}

	//in case of latency and player did not enter hitstun, but took damage according to the other client, then do a default mid hitstun/death/dismember
	public void NetworkHit()
	{
		//make sure to update rounds
		gameObject.GetComponent<NetworkPlayerController>().SetPoints(RoundManager.points[0], RoundManager.points[1]);

        StartShake(0.2f, 0.2f);

        //Music Variables
        switch (playerID)
        {
            case 1:
                fmodMusicManager.P1Health = HP;
                break;
            case 2:
                fmodMusicManager.P2Health = HP;
                break;
            default:
                break;
        }

        midBlood.Play();
        fmodAudioManager.PlayFMODOneShot("PlayerHit", transform.position);
        hitStunTimer = midAttackValues.stunTime;

        EnterState(PlayerState.HITSTUN);
        StartCoroutine(doHitstop(0.15f));

        // Play the death animation if the player has died
        if (HP <= 0)
        {
            bool isDismember = target.GetComponent<PlayerScript>().isCrowned;

            velocity = Vector3.zero;
            gameObject.GetComponent<BoxCollider>().enabled = false;
            revive1.Play(true);

            if (isDismember)
            {
                GameObject a = Instantiate(midDis, gameObject.transform.position, new Quaternion(0f, 217f * playerID - 325.5f, 0f, 0f));
                a.transform.eulerAngles = new Vector3(0f, 217f * playerID - 325.5f, 0f);
                gameObject.transform.Translate(0, -10, 0);

                fmodMusicManager.PlayerHit = 3;
                //Code to find the dismemberment animation and send the particle effect to it. -Talyn
                reviveObject = GameObject.Find("MidDisP1(Clone)");
                if (reviveObject == null)
                {
                    reviveObject = GameObject.Find("MidDisP2(Clone)");
                }
                revive1.transform.parent = null;
                revive1.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                revive1.transform.position = reviveObject.transform.position + new Vector3(0, 0, 0);
            }
            else
            {
                animator.Play("Base Layer.Death_Mid", 0, 0f);
                animator.SetInteger("animID", 5);
                fmodMusicManager.DoPlayerHit(2);
            }
        }
        else
        {
            // Launch the player based on the hitbox
            velocity = midAttackValues.launchPower;
            fmodMusicManager.DoPlayerHit(1);
        }
    }

	// Called when the player collides with a hitbox and should take damage
	private void hitPlayer(GameObject hitbox)
	{
        // Deal damage to the player
        HP -= hitbox.GetComponent<HitBoxScript>().damage;

        // Alert AI that player has been hit
        someoneHit?.Invoke(gameObject);

		//if (networkActive)
		//	gameObject.GetComponent<NetworkPlayerController>().UpdateHealth(HP, SteamLobby.Instance.GetIndex(gameObject.GetComponent<NetworkIdentity>().assetId));

		StartShake(0.2f, 0.2f);

		//Music Variables
		switch(playerID)
        {
			case 1:
				fmodMusicManager.P1Health = HP;
				break;
			case 2:
				fmodMusicManager.P2Health = HP;
				break;
			default:
				break;
        }

		// Zoom in the camera and slow down the game
		//StartCoroutine(slowGame(hitbox.GetComponent<HitBoxScript>().damage));
		// Spawn the blood particles
		switch (hitbox.GetComponent<HitBoxScript>().damage) {
			case 1:
				lowBlood.Play();
				break;
			case 2:
				midBlood.Play();
				break;
			case 3:
				midBlood.Play();
				break;
		}
		// Play the hit sound
		//audioManager.Play("PlayerHit");
		fmodAudioManager.PlayFMODOneShot("PlayerHit", transform.position);

        // Set the hitstun timer and update the state
        hitStunTimer = hitbox.GetComponent<HitBoxScript>().stunTime;
        EnterState(PlayerState.HITSTUN);
        StartCoroutine(doHitstop(0.15f));

        // Play the death animation if the player has died
        if (HP <= 0)
		{
			bool isDismember = target.GetComponent<PlayerScript>().isCrowned;

			velocity = Vector3.zero;
			gameObject.GetComponent<BoxCollider>().enabled = false;
			revive1.Play(true);
			// Play the death sound
			//audioManager.Play("PlayerDeath");
			//fmodAudioManager.PlayFMODOneShot("PlayerDeathCrowdCheer", transform.position);
			// Play the correct animation based on what attack it was
			switch(hitbox.GetComponent<HitBoxScript>().damage)
			{
				case 1:
					if (isDismember) 
					{
						Vector3 dismemberObjectPos = new Vector3(gameObject.transform.position.x, 0, gameObject.transform.position.z);
						GameObject a = Instantiate(lowDis, dismemberObjectPos, gameObject.transform.rotation);
						a.transform.eulerAngles = new Vector3(0f, 217f * playerID - 325.5f, 0f);
						gameObject.transform.Translate(0, -10, 0);
						//fmodMusicManager.PlayerHit = 3;
						fmodMusicManager.DoPlayerHit(3);

						//Code to find the dismemberment animation and send the particle effect to it. -Talyn
						reviveObject = GameObject.Find("LowDisP1(Clone)");
						if (reviveObject == null)
						{
							reviveObject = GameObject.Find("LowDisP2(Clone)");
						}
						revive1.transform.parent = null;
						revive1.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
						revive1.transform.position = reviveObject.transform.position + new Vector3(0, 0, 0);
					}
					else 
					{
						animator.Play("Base Layer.Death_Low", 0, 0f);
						animator.SetInteger("animID", 5);
						//fmodMusicManager.PlayerHit = 2;
						fmodMusicManager.DoPlayerHit(2);
					}
					break;
				case 2:
					if (isDismember) 
					{
						Vector3 dismemberObjectPos = new Vector3(gameObject.transform.position.x, 0, gameObject.transform.position.z);
						GameObject a = Instantiate(midDis, dismemberObjectPos, new Quaternion(0f, 217f * playerID - 325.5f, 0f, 0f));
						a.transform.eulerAngles = new Vector3(0f, 217f * playerID - 325.5f, 0f);
						gameObject.transform.Translate(0, -10, 0);

						fmodMusicManager.PlayerHit = 3;
						//Code to find the dismemberment animation and send the particle effect to it. -Talyn
						reviveObject = GameObject.Find("MidDisP1(Clone)");
						if (reviveObject == null)
						{
							reviveObject = GameObject.Find("MidDisP2(Clone)");
						}
						revive1.transform.parent = null;
						revive1.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
						revive1.transform.position = reviveObject.transform.position + new Vector3(0, 0, 0);
					}
					else 
					{
						animator.Play("Base Layer.Death_Mid", 0, 0f);
						animator.SetInteger("animID", 5);
						//fmodMusicManager.PlayerHit = 2;
						fmodMusicManager.DoPlayerHit(2);
					}
					break;
				case 3:
					if (isDismember) 
					{
						Vector3 dismemberObjectPos = new Vector3(gameObject.transform.position.x, 0, gameObject.transform.position.z);
						GameObject a = Instantiate(highDis, dismemberObjectPos, gameObject.transform.rotation);
						a.transform.eulerAngles = new Vector3(0f, 300f * playerID - 450f, 0f);
						gameObject.transform.Translate(0, -10, 0);

						//fmodMusicManager.PlayerHit = 3;
						fmodMusicManager.DoPlayerHit(3);
						//Code to find the dismemberment animation and send the particle effect to it. -Talyn
						reviveObject = GameObject.Find("HighDisP1(Clone)");
						if (reviveObject == null)
						{
							reviveObject = GameObject.Find("HighDisP2(Clone)");
						}
						revive1.transform.parent = null;
						revive1.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
						revive1.transform.position = reviveObject.transform.position + new Vector3 (0, 0, 0);
						
					}
					else 
					{
						animator.Play("Base Layer.Death_High", 0, 0f);
						animator.SetInteger("animID", 5);
						//fmodMusicManager.PlayerHit = 2;
						fmodMusicManager.DoPlayerHit(2);
					}
					break;
			}
		}
		else
		{
			// Launch the player based on the hitbox
			velocity = hitbox.GetComponent<HitBoxScript>().launchPower;
			//fmodMusicManager.PlayerHit = 1;
			fmodMusicManager.DoPlayerHit(1);
		}

	}

	// Called when the player colides with a hitbox while they are blocking
	private void hitPlayerBlocking(GameObject hitbox)
	{
		// Alert AI that player has been hit
		someoneHit?.Invoke(gameObject);
		// Play the hit sound
		//audioManager.Play("FullBlock");
		fmodAudioManager.PlayFMODOneShot("PlayerBlockFull", transform.position);
		// Spawn the blood particles
		blockParticles.Play();
		//Destroy(blockTrigger, 1f);
		// Set the hitstun timer and update the state
		hitStunTimer = 10 + 10 * hitbox.GetComponent<HitBoxScript>().damage;
		EnterState(PlayerState.HITSTUN);
	}

	// Checks if the player collides with a higbox
	public void OnTriggerEnter(Collider trigger)
	{
		// If the player collides with a hitbox
		if (trigger.gameObject.CompareTag("hitbox") && trigger.gameObject != currHitbox)
		{
			// If they are in the heavy attack and get hit by the light attack within the first few frames of the attack, ignore the damage
			if (currentState == PlayerState.ATTACK_HEAVY)
			{
				if (trigger.gameObject.GetComponent<HitBoxScript>().damage != 1 || attackTimer <= 30)
				{
					hitPlayer(trigger.gameObject);
				}
			}
			else
			{
				// If none of the above conditions are met, hit the player
				hitPlayer(trigger.gameObject);
			}

			// Turn off hitbox to prevent double hits
			trigger.gameObject.SetActive(false);
		}
	}

	// Slows down time and zooms in the camera, for effect
	public IEnumerator slowGame(float slowTime)
	{
		if (HP > 0 && target.GetComponent<PlayerScript>().HP > 0)
		{
			Time.timeScale = 0.1f;
			cameraMover.transform.position = new Vector3(cameraMover.transform.position.x, 0f, -1.5f);
			yield return new WaitForSeconds(slowTime * Time.timeScale);
			Time.timeScale = 1.0f;
			cameraMover.transform.position = new Vector3(cameraMover.transform.position.x, 0f, 0f);
		}
	}

	public IEnumerator doHitstop(float stopTime) 
	{
		if (target)
		{
            animator.speed = 0;
            target.GetComponent<Animator>().speed = 0;

            yield return new WaitForSeconds(stopTime);
            animator.speed = 1;
            target.GetComponent<Animator>().speed = 1;
        }
	}

	// Called by the zone script to toggle the player state (in/out of zone)
	public void setInZone(bool setZone)
	{
		// Music variables
		if(setZone)
        {
			fmodMusicManager.DoHotZoneFadeIn();
			fmodMusicManager.DoPlayerEnterHotZone();
        }

		else
        {
			fmodMusicManager.DoHotZoneFadeOut();
			fmodMusicManager.DoPlayerExitHotZone();
		}

		debugMessage("Set zone: " + setZone.ToString());
		inZone = setZone;
		zoneTimer = 0;
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

	// Functions used by AI to control player
	public void AISetAttacking(int attackValue)
	{
		attackPower = attackValue;
	}

	public void AIMovement(Vector2 movement)
	{
		moveDirection = movement;
	}

	public void AISetDashing(int dashValue)
	{
		dashing = dashValue;
	}

	// Input methods
	// Gets input from input system. Called when movement input
	public void Move(InputAction.CallbackContext context)
	{
		if (networkActive)
			if (!gameObject.GetComponent<NetworkPlayerController>().isOwned)
				return;

		moveDirection = context.ReadValue<Vector2>();
	}
    public void Move(Vector2 power)
    {
        //if (networkActive)
        //    if (!gameObject.GetComponent<NetworkPlayerController>().isOwned)
        //        return;

        moveDirection = power;
    }

    // Gets input from input system. Called when attack input
    // 1 = guardbreak, 2 = light, 3 = mid, 4 = heavy
    public void Attack(InputAction.CallbackContext context)
	{
        if (networkActive)
            if (!gameObject.GetComponent<NetworkPlayerController>().isOwned)
                return;

        attackPower = context.ReadValue<float>();
	}
	//steamworks did not like input system parameters
	public void Attack(float power, int dmg, int stun, Vector3 launch)
	{
        //if (networkActive)
        //    if (!gameObject.GetComponent<NetworkPlayerController>().isOwned)
        //        return;

		networkStunTime = stun;
		networkDamage = dmg;
		networkLaunchPower = launch;
		attackPower = power;
	}
	public void Block(float power)
	{
        //blocking = power;
    }

	// Gets input from input system. Called when dash input
	public void Dash(InputAction.CallbackContext context)
	{
        if (networkActive)
            if (!gameObject.GetComponent<NetworkPlayerController>().isOwned)
                return;

        dashing = context.ReadValue<float>();
	}
    public void Dash(float power)
    {
        //if (networkActive)
        //    if (!gameObject.GetComponent<NetworkPlayerController>().isOwned)
        //        return;

        dashing = power;
    }

	// Begin shaking the screen
	// duration  - How long to shake for
	// magnitude - How intense the shake should be
	public void StartShake(float duration, float magnitude)
    {
		isShaking = true;
		this.duration  = duration;
		this.magnitude = magnitude;
		initialPosition = cameraMover.transform.position;
    }

    //reset crown for networking
    public void ResetCrown()
    {
		isCrowned = false;
		crownZone.GetComponent<Crown>().Reset();
    }
	//for networking input
	public bool IsAttacking()
    {
		if (currentState == PlayerState.ATTACK_HEAVY ||
			currentState == PlayerState.ATTACK_MEDIUM ||
			currentState == PlayerState.ATTACK_LIGHT)
			return true;
		else
			return false;
    }
}