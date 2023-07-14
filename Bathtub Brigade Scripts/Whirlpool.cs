using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Whirlpool : MonoBehaviour
{
    // Class that holds a gameObject and its damage timer
    private class MobInRange
    {
        public GameObject mob;
        public float damageTimer;

        public MobInRange(GameObject newObj, float newTimer)
        {
            mob = newObj;
            damageTimer = newTimer;
        }
    }

    [SerializeField]
    private float spinPower, suckPower, timeBetweenDamage;

    [SerializeField]
    private int damage;

    [SerializeField]
    private float minPitch, maxPitch;
    public float maxDistance;

    public string whirlpoolSound;

    private List<MobInRange> mobsInRange = new List<MobInRange>();

    private void FixedUpdate()
    {
        // Update all mobs
        // Using for instead of foreach because foreach gives enumeration error when mob in whirlpool dies
        for (int i = mobsInRange.Count - 1; i >= 0; --i)
        {
            // Remove object when it dies
            if (mobsInRange[i].mob == null)
            {
                removeMob(mobsInRange[i].mob);
                continue;
            }

            mobsInRange[i].damageTimer += Time.fixedDeltaTime;

            // Deal damage if possible
            if(mobsInRange[i].damageTimer > timeBetweenDamage)
            {
                mobsInRange[i].damageTimer = 0;

                // Get correct script
                if (mobsInRange[i].mob.tag == "Player")
                {
                    mobsInRange[i].mob.GetComponent<PlayerScript>().dealDamage(damage);
                }

                else
                {
                    mobsInRange[i].mob.GetComponent<EnemyBase>().dealDamage(damage);
                }
            }

            // Spin around
            mobsInRange[i].mob.transform.RotateAround(transform.position, Vector3.up, spinPower * Time.fixedDeltaTime);

            // Keep y value for sucking
            Vector3 moveTarget = transform.position;
            moveTarget.y = mobsInRange[i].mob.transform.position.y;

            // Suck inwards
            mobsInRange[i].mob.transform.position = Vector3.MoveTowards(mobsInRange[i].mob.transform.position, moveTarget, suckPower * Time.fixedDeltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Add gameobject with new timer
        if (other.tag == "Player" || other.tag == "Enemy")
        {
            mobsInRange.Add(new MobInRange(other.gameObject, 0));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player" || other.tag == "Enemy")
        {
            removeMob(other.gameObject);
        }
    }

    private bool removeMob(GameObject toRemove)
    {
        // Find object that got removed
        // Using for instead of foreach to get the index in order to remove mob properly
        for (int i = 0; i < mobsInRange.Count; i++)
        {
            // Remove if found
            if (mobsInRange[i].mob == toRemove)
            {
                mobsInRange.RemoveAt(i);
                return true;
            }
        }

        // We shouldn't reach this, but if we ever try to remove a mob that wasn't in the array, something has gone wrong
        Debug.LogError("Tried to remove mob that wasn't already in range!");

        return false;
    }
}