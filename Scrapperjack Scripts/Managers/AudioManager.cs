using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AudioManager : MonoBehaviour
{
    [SerializeField]
    private Sound[] sounds;

    private Settings settings;

    private void Start()
    {
        settings = FindObjectOfType<Settings>();

        setGlobalVolume();

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

    public void setGlobalVolume()
    {
        AudioListener.volume = settings.getSetting(Settings.SETTINGS_TYPE.VOLUME);
    }

    public void setVolume(string name, float volume)
    {
        // Do nothing if sound not found
        if(!findSound(name, out Sound s)) { return; }

        s.source.volume = volume;
    }

    public void setPitch(string name, float pitch)
    {
        // Do nothing if sound not found
        if (!findSound(name, out Sound s)) { return; }

        s.source.pitch = pitch;
    }

    public void play(string name)
    {
        // Do nothing if sound not found
        if (!findSound(name, out Sound s)) { return; }

        s.source.Play();
    }

    public void pause(string name)
    {
        // Do nothing if sound not found
        if (!findSound(name, out Sound s)) { return; }

        s.source.Pause();
    }

    public void unpause(string name)
    {
        // Do nothing if sound not found
        if (!findSound(name, out Sound s)) { return; }

        s.source.UnPause();
    }

    public void playRandomPitch(string name, float minPitch, float maxPitch)
    {
        // Do nothing if sound not found
        if (!findSound(name, out Sound s)) { return; }

        // Randomize pitch
        s.source.pitch = UnityEngine.Random.Range(minPitch, maxPitch);

        s.source.PlayOneShot(s.clip, s.volume);
    }

    public void stop(string name)
    {
        // Do nothing if sound not found
        if (!findSound(name, out Sound s)) { return; }

        s.source.Stop();
    }

    // TODO: make sure this and fadeIn work with global volume setting
    // Fades out a sound in fadeTime seconds from starting volume and stopping it at 0 volume
    public void fadeOut(string name, float startVolume, float fadeTime)
    {
        // Do nothing if sound not found
        if (!findSound(name, out Sound s)) { return; }

        // Fade out if audible
        if (s.source.volume > 0)
        {
            s.source.volume -= startVolume * Time.deltaTime / fadeTime;
        }

        // Stop playing
        else
        {
            s.source.Stop();

            // Reset to original volume
            s.source.volume = startVolume;
        }
    }

    // Fades in a sound in fadeTime seconds from current volume and stopping it at endVolume
    public void fadeIn(string name, float endVolume, float fadeTime)
    {
        // Find sound
        if (!findSound(name, out Sound s)) { return; }

        // Fade in
        if (s.source.volume < endVolume)
        {
            s.source.volume += endVolume * Time.deltaTime / fadeTime;
        }

        // Set final volume once faded in
        else
        {
            s.source.volume = endVolume;
        }
    }

    public void stopAll()
    {
        foreach (Sound s in sounds)
        {
            stop(s.name);
        }
    }

    // Override for stopAll that allows for exemptions
    public void stopAll(string[] exemptions)
    {
        foreach(Sound s in sounds)
        {
            // Skip if sound is exempt
            if(Array.Exists(exemptions, name => name == s.name)) { continue; }

            stop(s.name);
        }
    }

    public void pauseAll()
    {
        foreach (Sound s in sounds)
        {
            pause(s.name);
        }
    }

    // Override for pauseAll that allows for exemptions
    public void pauseAll(string[] exemptions)
    {
        foreach(Sound s in sounds)
        {            
            // Skip if sound is exempt
            if (Array.Exists(exemptions, name => name == s.name)) { continue; }

            pause(s.name);
        }
    }

    public void unpauseAll()
    {
        foreach (Sound s in sounds)
        {
            unpause(s.name);
        }
    }

    // Override for unpauseAll that allows for exemptions
    public void unpauseAll(string[] exemptions)
    {
        foreach (Sound s in sounds)
        {
            // Skip if sound is exempt
            if (Array.Exists(exemptions, name => name == s.name)) { continue; }

            unpause(s.name);
        }
    }

    private bool findSound(string name, out Sound foundSound)
    {
        // Search array for sound
        Sound s = Array.Find(sounds, sound => sound.name == name);

        // Set output
        foundSound = s;

        // Log warning if not found
        if (s == null) { Debug.LogWarning("Sound \"" + name + "\" not found!"); }

        // Return if found
        return s != null;
    }
}