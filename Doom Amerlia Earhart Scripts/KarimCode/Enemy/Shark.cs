using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Shark (eats player if they go out of bounds)
public class Shark : MonoBehaviour, IEnemy
{
	protected EnemyDataSO _statBlock;

	public void Die()
	{
		throw new System.NotImplementedException();
	}

	public void TakeDamage(int damage)
	{
		throw new System.NotImplementedException();
	}

	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public EnemyDataSO GetStats() { return _statBlock; }
}
