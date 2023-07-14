using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamagedPlatform : MonoBehaviour
{
    [SerializeField]
    private float delayTime;

    [SerializeField]
    private GameObject breakParticles;
    private GameObject tempParticles;

    // Destroy self in delayTime seconds when player touches
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            Destroy(gameObject, delayTime);

            // Display particles
            tempParticles = Instantiate(breakParticles, transform.position, Quaternion.identity);
            // Particles spawn when platform breaks
            ParticleSystem.MainModule main = tempParticles.GetComponent<ParticleSystem>().main;
            main.startDelay = delayTime + 0.01f;
        }
    }
}