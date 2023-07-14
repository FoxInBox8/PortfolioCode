using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricPlatforms : MonoBehaviour
{
    [SerializeField]
    private float damage, initialDamageDelay, damageInterval;

    private float damageTimer;
    private bool touchingPlayer = false, initialDamageDealt = false;

    private PlayerScript player;

    private AudioManager am; 

    private void Start()
    {
        // Start able to damage
        damageTimer = damageInterval;

        player = FindObjectOfType<PlayerScript>();
        am = FindObjectOfType<AudioManager>();
    }

    private void Update()
    {
        // Do nothing if not touching player
        if (!touchingPlayer) { return; }

        damageTimer += Time.deltaTime;

        // Initial damage is dealt faster than normal interval, so use different timer
        if(!initialDamageDealt && damageTimer >= initialDamageDelay)
        {
            player.dealDamage(damage);
            am.play("Zap");

            // Use normal timer
            initialDamageDealt = true;

            // Reset timer
            damageTimer = 0;
        }

        else if(damageTimer >= damageInterval)
        {
            player.dealDamage(damage);
            am.play("Zap");

            // Reset timer
            damageTimer = 0;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            touchingPlayer = true;
        }
    }

    // Stop timer when jumps off platform
    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            touchingPlayer = false;

            // Reset timer
            damageTimer = 0;

            // Use initial damage timer
            initialDamageDealt = false;
        }
    }
}