using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DriverCannon : MonoBehaviour
{
    public GameObject projectile;
    public float power, destroyTime, reloadTime;
    
    [SerializeField]
    private float fireDelay;

    [SerializeField]
    private KeyCode shootKey1, shootKey2;

    [SerializeField]
    private UIManager.CannonSide cannonSide;

    [SerializeField]
    private ParticleSystem gunSmoke;

    [SerializeField]
    private FollowerCannon followerCannon;

    [SerializeField]
    private string shootSound;

    [SerializeField]
    private float minPitch, maxPitch;

    public float timer { get; private set; }

    private GameObject spawnedObject;
    private Transform spawnPoint;

    private void Awake()
    {
        // Start loaded
        timer = reloadTime;

        // Spawnpoint must always be first child of the object with the cannon script attatched
        spawnPoint = transform.GetChild(0);

        // Register to UI manager
        FindObjectOfType<UIManager>().registerCannon(this, cannonSide);
    }

    private void Update()
    {
        // Increase timer
        timer += Time.deltaTime;

        // Fire if loaded
        if ((Input.GetKeyDown(shootKey1) || Input.GetKeyDown(shootKey2)) && timer > reloadTime)
        {
            StartCoroutine(fireCannon());
            StartCoroutine(followerCannon.fireCannon());
        }
    }

    private IEnumerator fireCannon()
    {
        yield return new WaitForSeconds(fireDelay);

        // Reset timer
        timer = 0;

        // Calculate launch angle and force
        Ray ray = new Ray(transform.position, transform.right);

        // 7 is max rotation allowed (in radians) - 2*pi ~= 6.28, 7 so means we can always do a full rotation if necessary
        Vector3 launchForce = Vector3.RotateTowards(new Vector3(0, 0, power), ray.direction, 7, 0);

        // Spawn projectile
        spawnedObject = Instantiate(projectile);
        spawnedObject.transform.position = spawnPoint.position;
        spawnedObject.GetComponent<Rigidbody>().AddForce(launchForce);

        FindObjectOfType<AudioManager>().playRandomPitch(shootSound, minPitch, maxPitch);

        //Shoot particles
        gunSmoke.Play();

        // Destroy projectile
        Destroy(spawnedObject, destroyTime);
    }
}