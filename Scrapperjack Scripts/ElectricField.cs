using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricField : MonoBehaviour
{
    [SerializeField]
    private float damage;

    private PlayerScript player;
    private AudioManager am;

    private void Start()
    {
        player = FindObjectOfType<PlayerScript>();
        am = FindObjectOfType<AudioManager>();
    }

    // Damage player on collision
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            player.dealDamage(damage);
            am.play("Zap");
        }
    }
}