using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EscapePod : MonoBehaviour
{
    private GameManager gm;
    private MusicManager music;
    private AudioManager am;
   
    private void Start()
    {
        gm = FindObjectOfType<GameManager>();
        music = FindObjectOfType<MusicManager>();
        am = FindObjectOfType<AudioManager>();
    }

    // Notify game manager when player touches
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            gm.playerWon();
           // music.win();
            am.play("Victory");

        }
    }
}