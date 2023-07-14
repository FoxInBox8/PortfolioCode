using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrabBullet : MonoBehaviour
{
	[SerializeField]
	private float Speed = 0, _lifetime = 5, damage = 10;

	float _timeElapsed = 0;

	Vector3 _direction;

    private void Update()
    {
		transform.position += Speed * Time.deltaTime * _direction;

		_timeElapsed += Time.deltaTime;

		if (_timeElapsed >= _lifetime)
		{
			Destroy(gameObject);

			// set it as disabled to prevent weird psuedo-alive stuff from happening
			gameObject.SetActive(false);
		}
	}

	public void Initialize(Vector3 direction)
	{
		_direction = direction.normalized;
	}

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Player"))
		{
			collision.gameObject.GetComponent<PlayerScript>().dealDamage(damage);
		}

		Destroy(gameObject);
    }
}
