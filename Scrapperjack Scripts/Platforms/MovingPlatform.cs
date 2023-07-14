using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed;

    [SerializeField]
    private Vector3[] moveLocations;

    private int currentTarget = 0;
    private bool hasYMovement = false;

    private void Start()
    {
        // Check if the platform will ever move vertically
        float tempY = moveLocations[0].y;

        // If one of the move targets has a different y value, then the platform will move vertically at some point
        foreach(Vector3 location in moveLocations)
        {
            if(tempY != location.y)
            {
                hasYMovement = true;
            }
        }
    }

    private void Update()
    {
        // If platform will move vertically, use target y value, otherwise just use current y, so that script plays nice with sinking platforms
        Vector3 moveTarget = hasYMovement ? moveLocations[currentTarget] : new Vector3(moveLocations[currentTarget].x, transform.position.y, moveLocations[currentTarget].z);

        transform.position = Vector3.MoveTowards(transform.position, moveTarget, moveSpeed * Time.deltaTime);

        // Move to next target when we reach current target
        if (transform.position == moveTarget)
        {
            currentTarget = (currentTarget + 1) % moveLocations.Length;
        }
    }

    // Parent player when they jump on top of platform so that they move with platform
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            other.transform.SetParent(transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            other.transform.SetParent(null);
        }
    }
}