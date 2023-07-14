using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class startMusic : MonoBehaviour
{

	public FMODAudioManager fmodAudioManager;
	private FMOD.Studio.EventInstance music;
	
	void Start()
	{
		if (fmodAudioManager)
        {
			music = fmodAudioManager.CreateFMODEventInstance("FightThemeTEMP");
			music.start();
		}
	}
	
	public void StopMusic()
	{
		music.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
	}
}