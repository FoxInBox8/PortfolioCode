using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhaleBehavior : EnemyBase
{
    public enum WhaleState
    {
        INVALID_STATE = -1,
        PATROL = 0,
        CHASE,
        DIVE,
        SURFACE
    }

    public WhaleState currentState { get; private set; } = WhaleState.PATROL;

    [SerializeField]
    private float diveSpeed, diveDepth, diveDistance, underwaterDamageMultiplier, chaseTimer, followThroughTimer;

    [SerializeField]
    private List<Vector3> moveLocations;

    [SerializeField]
    private Vector3 diveTarget, surfaceTarget;

    [SerializeField]
    private string whaleDamagedSound, whaleDeathSound, whaleDiveSound, whaleSurfaceSound;

    [SerializeField]
    private float minDamagedPitch, maxDamagedPitch, minDeathPitch, maxDeathPitch, minDivePitch, maxDivePitch, minSurfacePitch, maxSurfacePitch;

    [SerializeField]
    private GameObject wakeHolder;

    private int currentMoveTarget = 0;
    private float origY, timer = 0;
    private Transform player;

    protected override void Start()
    {
        base.Start();

        // Save original y position so we can return to it after diving
        origY = transform.position.y;

        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update base
    protected override void Update()
    {
        base.Update();
    }

    private void FixedUpdate()
    {
        if (!isEnemyDead)
        {
            switch (currentState)
            {
                case WhaleState.PATROL:
                    // Move towards current target if not already there
                    if (transform.position != moveLocations[currentMoveTarget])
                    {
                        enemyMove(moveLocations[currentMoveTarget]);

                        // Fuck rigidbodies, need this to keep Y value even though enemyMove SHOULD do just that
                        transform.position = new Vector3(transform.position.x, origY, transform.position.z);
                    }

                    // Get new target
                    else
                    {
                        // Go to the next element of the array, looping back at the end
                        currentMoveTarget = (currentMoveTarget + 1) % moveLocations.Count;
                    }

                    break;

                case WhaleState.CHASE:

                    // Chase player
                    enemyMove(player.position);

                    timer += Time.deltaTime;

                    // Stop chasing if we chase for too long
                    if (timer >= chaseTimer)
                    {
                        changeState(WhaleState.DIVE);
                        timer = 0;
                    }

                    break;

                case WhaleState.DIVE:

                    // Dive away
                    verticalMove(diveTarget);

                    // If at dive target, begin surfacing
                    if (transform.position == diveTarget)
                    {
                        changeState(WhaleState.SURFACE);
                    }

                    break;

                case WhaleState.SURFACE:

                    // Surface
                    verticalMove(surfaceTarget);

                    // If at surface, resume patrolling
                    if (transform.position == surfaceTarget)
                    {
                        changeState(WhaleState.PATROL);
                        FindObjectOfType<AudioManager>().playRandomPitch(whaleSurfaceSound, minSurfacePitch, maxSurfacePitch);
                    }

                    break;
            }
        }
    }

    private void changeState(WhaleState newState)
    {
        if(isEnemyDead)
        {
            return;
        }

        currentState = newState;

        switch(newState)
        {
            case WhaleState.PATROL:

                // Enable wake
                wakeHolder.SetActive(true);

                break;

            case WhaleState.CHASE:

                // Enable wake
                wakeHolder.SetActive(true);

                break;

            case WhaleState.DIVE:
                FindObjectOfType<AudioManager>().playRandomPitch(whaleDiveSound, minDivePitch, maxDivePitch);

                // Disable wake
                wakeHolder.SetActive(false);

                // Reset dive timer
                timer = 0;

                // Calculate dive target
                Vector3 d1 = transform.position - player.position;
                Vector3 d2 = transform.position + (d1.normalized * diveDistance);
                diveTarget = new Vector3(d2.x, origY + diveDepth, d2.z);

                break;

            case WhaleState.SURFACE:

                // Calculate surface target
                Vector3 d3 = transform.position + (diveTarget.normalized * diveDistance);
                surfaceTarget = new Vector3(d3.x, origY, d3.z);

                break;
        }
    }


    //TODO - move to EnemyBase
    private void verticalMove(Vector3 target)
    {
        // Move towards position
        transform.position = Vector3.MoveTowards(transform.position, target, diveSpeed * Time.deltaTime);

        // Prevents weird error message
        if(target - transform.position != Vector3.zero)
        {
            // Rotate to face
            Quaternion newRotation = Quaternion.LookRotation(target - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, rotationSpeed * Time.deltaTime);
        }
    }

    public override void dealDamage(int damage)
    {
        // Take less damage when underwater
        if(currentState == WhaleState.DIVE || currentState == WhaleState.SURFACE)
        {
            damage = (int)Mathf.Floor(damage * underwaterDamageMultiplier);
        }

        currentHealth -= damage;

        FindObjectOfType<AudioManager>().playRandomPitch(whaleDamagedSound, minDamagedPitch, maxDamagedPitch);

        // Die when out of health
        //TODO - move to overide die()
        if (currentHealth <= 0)
        {
            FindObjectOfType<AudioManager>().playRandomPitch(whaleDeathSound, minDeathPitch, maxDeathPitch);
            base.die();
        }
    }

    // Begin diving after hitting player
    protected override void OnCollisionEnter(Collision other)
    {
        base.OnCollisionEnter(other);

        if (other.gameObject.tag == "Player")
        {
            StartCoroutine(BeginDiveAfterDelay());
        }
    }

    // Used by child triggers to notify when player leaves or enters chase range
    public void playerEnteredRange()
    {
        if(currentState == WhaleState.PATROL)
        {
            changeState(WhaleState.CHASE);
        }
    }

    public void playerLeftRange()
    {
        if (currentState == WhaleState.CHASE)
        {
            changeState(WhaleState.PATROL);
        }
    }

    // Delay before diving because designers said so
    private IEnumerator BeginDiveAfterDelay()
    {
        yield return new WaitForSeconds(followThroughTimer);

        changeState(WhaleState.DIVE);
    }
}