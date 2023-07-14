using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScript : MonoBehaviour
{
    // Need to do these weird variable declarations so that the headers look good in editor

    [Header("Basic Movement"), SerializeField]
    private float groundSpeed;

    [SerializeField]
    private float airSpeed, rotationTime, jumpForce, gravity, maxFallSpeed, forceDecayRate, maxCoyoteTime;

    [Header("Jetpack Durability"), SerializeField]
    private Slider healthBar;

    [SerializeField]
    private float maxHealth, healthDecayRate,lowDamage,mediumDamage,highDamage, lowHealthWarning, damageStage;

    [Header("Air Jumps"), SerializeField]
    private int maxAirJumps;

    [SerializeField]
    private float airJumpBaseCost, airJumpCostMultiplier;

    [Header("Final Explosion"), SerializeField]
    private float forwardsForce;

    [SerializeField]
    private float upwardsForce;

    [Header("Long Jump"), SerializeField]
    private float longJumpHeight;

    [SerializeField]
    private float longJumpDistance, stationaryPenalty, longJumpCost;

    [Header("High Jump"), SerializeField]
    private float highJumpChargeTime;

    [SerializeField]
    private float highJumpHeight, highJumpCost, highJumpSpeedMultiplier;

    [Header("Dash"), SerializeField]
    private float dashSpeed;

    [SerializeField]
    private float dashDistance, dashBaseCost, dashCostMultiplier, dashTimeLimit;

    [Header("Hover"), SerializeField]
    private float hoverBaseDamage;

    [SerializeField]
    private float hoverDamageRate;

    private const float DASH_MIN_DISTANCE = .1f;

    private float turnSmoothVelocity, airJumpCurrentCost, baseAirSpeed, baseGroundSpeed,
                  highJumpGroundSpeed, highJumpAirSpeed, dashTimer, dashCurrentCost, hoverDamageTimer = 0, coyoteTimer = 0;

    public float currentHealth { get; private set; }
    public float highJumpTimer { get; private set; } = 0;

    public bool jumpPressed { get; private set; } = false;
    public bool isGrounded { get; private set; } = true;

    private int airJumpsLeft;
    private bool jetpackExploded = false, dashStarted = false, applyGravity = true, lowSFX = false, dashSFX = false;
    private float walkSFXTimer= 0f;
    private PlayerControls playerInput;
    private CharacterController controller;
    private Transform mainCamera;
    private Vector3 movementVector, WASDMovementVector, forceVector, dashTarget;
    private AudioManager am;
    private GameManager gm;
    private FXScript vfx;

    private void Start()
    {
        playerInput = new PlayerControls();
        controller = GetComponent<CharacterController>();
        mainCamera = Camera.main.transform;
        am = FindObjectOfType<AudioManager>();
        gm = FindObjectOfType<GameManager>();
        vfx = GetComponentInChildren<FXScript>();

        playerInput.Player.Enable();

        // Set up air jumps
        airJumpsLeft = maxAirJumps;
        airJumpCurrentCost = airJumpBaseCost;

        // Set up high/long jump speeds
        baseAirSpeed = airSpeed;
        baseGroundSpeed = groundSpeed;

        highJumpAirSpeed = airSpeed * highJumpSpeedMultiplier;
        highJumpGroundSpeed = groundSpeed * highJumpSpeedMultiplier;

        // Set up dashing
        dashCurrentCost = dashBaseCost;

        // Set up health
        healthBar.maxValue = healthBar.value = currentHealth = maxHealth;   
    }

    private void Update()
    {
        // Do nothing when game paused
        if (gm == null || gm.isPaused) { return; }

        // Apply small damage over time
        dealDamage(healthDecayRate * Time.deltaTime);

        // Save y velocity for gravity/jumping later
        float yVelocity = movementVector.y;

        // Apply gravity by default
        applyGravity = true;

        // Get WASD input
        Vector2 inputVector = playerInput.Player.Movement.ReadValue<Vector2>();
        movementVector = new Vector3(inputVector.x, 0, inputVector.y);

        // Handle dashing input
        if(playerInput.Player.Dash.WasPerformedThisFrame() && !jetpackExploded)
        {
            dashStarted = true;

            if (isGrounded)
            {
                vfx.GroundDash();
            }

            else
            {
                vfx.AirDash();
            }

            // Calculate dash target
            dashTarget = transform.position + transform.forward * dashDistance;

            // Deal dash damage
            dealDamage(dashCurrentCost);

            // Scale up dash damage
            dashCurrentCost *= dashCostMultiplier;
        }

        // Handle actual dash movement
        if(dashStarted)
        {
            // Calculate dash movement vector
            Vector3 dashVector = dashTarget - transform.position;

            // Move towards target if not too close and timer not exceeded
            if(dashVector.magnitude > DASH_MIN_DISTANCE && dashTimer < dashTimeLimit)
            {
                // Timer acts as failsafe in case we can't reach our target
                dashTimer += Time.deltaTime;

                // Move towards target
                dashVector = dashSpeed * Time.deltaTime * dashVector.normalized;
                controller.Move(dashVector);
                if (!dashSFX)
                {
                    dashSFX = true;
                }
                    // Apply no other movement while dashing
                return;
            }

            else
            {
                // Stop dashing and reset timer
                dashTimer = 0;
                dashStarted = false;
                dashSFX = false;
            }
        }

        // Handle high jumps
        if (playerInput.Player.HighJump.IsInProgress() && airJumpsLeft > 0 && !jetpackExploded)
        {
            vfx.ChargeHighJump();
            highJumpTimer += Time.deltaTime;

            // Apply speed penalty while charging
            airSpeed = highJumpAirSpeed;
            groundSpeed = highJumpGroundSpeed;

            // Jump when charged
            if (highJumpTimer >= highJumpChargeTime)
            {
                am.stop("Scrapper_Charging");
                vfx.ActivateHighJump();
                dealDamage(highJumpCost);

                //am.play("Fly_Short");

                movementVector.y = highJumpHeight;

                airJumpsLeft--;

                // Force airborne this frame to avoid resetting air jump counter
                isGrounded = false;
                coyoteTimer = maxCoyoteTime;

                // Reset timer
                highJumpTimer = 0;

                // Don't apply gravity this frame
                applyGravity = false;
            }
        }

        else
        {
            // Remove VFX
            vfx.CancelCharge();
           
      
            // Reset high jump effects if button not pressed
            highJumpTimer = 0;

            // Reset speeds
            airSpeed = baseAirSpeed;
            groundSpeed = baseGroundSpeed;
        }

        // Move if WASD pressed
        if (movementVector != Vector3.zero)
        {
            // Calculate angles
            float movementAngle = Mathf.Atan2(movementVector.x, movementVector.z) * Mathf.Rad2Deg + mainCamera.eulerAngles.y;
            float rotationAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, movementAngle, ref turnSmoothVelocity, rotationTime);

            // Apply rotation
            transform.rotation = Quaternion.Euler(0, rotationAngle, 0);

            // Apply movement
            WASDMovementVector = Quaternion.Euler(0, movementAngle, 0f) * Vector3.forward;
            controller.Move((isGrounded ? groundSpeed : airSpeed) * Time.deltaTime * WASDMovementVector.normalized);
            walkSFXTimer -= Time.deltaTime;
            if (isGrounded && walkSFXTimer <= 0f )
            {
                am.play("Walking");
                walkSFXTimer = .25f;
                
               
            }
            if (!isGrounded)
            {
                am.stop("Walking");
                walkSFXTimer = 0;
            }
        }
        else if (movementVector == Vector3.zero)
        {
            am.stop("Walking");
            walkSFXTimer = 0;

        }

        // Prevent repeated input polls by caching jump input
        jumpPressed = playerInput.Player.Jump.WasPerformedThisFrame();

        // Check for grounded jumps
        if (isGrounded || coyoteTimer < maxCoyoteTime)
        {
            // Handle long jumps
            if (playerInput.Player.LongJump.WasPerformedThisFrame() && !jetpackExploded)
            {
                vfx.LongJump();

               //am.play("Fly_Short");

                // Add forward momentum, reduced if player stationary
                forceVector += (movementVector.magnitude == 0) ? (transform.forward * longJumpDistance) * stationaryPenalty : transform.forward * longJumpDistance;

                movementVector.y = longJumpHeight;

                dealDamage(longJumpCost);

                // Stop getting coyote time
                coyoteTimer = maxCoyoteTime;

                // Don't apply gravity for this frame
                applyGravity = false;
            }

            // Check for jumping
            else if (jumpPressed)
            {
                movementVector.y = jumpForce;
                vfx.jumping = "Jump";
                vfx.JumpingSFX();
                // Stop getting coyote time
                coyoteTimer = maxCoyoteTime;

                // Don't apply gravity this frame
                applyGravity = false;
            }

            // Reset air jumps
            airJumpsLeft = maxAirJumps;
            airJumpCurrentCost = airJumpBaseCost;

            // Reset dashing
            dashCurrentCost = dashBaseCost;
        }

        // Check for double jumps only when airborne
        else if (jumpPressed && airJumpsLeft > 0 && !jetpackExploded)
        {
            if(airJumpsLeft > 1)
            {
                vfx.AirJump();
            }

            else
            {
                vfx.FinalAirJump();
            }
            
            movementVector.y = jumpForce;

            
           // am.fadeOut("Fly_Short", 1, 1);

            dealDamage(airJumpCurrentCost);

            // Scale up damage
            airJumpCurrentCost *= airJumpCostMultiplier;

            airJumpsLeft--;

            // Don't apply gravity this frame
            applyGravity = false;
        }

        // Check for hovering only when airborne
        else if(playerInput.Player.Hover.IsInProgress() && !jetpackExploded)
        {
            vfx.InitiateHover();

            // Deal hover damage, increases the longer the hover is used
            dealDamage((hoverBaseDamage + (hoverDamageTimer * hoverDamageRate)) * Time.deltaTime);

            hoverDamageTimer += Time.deltaTime;

            // Don't apply gravity while hovering
            applyGravity = false;
        }

        // Reset hover damage timer when hover key not pressed
        else
        {
            hoverDamageTimer = 0;

            // End VFX
            vfx.EndHover();
        }

        if (forceVector != Vector3.zero)
        {
            // Apply force
            controller.Move(forceVector * Time.deltaTime);

            // Slowly reduce force to 0
            forceVector = Vector3.Lerp(forceVector, Vector3.zero, forceDecayRate * Time.deltaTime);
        }

        if(applyGravity)
        {
            // Apply gravity
            movementVector.y += yVelocity + Physics.gravity.y * gravity * Time.deltaTime;

            // Make sure we don't exceed normal fall speed limit
            movementVector.y = (movementVector.y < maxFallSpeed) ? maxFallSpeed : movementVector.y;
        }

        // Apply air movement
        controller.Move(movementVector * Time.deltaTime);

        // Check if grounded, must be done after all CharacterController.Moves
        isGrounded = controller.isGrounded;
       
        // Reset lights and some VFX on landing
        if(isGrounded && !vfx.groundReset)
        {
            vfx.JumpingSFX();
            if (vfx.tempNozzleSmoke != null)
            {
                Destroy(vfx.tempNozzleSmoke);
            }

            if(currentHealth > 0)
            {
                vfx.ResetLights();
            }

            vfx.groundReset = true;
        }

        else if(!isGrounded && vfx.groundReset)
        {
            vfx.groundReset = false;
        }

        // Prevent negative y velocity from building up while character is grounded
        if(isGrounded)
        {
            movementVector.y = 0;

            // Reset coyote time
            coyoteTimer = 0;
        }

        else
        {
            // Increment coyote time
            coyoteTimer += Time.deltaTime;
        }
    }

    public void dealDamage(float damage)
    {
        // Don't bother taking damage if jetpack already exploded
        if(jetpackExploded) { return; }

        // Deal damage
        currentHealth -= damage;
        healthBar.value = currentHealth;
        if (currentHealth <= lowDamage && damageStage == 0)
        {
            
            //am.play("DamageLow");
            damageStage++;
        }
        if (currentHealth <= mediumDamage && damageStage == 1)
        {
            //am.play("DamageMedium");
            damageStage++;
        }
        if (currentHealth <= highDamage && damageStage == 2)
        {
            //am.play("DamageHigh");
            damageStage++;
        }
        // Play low health sound when below threshold
        if (currentHealth <= lowHealthWarning && !lowSFX)
        {
            am.play("Jetpack_Low");
            lowSFX = true;
        }

        // Explode when out of health
        if(currentHealth <= 0)
        {
            jetpackExploded = true;

            vfx.Explode();

            // Stop playing low health warning
            am.stop("Jetpack_Low");
            lowSFX = false;

            am.play("Explosion");

            // Calculate explosion vector
            forceVector += WASDMovementVector * forwardsForce;
            forceVector.y += upwardsForce;
        }
    }

    public void heal(float health)
    {
        vfx.Repair();
        vfx.ResetLights();

        // Don't heal above full
        currentHealth = Mathf.Min(currentHealth + health, maxHealth);
        healthBar.value = currentHealth;

        // Re-enable jetpack
        jetpackExploded = false;
       if(currentHealth > highDamage)
        {
            damageStage=2;
        }
       if(currentHealth > mediumDamage)
        {
            damageStage-=1;
        }
        if (currentHealth > lowDamage)
        {
            damageStage=0;
        }

        if(currentHealth > lowHealthWarning && lowSFX)
        {
            am.stop("Jetpack_Low");
            lowSFX = false;
        }
    }

    public PlayerControls getInput() { return playerInput; }
}