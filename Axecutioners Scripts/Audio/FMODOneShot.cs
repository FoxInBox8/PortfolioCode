using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

[System.Serializable]
public class FMODOneShot
{
	public string name;			// Name of the sound
	public EventReference fmodReference; //Reference to an FMOD event

	//public float volume = 1;	// Volume of the sound
	//public Vector3 worldPos;
	//public float pitch = 1;		// Pitch of the sound
	//public bool loop;			// Whether or not the clip should loop
}