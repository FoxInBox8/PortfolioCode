using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinkingPlatform : MonoBehaviour
{
    private enum MovementState
    {
        NONE = 0,
        SINKING,
        RISING
    }

    [SerializeField]
    private float sinkSpeed, riseSpeed;

    private Vector3 returnPoint;
    private MovementState state = MovementState.NONE;

    private void Start()
    {
        returnPoint = transform.position;
    }

    private void Update()
    {
        switch(state)
        {
            // Move downwards while sinking
            case MovementState.SINKING:
                transform.position = Vector3.MoveTowards(transform.position, transform.position - transform.up, sinkSpeed * Time.deltaTime);
                break;

            // Move upwards while rising
            case MovementState.RISING:
                transform.position = Vector3.MoveTowards(transform.position, returnPoint, riseSpeed * Time.deltaTime);

                // Stop moving once we reach return point
                if(transform.position == returnPoint)
                {
                    state = MovementState.NONE;
                }

                break;
        }
    }

    // Start sinking when player lands on top of platform
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            state = MovementState.SINKING;

            // Parent player so they sink with platform
            other.transform.SetParent(transform);
        }
    }

    // Start rising when player jumps off platform
    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            state = MovementState.RISING;

            // Un-parent player
            other.transform.SetParent(null);
        }
    }
}