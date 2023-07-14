using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using FMODUnity;
using FMOD.Studio;
using System.Collections;

[System.Serializable]
public class FMODAudioManager : MonoBehaviour
{
	//[SerializeField]
	//List of FMOD event instances
	private List<EventInstance> eventInstances;

	//Array of all FMOD one shots (non-looping sfx)
	public FMODOneShot[] oneShots;

	//Array of all FMOD events (anything that loops, i.e.: footsteps, music)
	public FMODEvent[] events;
	public static FMODAudioManager instance { get; private set; }
	public string playSceneName;


	//Prevent multiple audio managet instances, instantiate event list
	private void Awake()
	{
		//DontDestroyOnLoad(gameObject);
		// Ensure there is only ever one AudioManager
		//if (instance == null || SceneManager.GetActiveScene().name == playSceneName)
		//{
		//	instance = this;
		//}
		//else
		//{
		//	Destroy(gameObject);
		//	//Debug.LogError("Found multiple audio managers in scene");
		//	//return;
		//}

		eventInstances = new List<EventInstance>();
	}

    private void Start()
    {
		// Subscribe to event
		//SceneManager.sceneLoaded += validateScene;
	}

    // Unsubscribe to event on deletion
    private void OnDisable()
	{
		//SceneManager.sceneLoaded -= validateScene;
	}

	private void validateScene(Scene scene, LoadSceneMode mode)
    {
		Debug.Break();

		if(scene.name =="MainMenu" || scene.name == "EndScene" || scene.name == playSceneName)
        {
			Destroy(gameObject);
        }
    }

	// Return the FMOD Event given it's name; prints error if not found
	private FMODEvent getFMODEvent(string name)
	{
		FMODEvent e = Array.Find(events, sound => sound.name == name);
		if (e == null)
		{
			Debug.Log("FMOD Event \"" + name + "\" not found");
		}
		return e;
	}

	//Creates an FMOD Event Instance and returns it
	public EventInstance CreateFMODEventInstance(string name/*, bool allowMultipleInstances*/)
	{
		FMODEvent e = getFMODEvent(name);
		if (e != null)
		{
			EventInstance eventInstance = RuntimeManager.CreateInstance(e.fmodReference);
			eventInstances.Add(eventInstance);
			return eventInstance;

			//if(allowMultipleInstances)
			//{
			//	EventInstance eventInstance = RuntimeManager.CreateInstance(e.fmodReference);
			//	eventInstances.Add(eventInstance);
			//	return eventInstance;
			//}
			//else
			//{
			//	foreach (EventInstance activeInstance in eventInstances)
			//	{
			//		if (getFMODEventPath(activeInstance) == e.fmodReference.Path)
			//		{
			//			EventInstance nullEvent = RuntimeManager.CreateInstance(null);
			//			return nullEvent;
			//		}
			//	}
			//	EventInstance eventInstance = RuntimeManager.CreateInstance(e.fmodReference);
			//	eventInstances.Add(eventInstance);
			//	return eventInstance;
			//}
		}
		else
		{
			Debug.LogError("No FMOD Event found with name: " + name);
			EventInstance eventInstance = RuntimeManager.CreateInstance(null);
			return eventInstance;
		}
	}

	//Get path of FMOD Event (unused)
	//private string getFMODEventPath(EventInstance eventInstance)
	//{
	//	string result;
	//	EventDescription description;

	//	eventInstance.getDescription(out description);
	//	description.getPath(out result);

	//	return result;
	//}

	//Cleanup FMOD Event List
	private void CleanFMODEventList()
	{
		foreach (EventInstance eventInstance in eventInstances)
		{
			eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
			eventInstance.release();
		}
	}



	// Return the FMOD One Shot given it's name; prints error if not found
	private FMODOneShot getFMODOneShot(string name)
	{
		FMODOneShot os = Array.Find(oneShots, sound => sound.name == name);
		if (os == null)
		{
			Debug.Log("FMOD One Shot \"" + name + "\" not found");
		}
		return os;
	}

	// Play a sound given it's name
	public void PlayFMODOneShot(string name, Vector3 worldPosition)
	{
		FMODOneShot os = getFMODOneShot(name);
		if (os != null)
		{
			RuntimeManager.PlayOneShot(os.fmodReference, worldPosition);
		}
	}

	// Play a sound given it's name and a delay in seconds
	public void PlayFMODOneShot(string name, Vector3 worldPosition, float delay)
	{
		FMODOneShot os = getFMODOneShot(name);
		if (os != null)
		{
			StartCoroutine(PlayDelayedFMODOneShot(delay, os, worldPosition));
		}
	}

	//Coroutine for delay functionality
	IEnumerator PlayDelayedFMODOneShot(float delay, FMODOneShot os, Vector3 worldPosition)
	{
		yield return new WaitForSeconds(delay);
		RuntimeManager.PlayOneShot(os.fmodReference, worldPosition);
	}



	//Cleans up list upon destruction of object

	public void OnDestroy()
	{
		CleanFMODEventList();
	}
}