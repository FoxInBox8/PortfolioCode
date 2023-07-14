using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhaleStartTrigger : MonoBehaviour
{
    private WhaleBehavior parentWhale;

    private void Start()
    {
        parentWhale = transform.parent.GetComponent<WhaleBehavior>();
    }

    // When player leaves enters range, notify parent
    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
        {
            parentWhale.playerEnteredRange();
        }
    }
}