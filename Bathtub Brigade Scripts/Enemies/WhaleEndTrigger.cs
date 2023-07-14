using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhaleEndTrigger : MonoBehaviour
{
    private WhaleBehavior parentWhale;

    private void Start()
    {
        parentWhale = transform.parent.GetComponent<WhaleBehavior>();
    }

    // When player leaves chase range, notify parent
    private void OnTriggerExit(Collider other)
    {
        if(other.tag == "Player")
        {
            parentWhale.playerLeftRange();
        }
    }
}