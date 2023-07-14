using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinScript : MonoBehaviour
{
    [SerializeField]
    private float rotSpeed;

    private void Update()
    {
        transform.Rotate(0f, 0f, rotSpeed);
    }
}