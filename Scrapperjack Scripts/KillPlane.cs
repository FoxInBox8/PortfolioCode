using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KillPlane : MonoBehaviour
{
    private GameManager gm;
    private AudioManager am;
    private bool deathSound = false;
    private void Start()
    {
        gm = FindObjectOfType<GameManager>();
        am = FindObjectOfType<AudioManager>();
    }

    // Lose when player touches kill plane
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            if(!deathSound)
            {
                deathSound = true;
                am.play("Scrapper_Fall_Death");
            }
            gm.playerLost();
        }
    }
}