using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PirateTrigger : MonoBehaviour
{
    private PirateBehavior parent;

    private void Start()
    {
        parent = transform.parent.GetComponent<PirateBehavior>();
    }

    // Toggle chasing when player enters or exits trigger
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            parent.playerEnteredRange();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.tag == "Player")
        {
            parent.playerLeftRange();
        }
    }
}