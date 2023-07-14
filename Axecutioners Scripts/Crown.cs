using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crown : MonoBehaviour
{
	public GameObject crown;
	private Vector3 defaultCrownPosition = new Vector3(0f, 0.1f, 0f), defaultZonePosition = new Vector3(0f, 0.25f, 0f);
    public Transform defaultParent;

	private void OnTriggerEnter(Collider trigger)
	{
		if (trigger.gameObject.CompareTag("Player"))
		{
			trigger.gameObject.GetComponent<PlayerScript>().setInZone(true);
		}
	}

	private void OnTriggerExit(Collider trigger)
	{
		if (trigger.gameObject.CompareTag("Player"))
		{
			trigger.gameObject.GetComponent<PlayerScript>().setInZone(false);
		}
	}

	public void DestroyZone(PlayerScript crownedPlayer)
	{
		// Stop crowning players
		PlayerScript[] players = FindObjectsOfType<PlayerScript>();

		foreach(PlayerScript p in players)
        {
			p.setInZone(false);
        }

		// Move crown to player
		crown.transform.position = crownedPlayer.crownPoint.position;
		crown.transform.parent = crownedPlayer.crownPoint;

		// Turn self off
		gameObject.SetActive(false);
	}

	//reset for networking
	public void Reset()
	{
        gameObject.SetActive(true);

        transform.position = defaultZonePosition;
		transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));
        crown.transform.position = defaultCrownPosition;
        crown.transform.parent = defaultParent;
    }
}