using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NecromancerScript : MonoBehaviour
{
    public Animator animator;
    public ParticleSystem revive1;                              // Particle for when a player starts to get revived - Talyn
    public ParticleSystem revive2;                              // Particle for when a player starts to get revived - Talyn
    bool shouldSpin = false;
    int spinCounter = 0;

    // Start is called before the first frame update
    void Start()
    {
        transform.rotation = Quaternion.Euler(0, 0, 0);


        //animator.Play("Base Layer.Idle", 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if(shouldSpin)
        {
            spinCounter++;
            if(spinCounter == 15)
            {
                transform.rotation = Quaternion.Euler(0, 180, 0);
                shouldSpin = false;
                animator.Play("Base Layer.Idle", 0, 0);
            }
        }
    }

    public void ReviveLeft()
    {
    	animator.Play("Base Layer.ReviveLeft", 0, 0);
        revive1.Play(true);
    }
    
    public void ReviveRight()
    {
    	animator.Play("Base Layer.ReviveRight", 0, 0);
        revive2.Play(true);
    }

    public void CorrectRotation()
    {
        shouldSpin = true;

    }

}
