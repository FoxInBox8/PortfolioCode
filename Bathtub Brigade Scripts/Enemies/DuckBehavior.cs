using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuckBehavior : EnemyBase
{
    [SerializeField]
    private string duckDamagedSound, duckDeathSound;

    [SerializeField]
    private float minPitch, maxPitch, minDeathPitch, maxDeathPitch;

    private GameObject player;
    private Vector3 returnPoint;
    private bool playerInRange = false;

    protected override void Start()
    {
        base.Start();

        // Initialize
        player = GameObject.FindGameObjectWithTag("Player");
        returnPoint = transform.position;
    }

    // Update base
    protected override void Update()
    {
        base.Update();
    }

    private void FixedUpdate()
    {
        // Move towards player if they are in range, otherwise return home
        if (playerInRange && !isEnemyDead)
        {
            enemyMove(player.transform.position);
        }

        else if(!isEnemyDead)
        {
            enemyMove(returnPoint);
        }
    }

    // Take damage
    public override void dealDamage(int damage)
    {
        currentHealth -= damage;

        FindObjectOfType<AudioManager>().playRandomPitch(duckDamagedSound, minPitch, maxPitch);

        // Die when out of health
        if (currentHealth <= 0 && !isEnemyDead)
        {
            die();
        }
    }

    protected override void die() {
        FindObjectOfType<AudioManager>().playRandomPitch(duckDeathSound, minDeathPitch, maxDeathPitch);

        base.die();
    }

    // Used by child trigger to toggle chasing
    public void togglePlayerInRange()
    {
        playerInRange = !playerInRange;
    }
}