using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Squid with pistol in each tentacle
public class Squid : MonoBehaviour, IEnemy
{
	[SerializeField]
	protected EnemyDataSO _statBlock;

	[SerializeField]
	GameObject _projectile;

	/// <summary>
	/// Should have 8 arms! So 8 spawn points!
	/// </summary>
	[SerializeField]
	List<Transform> _bulletSpawnPoints = new List<Transform>();
	int _currentSpawnIndex = 0;

	[SerializeField]
	float _timeBetweenShots = 5;
	float _elapsedTime = 0;

	PlayerScript player;

	protected int _currentHealth;

	// Start is called before the first frame update
	void Start()
	{
		_currentHealth = _statBlock.MaxHealth;
		player = FindAnyObjectByType<PlayerScript>();
	}

	public void Die()
	{
		FindObjectOfType<ScoreManager>().increaseScore(_statBlock.score);

		Destroy(gameObject);

		// Set it as disabled to prevent weird psuedo-alive stuff from happening
		gameObject.SetActive(false);
	}

	public void TakeDamage(int damage)
	{
		_currentHealth -= damage;
		Debug.Log($"{gameObject.name} taking {damage} damage, health is {_currentHealth}");

		if (_currentHealth <= 0)
		{
			Die();
		}
	}

	// Update is called once per frame
	void Update()
    {
		_elapsedTime += Time.deltaTime;
		if (_elapsedTime >= _timeBetweenShots)
		{
			_elapsedTime = 0;
			Attack();
		}
	}

	public void Attack()
	{
		if (_currentSpawnIndex >= _bulletSpawnPoints.Count)
		{
			_currentSpawnIndex = 0;
		}

		var spawnTransform = _bulletSpawnPoints[_currentSpawnIndex];
		_currentSpawnIndex++;

		GameObject projectile = Instantiate(_projectile, spawnTransform.position, spawnTransform.rotation);


		if (player != null)
		{
			var directionToPlayer = player.transform.position - transform.position;
			projectile.GetComponent<CrabBullet>().Initialize(directionToPlayer);
		}
		else
		{
			var playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
		
			var directionToPlayer = playerTransform.position - transform.position;
			projectile.GetComponent<CrabBullet>().Initialize(directionToPlayer);
		}
	}

	public EnemyDataSO GetStats() { return _statBlock; }
}
