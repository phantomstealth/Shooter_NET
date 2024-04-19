using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hottrail : NetworkBehaviour
{
    public float destroyAfter = 2f;

    // Start is called before the first frame update
    public override void OnStartServer()
    {
        Invoke(nameof(DestroySelf), destroyAfter);
    }

    [Server]
    void DestroySelf()
    {
        NetworkServer.Destroy(gameObject);
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
