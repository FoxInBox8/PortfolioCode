using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinScript : MonoBehaviour
{
    [SerializeField]
    private Vector3 spinSpeed;

    private void Update()
    {
        transform.Rotate(spinSpeed * Time.deltaTime);
    }
}