using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemy
{
	public EnemyDataSO GetStats();

	public void TakeDamage(int damage);

	public void Die();
}
