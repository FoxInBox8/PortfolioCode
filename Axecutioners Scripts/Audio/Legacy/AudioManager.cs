// Code from Brackeys https://www.youtube.com/watch?v=6OT43pvUyfY

using UnityEngine.Audio;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class AudioManager : MonoBehaviour
{
	public Sound[] sounds;					// The list of sounds available to play; set in inspector
	public static AudioManager instance;	// The instance of the AudioManager; only one allowed to exist, DontDestroyOnLoad
	public string playSceneName;			// The name of the scene the main game takes place in, used to ensure sounds play when intended

	void Awake()
	{
		// Ensure there is only every one AudioManager
		if (instance == null || SceneManager.GetActiveScene().name == playSceneName)
		{
			instance = this;
		}
		else
		{
			Destroy(gameObject);
			return;
		}
		DontDestroyOnLoad(gameObject);

		// Initialize each sound object with the appropriate values
		foreach (Sound s in sounds)
		{
			s.source = gameObject.AddComponent<AudioSource>();
			s.source.clip = s.clip;
			s.source.volume = s.volume;
			s.source.pitch = s.pitch;
			s.source.loop = s.loop;
		}
	}

	void Update()
	{
		// While in the game scene
		if (SceneManager.GetActiveScene().name == playSceneName)
		{
			// Play the backgronud sounds
			if (!getSound("CityAmbience").source.isPlaying && !getSound("CrowdAmbience").source.isPlaying)
			{
				Play("CityAmbience");
				Play("CrowdAmbience");
			}
		}
		else
		{
			// Stop all of them except the player death (as that one will last longer than the scene change)
			foreach (Sound s in sounds)
			{
				if (s.name != "PlayerDeath")
				{
					s.source.Stop();
				}
			}
		}
	}

	// Return the Sound object given it's name; prints error if not found
	private Sound getSound(string name)
	{
		Sound s = Array.Find(sounds, sound => sound.name == name);
		if (s == null)
		{
			Debug.Log("Sound \"" + name + "\" not found");
		}
		return s;
	}

	// Play a sound given it's name
	public void Play(string name)
	{
		Sound s = getSound(name);
		if (s != null)
		{
			s.source.Play();
		}
	}

	// Stop a sound given it's name
	public void Stop(string name)
	{
		Sound s = getSound(name);
		if (s != null)
		{
			s.source.Stop();
		}
		
	}

	// Play a sound given it's name after a delay in seconds
	public void PlayDelayed(string name, float delay)
	{
		Sound s = getSound(name);
		if (s != null)
		{
			s.source.PlayDelayed(delay);
		}
	}

	// Pause every sound
	public void PauseAudio()
	{
		foreach (Sound s in sounds)
		{
			s.source.Pause();
		}
	}

	// Unpause every sound
	public void UnpauseAudio()
	{
		foreach (Sound s in sounds)
		{
			s.source.UnPause();
		}
	}

	// Stop every sound
	public void StopAudio()
	{
		foreach (Sound s in sounds)
		{
			s.source.Stop();
		}
	}
}
