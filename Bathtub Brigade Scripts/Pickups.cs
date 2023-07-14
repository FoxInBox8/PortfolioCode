using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickups : MonoBehaviour
{
    [SerializeField]
    private float minTreasurePitch, maxTreasurePitch, minHealthPitch, maxHealthPitch;

    public enum PickupType {
        INVALID_TYPE = -1,
        TREASURE = 0,
        HEALTH,
        WOOD,
        METAL,
        PLASTIC
    }

    public const int NUM_PICKUP_TYPES = 5;

    [SerializeField]
    private List<string> pickupSounds = new List<string>();

    public int value;
    public PickupType type;

    private GameObject boat;

    private void Awake()
    {
        boat = GameObject.FindGameObjectWithTag("Player");
    }

    private void OnTriggerEnter(Collider other) {
        // Perform appropriate action when player touches pickup
        if (other.gameObject.name == boat.name) {
            FindObjectOfType<AudioManager>().playRandomPitch(pickupSounds[(int)type], minTreasurePitch, maxTreasurePitch);
            other.gameObject.GetComponent<PlayerScript>().collect(type, value);

            // Destroy on pickup
            Destroy(gameObject);
        }
    }
}