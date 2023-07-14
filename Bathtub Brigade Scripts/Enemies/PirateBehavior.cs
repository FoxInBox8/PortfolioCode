using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PirateBehavior : EnemyBase
{
    [SerializeField]
    private float moveRadius, followDistance, reloadTime, resetTime;

    [SerializeField]
    private bool moveClockwise;

    [SerializeField]
    private string pirateDeathSound;

    [SerializeField]
    private float minDeathPitch, maxDeathPitch;

    private bool playerInRange = false, leftSide, hitProp = false, canFreeSelf = false, coroutineStarted = false;
    private int angle = 0;
    private float timer, origY;
    private Vector3 circleTarget, center, followTarget;
    private PlayerScript player;
    private PirateCannon leftCannon, rightCannon;

    protected override void Start()
    {
        base.Start();

        // Start loaded
        timer = reloadTime;

        // Calculate initial move target
        center = transform.position;
        circleTarget = calcPointOnCircle();

        // Get player
        player = FindObjectOfType<PlayerScript>();

        // Get cannons
        // Left cannon must be first child, right must be second
        leftCannon = transform.GetChild(0).GetComponent<PirateCannon>();
        rightCannon = transform.GetChild(1).GetComponent<PirateCannon>();

        origY = transform.position.y;
    }

    protected override void Update()
    {
        if(!isEnemyDead)
        {
            base.Update();
        }   
    }

    private void FixedUpdate()
    {
        if(isEnemyDead)
        {
            return;
        }

        timer += Time.fixedDeltaTime;

        // On hitting a prop, stop, start process of freeing self
        if(hitProp)
        {
            // Only start once
            if(!coroutineStarted)
            {
                StartCoroutine(freeSelf());
                coroutineStarted = true;
            }

            return;
        }

        // Start freeing self
        if (canFreeSelf)
        {
            // Return to previous target
            if (!Mathf.Approximately(transform.position.x, circleTarget.x) && !Mathf.Approximately(transform.position.z, circleTarget.z))
            {
                fixedEnemyMove(circleTarget);
            }

            // Resume normal movement upon reaching target
            else
            {
                canFreeSelf = false;
            }

            return;
        }

        // TODO - make this not jittery
        // Move towards target if not there
        if (playerInRange)
        {
            // Calculate point next to player
            followTarget = player.transform.position - player.transform.right * followDistance * (leftSide ? 1 : -1);

            fixedEnemyMove(followTarget);

            // Shoot if able
            if (timer > reloadTime)
            {
                // Reset timer
                timer = 0;

                //Fire appropriate cannon
                if (transform.InverseTransformPoint(player.transform.position).x > 0)
                {
                    rightCannon.fireCannon();
                }

                else
                {
                    leftCannon.fireCannon();
                }
            }
        }

        else
        {
            // Move towards target if not there
            if (Vector3.Distance(transform.position, circleTarget) > 1f && !hitProp)
            {
                fixedEnemyMove(circleTarget);

                // Fuck rigidbodies, need this to keep Y value even though enemyMove SHOULD do just that
                transform.position = new Vector3(transform.position.x, origY, transform.position.z);
            }

            // Calculate new point on circle
            else
            {
                angle = moveClockwise ? angle - 1 : angle + 1;
                circleTarget = calcPointOnCircle();
            }
        }
    }

    // Calculate point along edge of circle which we will move towards
    private Vector3 calcPointOnCircle()
    {
        float targetX = center.x + moveRadius * Mathf.Cos(angle * Mathf.Deg2Rad),
              targetZ = center.z + moveRadius * Mathf.Sin(angle * Mathf.Deg2Rad);

        return new Vector3(targetX, transform.position.y, targetZ);
    }

    // Used by child triggers to notify parent
    public void playerEnteredRange()
    {
        if(!isEnemyDead)
        {
            playerInRange = true;

            // Determine if player is on left or right side
            leftSide = player.transform.InverseTransformPoint(transform.position).x < 0;
        }      
    }

    public void playerLeftRange()
    {
        if(!isEnemyDead)
        {
            playerInRange = false;

            // Re-calculate move circle
            angle = (int)(Mathf.Atan2(transform.forward.x, transform.forward.z) * Mathf.Rad2Deg);
            center = transform.position;
            circleTarget = calcPointOnCircle();
        }     
    }

    protected override void OnCollisionEnter(Collision other)
    {
        base.OnCollisionEnter(other);

        if(other.gameObject.tag == "Environment")
        {
            hitProp = true;
        }
    }

    // Wait a few seconds, then start moving again
    private IEnumerator freeSelf()
    {
        yield return new WaitForSeconds(resetTime);

        coroutineStarted = hitProp = false;
        canFreeSelf = true;
    }

    // Used by cannons to determine damage
    public int getDamage() { return attackDamage; }

    protected override void die()
    {
        FindObjectOfType<AudioManager>().playRandomPitch(pirateDeathSound, minDeathPitch, maxDeathPitch);

        base.die();
    }
}