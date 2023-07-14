using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PirateCannonball : MonoBehaviour
{
    [SerializeField]
    private float splashVal;

    [Space(10)]

    [SerializeField]
    private string splashSound;

    [SerializeField]
    private float minPitch, maxPitch;

    [SerializeField]
    private ParticleSystem splashParticle;

    private TrailRenderer trailRenderer;

    [HideInInspector]
    public int damage;

    private bool splashed = false;

    private void Start()
    {
        trailRenderer = gameObject.GetComponent<TrailRenderer>();
    }

    //TODO - refactor to use inheritance with other cannonball
    private void Update()
    {
        // TODO - make this happen when colliding with water, rather than just checking y value
        if (!splashed && transform.position.y <= splashVal)
        {
            trailRenderer.emitting = false;
            ParticleSystem tempSplashEffect = Instantiate(splashParticle, gameObject.transform.position, Quaternion.Euler(0f, 0f, 0f));
            FindObjectOfType<AudioManager>().playRandomPitch(splashSound, minPitch, maxPitch);
            splashed = true;
        }
    }

    // Damage player
    private void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.tag == "Player")
        {
            other.gameObject.GetComponent<PlayerScript>().dealDamage(damage);
        }
    }
}