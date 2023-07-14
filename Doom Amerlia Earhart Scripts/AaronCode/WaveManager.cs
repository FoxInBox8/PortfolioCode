using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [SerializeField]
    private int startCredits, creditsPerWave;

    [SerializeField]
    private List<Crab> enemies;

    private int currentCredits;
    private EnemySpawnPoint[] spawnPoints;

    private void Start()
    {
        currentCredits = startCredits;
        spawnPoints = FindObjectsOfType<EnemySpawnPoint>();

        spawnWave();
    }

    private void Update()
    {
        // If no enemies, spawn a new wave
        if(GameObject.FindGameObjectsWithTag("Enemy").Length == 0)
        {
            currentCredits = startCredits + creditsPerWave;

            spawnWave();
        }
    }

    private void spawnWave()
    {
        List<Crab> affordableEnemies = new(enemies);

        // Spawning algorithm:
        //  1. Choose a random crab from the list
        //  2. If that crab is too expensive, remove it from the list
        //  3. If we can afford that crab, spawn it and subtract its credit cost
        //  4. Keep doing this until we can't spawn any more enemies
        while (affordableEnemies.Count > 0)
        {
            int randomEnemy = Random.Range(0, affordableEnemies.Count);

            if (affordableEnemies[randomEnemy].GetStats().CreditCost > currentCredits)
            {
                affordableEnemies.Remove(affordableEnemies[randomEnemy]);
            }

            else
            {
                // Choose a random spawnpoint and randomly choose a nearby position to spawn the enemy
                int randomSpawnPoint = Random.Range(0, spawnPoints.Length);
                Vector3 spawnPos = spawnPoints[randomSpawnPoint].getNearbyPoint();

				// Make sure all enemies spawn on the ground, will probably break if the terrain is not all flat
				spawnPos.y = spawnPoints[randomSpawnPoint].transform.position.y + affordableEnemies[randomEnemy].transform.localScale.y / 2;

                Instantiate(affordableEnemies[randomEnemy], spawnPos, transform.rotation);
				currentCredits -= affordableEnemies[randomEnemy].GetStats().CreditCost;
			}
        }
    }
}