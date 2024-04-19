using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet_ver : NetworkBehaviour
{
    public float speed=1000f;
    public float destroyAfter = 2f;
    [SyncVar] public Vector3 direction;
    private Rigidbody rb;

    private void Start()
    {
        rb=GetComponent<Rigidbody>(); 
        rb.AddForce(direction*speed);
    }
    public override void OnStartServer()
    {
        Invoke(nameof(DestroySelf), destroyAfter);
    }

    [Server]
    void DestroySelf()
    {
        NetworkServer.Destroy(gameObject);
    }

    [ServerCallback]
    void OnTriggerEnter(Collider co) => DestroySelf();
}
