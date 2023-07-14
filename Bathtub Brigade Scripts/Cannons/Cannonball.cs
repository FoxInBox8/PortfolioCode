using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannonball : MonoBehaviour
{
    public int damage;

    [SerializeField]
    private float splashVal;

    [SerializeField]
    private string splashSound, hitSound;

    [SerializeField]
    private float minPitch, maxPitch, minHitPitch, maxHitPitch;

    [SerializeField]
    private ParticleSystem splashParticle;

    private TrailRenderer trailRenderer;

    private bool splashed = false;

    private void Start()
    {
        trailRenderer = gameObject.GetComponent<TrailRenderer>();
    }

    private void Update()
    {
        // TODO - make this happen when colliding with water, rather than just checking y value
        if(!splashed && transform.position.y <= splashVal)
        {
            // Play splash particles
            trailRenderer.emitting = false;
            ParticleSystem tempSplashEffect = Instantiate(splashParticle, gameObject.transform.position, Quaternion.Euler(0f,0f,0f));
            FindObjectOfType<AudioManager>().playRandomPitch(splashSound, minPitch, maxPitch);
            splashed = true;
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.tag != "WaterSurface")
        {
            FindObjectOfType<AudioManager>().playRandomPitch(hitSound, minHitPitch, maxHitPitch);
        }
    }
}