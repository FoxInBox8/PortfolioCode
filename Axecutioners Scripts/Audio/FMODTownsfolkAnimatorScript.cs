using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FMODTownsfolkAnimatorScript : MonoBehaviour
{
    FMODMusicManager fmodMusicManager;

    string lastMarker;

    public Animator animator;

    public string animationToPlay;

    public float beatToAnimate1;
    public float beatToAnimate2;
    public float beatToAnimate3 = 4;

    float currentSection;

    float currentBeat;

    bool oneShotBool;

    // Start is called before the first frame update
    void Start()
    {
        //Get Music Manager Instance
        oneShotBool = true;
        fmodMusicManager = FMODMusicManager.instance;
    }

    // Update is called once per frame
    void Update()
    {
        //Import variables from beat detection
        lastMarker = fmodMusicManager.timelineInfo.lastMarker;
        currentBeat = fmodMusicManager.timelineInfo.currentBeat;

        //Get current section of song
        if(lastMarker.StartsWith("0"))
        {
            currentSection = 0;
        }
        else if(lastMarker.StartsWith("1"))
        {
            currentSection = 1;
        }
        else if (lastMarker.StartsWith("2"))
        {
            currentSection = 2;
        }
        else if (lastMarker.StartsWith("3"))
        {
            currentSection = 3;
        }
        else
        {

        }

        //Only allow for cheer animations if crowd vocals are currently in the song
        if(
            lastMarker == "0_Verse1" ||
            lastMarker == "0_MegaSolo" ||
            lastMarker == "1_CrowdBridgeA" ||
            lastMarker == "1_CrowdBridge_B" ||
            lastMarker == "1_MiniSolo" ||
            lastMarker == "2_MiniSolo2" ||
            lastMarker == "3_OrganSection"
          )
        {
            GetAnimationTrigger();
        }
    }

    void GetAnimationTrigger()
    {
        //Play animations when crowd vocals trigger
        if((currentSection == 1 || currentSection == 2) && currentBeat == beatToAnimate1)
        {
            if (oneShotBool)
            {
                oneShotBool = false;
                //trigger animation once
                animator.Play(animationToPlay);
            }
            oneShotBool = false;
        }
        else if(currentSection == 3 && currentBeat == beatToAnimate2)
        {
            if (oneShotBool)
            {
                oneShotBool = false;
                //trigger animation once
                animator.Play(animationToPlay);
            }
            oneShotBool = false;
        }
        else if(currentSection == 0 && currentBeat == beatToAnimate3)
        {
            if (oneShotBool)
            {
                oneShotBool = false;
                //trigger animation once
                animator.Play(animationToPlay);
            }
            oneShotBool = false;
        }
        else
        {
            oneShotBool = true;
        }
    }
}
