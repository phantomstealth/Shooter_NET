using UnityEngine;
using System.Collections;
using Mirror;
using static Unity.VisualScripting.Member;

public class AutoDestruct : NetworkBehaviour
{
	public bool OnlyDeactivate;
	public float destroyAfter = 2f;

    [Server]
    void DestroySelf()
    {
        NetworkServer.Destroy(gameObject);
    }
    public override void OnStartServer()
    {
        Invoke(nameof(DestroySelf), destroyAfter);
    }
}
