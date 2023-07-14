using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    [SerializeField]
    private float healthRestored;

    private PlayerScript player;

    private AudioManager am;

    private void Start()
    {
        player = FindObjectOfType<PlayerScript>();
        am = FindObjectOfType<AudioManager>();
    }

    // Heal player on collision
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            player.heal(healthRestored);
            am.playRandomPitch("Scraps", 0.5f, 2f);

            // Destroy self so can only be picked up once
            Destroy(gameObject);
        }
    }
}