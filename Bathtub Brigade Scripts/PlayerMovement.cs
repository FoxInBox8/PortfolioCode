using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private const float MAX_BOOST_SPEED_MARKIPLIER = 1.5f;

    [SerializeField]
    private float accelerationFactor, brakingFactor, turnSpeedDampening, maxSpeed, 
                  maxBoostSpeed, boostFactor, boostUseSpeed, boostReloadSpeed, maxBoostStamina, previousStamina;

    public float turningFactor;

    [SerializeField]
    private string collisionSound, BoostEmpty, BoostFull, movementSound;

    [SerializeField]
    private float minPitch, maxPitch, boostMinPitch, boostMaxPitch, maxVolume;

    [SerializeField]
    private ParticleSystem smokeStackParticles, boostParticles;

    public float boostStamina { get; private set; }
    public bool boostExaustion {get; private set;} = false;

    private bool playMovementSound = true;
    private float accelerate, decelerate, turning, boost;

    private Rigidbody rb;
    private Transform playerTransform;
    private AudioManager audioPlayer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerTransform = GetComponent<Transform>();
        audioPlayer = FindObjectOfType<AudioManager>();

        boostStamina = maxBoostStamina;
        previousStamina = boostStamina;

        maxBoostSpeed = maxSpeed * MAX_BOOST_SPEED_MARKIPLIER;
    }

    private void Update()
    {
        // Check for key input
        accelerate = accelerationFactor * (Input.GetKey(KeyCode.W) ? 1 : 0);
        decelerate = brakingFactor * (Input.GetKey(KeyCode.S) ? 1 : 0);
        turning = Input.GetAxis("Horizontal") * turningFactor;
        boost = boostFactor * (((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.Space)) && boostStamina > 0) ? 1 : 0);

        // Smokestack particles
        // Use boost particles when boosting and normal ones when not
        if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.Space)) && boostStamina > 0 && !boostExaustion)
        {
            ParticleSystem.EmissionModule boostParticleEmission = boostParticles.emission;
            boostParticleEmission.enabled = true;

            ParticleSystem.EmissionModule smokeStackParticleEmission = smokeStackParticles.emission;
            smokeStackParticleEmission.enabled = false;
        }

        else
        {
            ParticleSystem.EmissionModule boostParticleEmission = boostParticles.emission;
            boostParticleEmission.enabled = false;

            ParticleSystem.EmissionModule smokeStackParticleEmission = smokeStackParticles.emission;
            smokeStackParticleEmission.enabled = true;
        }
    }

    private void FixedUpdate() {
        // Rotate cart to face forward
        // Cacluate the linear decay of turning speed based on velocity

        // Calculate turning power (don't ask)
        float turningEffect = -(turnSpeedDampening / (boost == 0 ? maxSpeed : maxBoostSpeed)) * rb.velocity.magnitude + 1f;

        // Rotate the player
        playerTransform.Rotate(Vector3.up * turning * turningEffect);

        // Make sure no weird rigidbody things happen
        rb.angularVelocity = Vector3.zero;

        // Accelerate the cart
        rb.AddForce(playerTransform.forward * accelerate);

        // Play sound when moving
        if (accelerate > 0 || (boost > 0 && !boostExaustion)) {
            if (playMovementSound) {
                audioPlayer.playLoop(movementSound, 0f);
                playMovementSound = false;
            }

            audioPlayer.fadeIn(movementSound, maxVolume, 0.5f);
        } else {
            audioPlayer.fadeOut(movementSound, maxVolume, 5f);
            playMovementSound = true;
        }
        audioPlayer.setPitch(movementSound, Mathf.Lerp(minPitch, maxPitch, rb.velocity.magnitude / maxSpeed));

        // Brakes
        rb.AddForce(-rb.velocity * decelerate);

        // Speed boost
        // When boost is active
        if (boost > 0 && !boostExaustion) {
            // Use stamina
            previousStamina = boostStamina;
            boostStamina -= Time.deltaTime * boostUseSpeed;

            // Boost player
            rb.AddForce(playerTransform.forward * boostFactor);

            // Cause exaustion on empty stamina
            if (boostStamina <= 0) {
                boostStamina = 0;
                boostExaustion = true;
                audioPlayer.playRandomPitch(BoostEmpty, boostMinPitch, boostMaxPitch);
            }
        } else {
            // Refill stamina
            boostStamina += (boostStamina < maxBoostStamina) ? Time.deltaTime * boostReloadSpeed : 0;

            // Remove exaustion on full stamina
            if (boostExaustion && boostStamina >= maxBoostStamina) {
                boostExaustion = false;
            }
        }
        
        // Play boost refill sound once
        if (boostStamina >= maxBoostStamina && previousStamina != maxBoostStamina)
        {
            previousStamina = maxBoostStamina;
            audioPlayer.playRandomPitch(BoostFull, boostMinPitch, boostMaxPitch);
        }

        // Make the cart do the turning good (finally, holy cow that was painful)
        rb.velocity = playerTransform.forward * rb.velocity.magnitude * Vector3.Dot(playerTransform.forward, rb.velocity.normalized);

        // No go too speedy
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, boost == 0 ? maxSpeed : maxBoostSpeed);
    }

    // Play collision sound when hitting geometry
    private void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.tag != "WaterSurface" && other.gameObject.tag != "Projectile")
        {
            audioPlayer.playRandomPitch(collisionSound, minPitch, maxPitch);
        }
    }

    public void updateMaxSpeed(float newSpeed) {
        maxSpeed = newSpeed;
        maxBoostSpeed = maxSpeed * MAX_BOOST_SPEED_MARKIPLIER;
    }
}