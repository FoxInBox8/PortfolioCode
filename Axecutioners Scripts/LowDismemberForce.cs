using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LowDismemberForce : MonoBehaviour
{
    public Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb.AddForce(new Vector3(Random.Range(-1f, 0f), 0f, Random.Range(0f, 1f)) * Random.Range(100f, 500f));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
