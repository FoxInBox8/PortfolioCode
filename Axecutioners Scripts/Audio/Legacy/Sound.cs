// Code from Brackeys https://www.youtube.com/watch?v=6OT43pvUyfY

using UnityEngine.Audio;
using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;			// Name of the sound
    public AudioClip clip;		// Audio clip that gets played
    
    public float volume = 1;	// Volume of the sound
    public float pitch = 1;		// Pitch of the sound
    public bool loop;			// Whether or not the clip should loop

    [HideInInspector]
    public AudioSource source;	// Source that plays the clip
}
