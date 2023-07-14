using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class platformBobbing : MonoBehaviour
{
    [SerializeField]
    private float speed, range;

    private float current;
    private Vector3 startPos;

    private void Start ()
    {
        // Save starting position for later
        startPos = transform.position;
    }

    private void Update()
    {
        // Increase current position
        current += speed * Time.deltaTime;

        // Move in wave
        transform.position = startPos + new Vector3(0, Mathf.Sin(current) * range, 0);
    }
}