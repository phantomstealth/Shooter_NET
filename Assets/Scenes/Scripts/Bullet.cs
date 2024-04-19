using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    public float destroyAfter = 2f;
    public float force = 2500f;
    public Rigidbody rigidBody;
    public GameObject dfltImpactEffect;
    public int maxDamage;

    public override void OnStartServer()
    {
        Invoke(nameof(DestroySelf), destroyAfter);
    }

    void Start()
    {
        rigidBody.AddForce(transform.forward * force);
    }


    [Server]
    void DestroySelf()
    {
        NetworkServer.Destroy(gameObject);
    }

    [Server]
    void Impact()
    {
        GameObject impactEffect = Instantiate(dfltImpactEffect, transform.position, transform.rotation);
        NetworkServer.Spawn(impactEffect);
    }

    [ServerCallback]
    void OnTriggerEnter(Collider co)
    {
        Impact();
        DestroySelf();
    }

}
