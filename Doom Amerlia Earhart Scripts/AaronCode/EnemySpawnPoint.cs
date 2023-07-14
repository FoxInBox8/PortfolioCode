using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnPoint : MonoBehaviour
{
    [SerializeField]
    private float maxX, maxY, maxZ;

    public Vector3 getNearbyPoint()
    {
        Vector3 point;

        point.x = Random.Range(transform.position.x - maxX, transform.position.x + maxX);
        point.y = Random.Range(transform.position.y - maxX, transform.position.y + maxX);
        point.z = Random.Range(transform.position.z - maxX, transform.position.z + maxX);

        return point;
    }
}