using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Projectile crab (launches projectiles at player, unsure of real-life equivalent)
//		- Big crab w/ harpoon launcher on its bag manned by a smaller crab
public class ProjectileCrab : Crab
{
	[SerializeField]
	private GameObject _projectile;

	[SerializeField]
	private float timeBetweenShots = 5;

	private float _elapsedTime = 0;

    protected override void Update()
    {
        _elapsedTime += Time.deltaTime;
        if (_elapsedTime >= timeBetweenShots)
        {
            _elapsedTime = 0;
            Attack();
        }

        base.Update();
    }

	private void Attack()
	{
		GameObject projectile = Instantiate(_projectile, transform.position, transform.rotation);

		var directionToPlayer = player.transform.position - transform.position;
		projectile.GetComponent<CrabBullet>().Initialize(directionToPlayer);
	}
}
