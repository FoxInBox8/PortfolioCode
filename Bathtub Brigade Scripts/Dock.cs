using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dock : MonoBehaviour
{
    private UIManager UI;

    private void Start()
    {
        UI = FindObjectOfType<UIManager>();
    }

    // Notify UI manager when player enters/leaves upgrade dock range
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            UI.togglePlayerUpgrade();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.tag == "Player")
        {
            UI.togglePlayerUpgrade();
        }
    }
}