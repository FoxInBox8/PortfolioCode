using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AudioManager : MonoBehaviour
{
    [SerializeField]
    private Sound[] sounds;

    private void Start()
    {
        // Initialize sounds
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    public void setVolume(string name, float volume) {
        // Find sound
        Sound s = Array.Find(sounds, sound => sound.name == name);

        // Play if found
        if (s != null) {
            s.source.volume = volume;
        } else {
            Debug.LogWarning("Sound \"" + name + "\" not found!");
        }
    }

    public void setPitch(string name, float pitch) {
        // Find sound
        Sound s = Array.Find(sounds, sound => sound.name == name);

        // Play if found
        if (s != null) {
            s.source.pitch = pitch;
        } else {
            Debug.LogWarning("Sound \"" + name + "\" not found!");
        }
    }


    // TODO - refactor playing into one function
    public void play(string name)
    {
        // Find sound
        Sound s = Array.Find(sounds, sound => sound.name == name);

        // Play if found
        if (s != null)
        {
            s.source.PlayOneShot(s.clip, s.volume);
        }

        else
        {
            Debug.LogWarning("Sound \"" + name + "\" not found!");
        }
    }

    public void playRandomPitch(string name, float minPitch, float maxPitch)
    {
        // Find sound
        Sound s = Array.Find(sounds, sound => sound.name == name);

        // Play if found
        if(s != null)
        {
            s.source.pitch = UnityEngine.Random.Range(minPitch, maxPitch);
            s.source.PlayOneShot(s.clip, s.volume);
        }

        else
        {
            Debug.LogWarning("Sound \"" + name + "\" not found!");
        }
    }

    public void playLoop(string name, float volume) {
        // Find sound
        Sound s = Array.Find(sounds, sound => sound.name == name);

        // Play if found
        if (s != null) {
            s.source.volume = volume;
            s.source.loop = true;
            s.source.Play();
        } else {
            Debug.LogWarning("Sound \"" + name + "\" not found!");
        }
    }

    public void stop(string name) {
        // Find sound
        Sound s = Array.Find(sounds, sound => sound.name == name);

        // Stop if found
        if (s != null) {
            s.source.Stop();
        } else {
            Debug.LogWarning("Sound \"" + name + "\" not found!");
        }
    }


    // Fades out a sound in fadeTime seconds from starting volume and stopping it at 0 volume
    public void fadeOut(string name, float startVolume, float fadeTime) {
        // Find sound
        Sound s = Array.Find(sounds, sound => sound.name == name);

        // Fade out if found
        if (s != null) {
            if (s.source.volume > 0) {
                s.source.volume -= startVolume * Time.deltaTime / fadeTime;
            } else {
                s.source.Stop();

                // Reset to original volume
                s.source.volume = startVolume;
            }
        } else {
            Debug.LogWarning("Sound \"" + name + "\" not found!");
        }
    }

    // Fades in a sound in fadeTime seconds from current volume and stopping it at  endVolume
    public void fadeIn(string name, float endVolume, float fadeTime) {
        // Find sound
        Sound s = Array.Find(sounds, sound => sound.name == name);

        // Fade out if found
        if (s != null) {
            if (s.source.volume < endVolume) {
                s.source.volume += endVolume * Time.deltaTime / fadeTime;
            } else {
                s.source.volume = endVolume;
            }
        } else {
            Debug.LogWarning("Sound \"" + name + "\" not found!");
        }
    }
}