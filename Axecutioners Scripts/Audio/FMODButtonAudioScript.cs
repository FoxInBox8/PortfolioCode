using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FMODButtonAudioScript : MonoBehaviour
{
    public FMODAudioManager fmodAudioManager;

    float firstframetimer = 10f;

    private void Update()
    {
        if(firstframetimer >= 0f)
        {
            firstframetimer--;
        }
    }

    public void PlayHoverSFX()
    {
        if (firstframetimer <= 0f)
            fmodAudioManager.PlayFMODOneShot("UIHover", Vector3.zero);
    }

    public void PlayErrorSFX()
    {
        if (firstframetimer <= 0f)
            fmodAudioManager.PlayFMODOneShot("UIError", Vector3.zero);
    }

    public void PlayClickSFX()
    {
        if (firstframetimer <= 0f)
            fmodAudioManager.PlayFMODOneShot("UIClick", Vector3.zero);
    }
}
