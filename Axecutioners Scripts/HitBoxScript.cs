using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Literally just for holding 3 variables :3
public class HitBoxScript : MonoBehaviour
{
	public int stunTime;		// Length of time the player will be stunned
	public int damage;			// Amount of damage the attack does
	public Vector3 launchPower;	// The amonut of velocity added to the player when struck to launch them
}