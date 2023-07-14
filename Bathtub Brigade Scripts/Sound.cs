using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

// Just holds a list of attributes for different sounds
public class Sound
{
    public string name;

    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume;

    [Range(.1f, 2f)]
    public float pitch;

    public bool loop;

    [HideInInspector]
    public AudioSource source;
}