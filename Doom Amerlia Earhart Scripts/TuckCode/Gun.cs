using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{

    [SerializeField] int DMG;

    [SerializeField] float range;

    private GameObject gunModel;

    private float shotTimer;
    [SerializeField] float shotCD;

    private bool shotReady;

    private void Awake()
    {
        gunModel = gameObject.GetComponent<GameObject>();
    }

    public MeshRenderer GetGunModel()
    {
        return gunModel.GetComponent<MeshRenderer>();
    }

    protected virtual void Start()
    {
        shotTimer = shotCD;
    }

    protected virtual void Update()
    {
        ShotBuffer();
    }

    public virtual void Raycast(Transform camTransform, LayerMask layermask)
    {
        RaycastHit hit;
        Physics.Raycast(camTransform.position, camTransform.forward, out hit, range, layermask);
        DealDMG(hit);
    }

    protected virtual void DealDMG(RaycastHit raycast)
    {
        if (shotReady)
        {
            GameObject hit = raycast.collider.gameObject;

            if (hit.tag == "Enemy")
            {
                hit.GetComponent<Crab>().TakeDamage(DMG);
            }
            shotReady = false;
        }
    }

    protected virtual void ShotBuffer()
    {
        if(shotReady == false)
        {
            if (shotTimer > 0)
            {
                shotTimer -= Time.deltaTime;
            }
            else
            {
                shotReady = true;
                shotTimer = shotCD;
            }
        }
    }

}
