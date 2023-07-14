using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuckTrigger : MonoBehaviour
{
    private DuckBehavior parentDuck;

    private void Start()
    {
        parentDuck = transform.parent.GetComponent<DuckBehavior>();
    }

    // Toggle chasing when player enters or exits trigger
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            parentDuck.togglePlayerInRange();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            parentDuck.togglePlayerInRange();
        }
    }
}