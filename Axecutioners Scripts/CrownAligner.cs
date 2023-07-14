using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrownAligner : MonoBehaviour
{
    void Update()
    {
        if (gameObject.transform.parent != null)
		{
			gameObject.transform.rotation = gameObject.transform.parent.rotation;
		}
    }
}
