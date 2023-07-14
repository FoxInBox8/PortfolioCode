using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [SerializeField]
    protected int startHealth, attackDamage;

    [SerializeField]
    protected float moveSpeed, rotationSpeed;

    [SerializeField]
    protected ParticleSystem damageParticlePrefab;

    protected int currentHealth;
    protected bool touchingPlayer = false, isEnemyDead = false;

    [SerializeField]
    protected GameObject spawnObject;

    [SerializeField]
    protected int pickupValue;

    [SerializeField]
    protected GameObject deathDummyPrefab;

    private const int HEALTH_PICKUP_VALUE = 15;

    protected virtual void Start()
    {
        // Start at max health
        currentHealth = startHealth;
    }

    protected virtual void Update()
    {
        // Damage player on contact, player has i-frames, so don't worry about update frequency
        if (touchingPlayer)
        {
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>().dealDamage(attackDamage);
        }
    }

    protected virtual void enemyMove(Vector3 target)
    {
        // TODO - switch to NavMeshAgent

        // Enemies often have weird y positions to get model over water, save it here
        target.y = gameObject.transform.position.y;

        // Prevents weird error message
        if (target - transform.position != Vector3.zero)
        {
            // Rotate to face
            Quaternion newRotation = Quaternion.LookRotation(target - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, rotationSpeed * Time.deltaTime);
        }

        // Move towards position
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
    }

    protected virtual void fixedEnemyMove(Vector3 target)
    {
        // Enemies often have weird y positions to get model over water, save it here
        target.y = gameObject.transform.position.y;

        // Prevents weird error message
        if (target - transform.position != Vector3.zero)
        {
            // Rotate to face
            Quaternion newRotation = Quaternion.LookRotation(target - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, rotationSpeed * Time.fixedDeltaTime);
        }

        // Move towards position
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.fixedDeltaTime);
    }

    protected virtual void OnCollisionEnter(Collision other)
    {
        // Lose health when hit by projectile
        if (other.gameObject.tag == "Projectile")
        {
            // Spawn damage particles
            Vector3 hitLocation = other.GetContact(0).point;

            if (damageParticlePrefab != null)
            {
                ParticleSystem damageParticles = Instantiate(damageParticlePrefab, hitLocation, Quaternion.identity);
            }

            // Take damage
            dealDamage(other.gameObject.GetComponent<Cannonball>().damage);

            // Destroy projectile
            Destroy(other.gameObject);
        }

        else if (other.gameObject.tag == "Player")
        {
            touchingPlayer = true;
        }
    }

    protected virtual void OnCollisionExit(Collision other)
    {
        if (other.gameObject.tag == "Player")
        {
            touchingPlayer = false;
        }
    }

    // Take damage
    public virtual void dealDamage(int damage)
    {
        currentHealth -= damage;

        // Die when out of health
        if (currentHealth <= 0)
        {
            die();
        }
    }

    // Overridden by inheriting classes for custom death actions
    protected virtual void die() {
        // Death animation nonsense
        isEnemyDead = true;
        GameObject tempDummy = Instantiate(deathDummyPrefab);
        gameObject.transform.SetParent(tempDummy.transform);
        Destroy(gameObject, tempDummy.GetComponent<Animation>().clip.length);

        if (spawnObject != null)
        {
            // Spawn pickup
            GameObject spawnedObject = Instantiate(spawnObject, new Vector3(transform.position.x, 0, transform.position.z), transform.rotation);
            spawnedObject.GetComponent<Pickups>().value = pickupValue;

            // Determine whether to spawn health or treasure pickup
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            // More likely to spawn health as player health decreases
            if (Random.Range(0f, 0.999999f) < 1 - (player.GetComponent<PlayerScript>().currentHealth / player.GetComponent<PlayerScript>().startHealth))
            {
                spawnedObject.GetComponent<Pickups>().type = Pickups.PickupType.HEALTH;
                spawnedObject.GetComponent<Pickups>().value = HEALTH_PICKUP_VALUE;
            }
            else
            {
                spawnedObject.GetComponent<Pickups>().type = Pickups.PickupType.TREASURE;
            }
        }
    }
}