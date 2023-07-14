using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using UnityEngine;
using System;

public class UpgradeScript : MonoBehaviour
{
    public const int NUM_UPGRADE_LEVELS = 4, NUM_UPGRADE_TYPES = 6;

    public enum upgradeType
    {
        INVALID_TYPE = -1,
        CANNON_RANGE = 0,
        CANNON_DAMAGE,
        SHIP_HEALTH,
        TURN_RADIUS,
        MAX_SPEED,
        CANNON_RELOAD,
    }

    public class Upgrade
    {
        public upgradeType type;
        public int currentLevel;
        public float[] values;
        public Pickups.PickupType materialForUpgrade;

        public Upgrade()
        {
            type = upgradeType.INVALID_TYPE;
            currentLevel = 0;
            values = new float[NUM_UPGRADE_LEVELS];
            materialForUpgrade = Pickups.PickupType.INVALID_TYPE;
        }
    }

    public int[] goldUpgradeCost = new int[NUM_UPGRADE_LEVELS], materialUpgradeCost = new int[NUM_UPGRADE_LEVELS];

    [SerializeField]
    private string fileName;

    public Upgrade[] upgradeList { get; private set; } = new Upgrade[NUM_UPGRADE_TYPES];

    private GameObject boat;
    private PlayerScript player;

    void Start() {

        // Initialize upgrades
        for(int i = 0; i < upgradeList.Length; ++i)
        {
            upgradeList[i] = new Upgrade();
            upgradeList[i].type = (upgradeType)i;
        }

        // Get full file path
        string filePath = Path.Combine(Application.streamingAssetsPath, fileName);

        // Read values from file
        using (StreamReader sr = new StreamReader(filePath)) {
            string line;
            int i;

            // Read through the file skipping every other line
            for (int lineNumber = 0; (line = sr.ReadLine()) != null; ++lineNumber)
            {
                if (lineNumber % 2 == 1)
                {
                    // Read the CSVs into an array
                    string[] strings = line.Split(',');

                    for (i = 0; i < strings.Length - 1; ++i)
                    {
                        // Load upgrade values
                        upgradeList[lineNumber / 2].values[i] = float.Parse(strings[i], CultureInfo.InvariantCulture.NumberFormat); ;
                    }

                    // Load upgrade materials
                    upgradeList[lineNumber / 2].materialForUpgrade = (Pickups.PickupType)int.Parse(strings[i], CultureInfo.InvariantCulture.NumberFormat);
                }
            }
        }

        boat = GameObject.FindGameObjectWithTag("Player");
        player = FindObjectOfType<PlayerScript>();

        // Reset all upgradable values to defaults
        foreach(Upgrade u in upgradeList)
        {
            updateLevel(u.type, 0);
        }
    }

    // Wrapper function so other classes don't need to deal with logic
    public void upgrade(upgradeType type)
    {
        // Save this here to improve readability
        Upgrade newUpgrade = upgradeList[(int)type];

        // Make sure not already at max level
        if(newUpgrade.currentLevel >= NUM_UPGRADE_LEVELS - 1)
        {
            Debug.Log("Max level!");
            return;
        }

        // Only upgrade if player has enough money and materials
        if(player.collectables[(int)Pickups.PickupType.TREASURE] >= goldUpgradeCost[newUpgrade.currentLevel + 1] && 
           player.collectables[(int)newUpgrade.materialForUpgrade] >= materialUpgradeCost[newUpgrade.currentLevel] + 1)
        {
            // Deduct cost from player
            player.collect(Pickups.PickupType.TREASURE, -goldUpgradeCost[newUpgrade.currentLevel + 1]);
            player.collect(newUpgrade.materialForUpgrade, -materialUpgradeCost[newUpgrade.currentLevel + 1]);

            updateLevel(type, newUpgrade.currentLevel + 1);
        }
        
        else
        {
            Debug.Log("Not enough resources!");
        }
    }

    public void updateLevel(upgradeType type, int newLevel) {
        int toUpgrade = (int)type;

        // Update the list of levels
        upgradeList[toUpgrade].currentLevel = Mathf.Min(newLevel, NUM_UPGRADE_LEVELS);

        // Get the new value
        float newValue = upgradeList[toUpgrade].values[newLevel];


        Transform cannonHolder = boat.transform.GetChild(0);

        switch (type) {
            case upgradeType.CANNON_RANGE:
                // Update all the cannons' shoot power
                cannonHolder.GetChild(0).GetComponent<DriverCannon>().power = newValue;
                cannonHolder.GetChild(2).GetComponent<DriverCannon>().power = newValue;
                cannonHolder.GetChild(1).GetComponent<FollowerCannon>().power = newValue;
                cannonHolder.GetChild(3).GetComponent<FollowerCannon>().power = newValue;

                break;

            case upgradeType.CANNON_DAMAGE:

                // Update all the cannons' dimaggio
                cannonHolder.GetChild(0).GetComponent<DriverCannon>().projectile.GetComponent<Cannonball>().damage = (int)newValue;
                cannonHolder.GetChild(2).GetComponent<DriverCannon>().projectile.GetComponent<Cannonball>().damage = (int)newValue;
                cannonHolder.GetChild(1).GetComponent<FollowerCannon>().projectile.GetComponent<Cannonball>().damage = (int)newValue;
                cannonHolder.GetChild(3).GetComponent<FollowerCannon>().projectile.GetComponent<Cannonball>().damage = (int)newValue;

                break;

            case upgradeType.CANNON_RELOAD:

                // Update the drive cannons' reload speed
                cannonHolder.GetChild(0).GetComponent<DriverCannon>().reloadTime = newValue;
                cannonHolder.GetChild(2).GetComponent<DriverCannon>().reloadTime = newValue;

                // Update the UI reload bar values
                FindObjectOfType<UIManager>().leftCannonBar.maxValue = newValue;
                FindObjectOfType<UIManager>().rightCannonBar.maxValue = newValue;

                break;

            case upgradeType.SHIP_HEALTH:

                // Update ship health and heal the player
                boat.GetComponent<PlayerScript>().startHealth = (int)newValue;
                boat.GetComponent<PlayerScript>().heal((int)newValue);

                // Update the UI health bar
                FindObjectOfType<UIManager>().healthBar.maxValue = (int)newValue;

                break;

            case upgradeType.TURN_RADIUS:

                // Update ship turn speed
                boat.GetComponent<PlayerMovement>().turningFactor = newValue;

                break;

            case upgradeType.MAX_SPEED:
                // Update ship max speed
                boat.GetComponent<PlayerMovement>().updateMaxSpeed(newValue);

                break;
        }
    }
}