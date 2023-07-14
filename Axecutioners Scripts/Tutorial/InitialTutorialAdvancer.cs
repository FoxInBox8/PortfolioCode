using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitialTutorialAdvancer : MonoBehaviour
{

    public GameObject player;
    public GameObject tutorialUI;
    int delayTimer = 50;
    int stepsTaken = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        delayTimer--;
    }

    public void advanceTutorial()
    {

        if (delayTimer <= 0)
        {
            delayTimer = 100;
            stepsTaken++;
            TutorialUIManager.advanceStep();
            if (stepsTaken == 2)
            {
                player.GetComponent<TutorialPlayerScript>().finishedMovement = false;
                player.GetComponent<TutorialPlayerScript>().finishedDashing = false;
                GameObject.Destroy(this);
            }
        }
    }
}
