using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PirateCannon : MonoBehaviour
{
    [SerializeField]
    private GameObject projectile;

    [SerializeField]
    private float power, destroyTime;

    [SerializeField]
    ParticleSystem gunSmoke;

    [SerializeField]
    private string shootSound;

    [SerializeField]
    private float minPitch, maxPitch;

    private int damage;
    private GameObject spawnedObject;
    private Transform spawnPoint;

    private void Awake()
    {
        // Spawnpoint must always be first child of the object with the cannon script attatched
        spawnPoint = transform.GetChild(0);

        damage = gameObject.GetComponentInParent<PirateBehavior>().getDamage();
    }

    public void fireCannon()
    {
        // Calculate launch angle and force
        Ray ray = new Ray(transform.position, transform.right);

        // 7 is max rotation allowed (in radians) - 2*pi ~= 6.28, 7 so means we can always do a full rotation if necessary
        Vector3 launchForce = Vector3.RotateTowards(new Vector3(0, 0, power), ray.direction, 7, 0);

        // Spawn projectile
        spawnedObject = Instantiate(projectile);
        spawnedObject.transform.position = spawnPoint.position;
        spawnedObject.GetComponent<Rigidbody>().AddForce(launchForce);

        // Set damage
        spawnedObject.GetComponent<PirateCannonball>().damage = damage;

        FindObjectOfType<AudioManager>().playRandomPitch(shootSound, minPitch, maxPitch);

        //Shoot particles
        gunSmoke.Play();

        // Destroy projectile
        Destroy(spawnedObject, destroyTime);
    }
}