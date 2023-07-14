using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public List<int> collectables { get; private set; } = new List<int>();

    public int currentHealth { get; private set; }

    [SerializeField]
    private string damagedSound;

    [SerializeField]
    private float minPitch, maxPitch;

    public int startHealth = 100;

    [SerializeField]
    private float invincibilityDuration;

    [SerializeField]
    private ParticleSystem damageParticlePrefab;

    private float invincibilityTimer;

    private bool playSound = true;

    private void Awake() {
        // Start at max health
        currentHealth = startHealth;

        // Start able to take damage
        invincibilityTimer = invincibilityDuration;

        // Populate collectables list with 0s
        for (int i = 0; i < Pickups.NUM_PICKUP_TYPES; ++i)
        {
            collectables.Add(0);
        }
    }

    private void Update() {
        // Update Health
        invincibilityTimer += Time.deltaTime;

        // Get list of all whirlpools
        // TODO - fix error thrown when scene has no whirlpools
        GameObject[] whirlpools = GameObject.FindGameObjectsWithTag("Whirlpool");
        Whirlpool whirlpoolReference = whirlpools[0].GetComponent<Whirlpool>();

        // Start the sound
        if (playSound)
        {
            FindObjectOfType<AudioManager>().playLoop(whirlpoolReference.whirlpoolSound, 0f);
            playSound = false;
        }

        // Find whirlpool closest to player
        GameObject closest = whirlpools[0];
        for (int i = 0; i < whirlpools.Length; ++i) {
            float newDist = Vector3.Distance(transform.position, whirlpools[i].transform.position);
            float closestDist = Vector3.Distance(transform.position, closest.transform.position);
            if (newDist < closestDist) {
                closest = whirlpools[i];
            }
        }

        // Fade sound out as distance to player increases
        float distToPlayer = Vector3.Distance(transform.position, closest.transform.position);
        float newVolume = Mathf.Max((whirlpoolReference.maxDistance - distToPlayer) / whirlpoolReference.maxDistance, 0);
        FindObjectOfType<AudioManager>().setVolume(whirlpoolReference.whirlpoolSound, newVolume);
    }

    public void collect(Pickups.PickupType type, int amount) {
        // Update the value of the corresponding collectable
        if (type == Pickups.PickupType.HEALTH)
        {
            heal(amount);
        } else
        {
            collectables[(int)type] += amount;
        }
    }

    public void dealDamage(int damage) {
        // If able to take damage
        if (invincibilityTimer > invincibilityDuration) {
            // Reset timer
            invincibilityTimer = 0;

            currentHealth -= damage;
            
            // Play damage sound
            FindObjectOfType<AudioManager>().playRandomPitch(damagedSound, minPitch, maxPitch);

            // Spawn particles
            ParticleSystem damageParticles = Instantiate(damageParticlePrefab,gameObject.transform);
        }
    }

    public void heal(int toHeal) {
        // never go above starting health
        currentHealth = Mathf.Min(currentHealth + toHeal, startHealth);
    }
}