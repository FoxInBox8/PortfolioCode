using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum CrabType
{
	Crab,
	BigCrab,
	SmallCrab,
	ShooterCrab
}

/// <summary>
/// Should implement what every enemy needs to exist.
/// </summary>
public class Crab : MonoBehaviour, IEnemy
{
	[SerializeField]
	protected EnemyDataSO _statBlock;

	[SerializeField]
	protected float timeBetweenAttacks;

	protected int _currentHealth;
	protected float attackTimer = 0;
	protected bool touchingPlayer = false;

	protected PlayerScript player;
	protected NavMeshAgent agent;

	protected virtual void Start()
    {
		_currentHealth = _statBlock.MaxHealth;
		attackTimer = timeBetweenAttacks;

		player = FindAnyObjectByType<PlayerScript>();
		agent = GetComponent<NavMeshAgent>();
	}

	// Update is called once per frame
	protected virtual void Update()
    {
		move();

		if(touchingPlayer)
		{
			attack();
		}
    }

	protected virtual void move()
	{
		agent.destination = player.transform.position;
	}

	protected virtual void attack()
	{
		attackTimer += Time.deltaTime;

		if(attackTimer >= timeBetweenAttacks)
		{
			attackTimer = 0;
			player.dealDamage(_statBlock.Damage);
		}
	}

	public virtual void TakeDamage(int damage)
	{
		_currentHealth -= damage;
		Debug.Log($"{gameObject.name} taking {damage} damage, health is {_currentHealth}");

		if (_currentHealth <= 0)
		{
			Die();
		}
	}

	public virtual void Die()
	{
		FindObjectOfType<ScoreManager>().increaseScore(_statBlock.score);

		Destroy(gameObject);

		// Set it as disabled to prevent weird psuedo-alive stuff from happening
		gameObject.SetActive(false);
	}

    protected void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
		{
			touchingPlayer = true;
		}
    }

    protected void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
		{
			touchingPlayer = false;
		}
    }

	public EnemyDataSO GetStats() { return _statBlock; }
}
