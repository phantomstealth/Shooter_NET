using UnityEngine;
using System.Collections;
using Mirror;
using static Unity.VisualScripting.Member;

[RequireComponent(typeof(ParticleSystem))]
public class CFX_AutoDestructShurikenNET : NetworkBehaviour
{
	public bool OnlyDeactivate;
	public float destroyAfter = 2f;


    void OnEnable()
	{
		//StartCoroutine("CheckIfAlive");
	}

    [Server]
    void DestroySelf()
    {
        NetworkServer.Destroy(gameObject);
    }
    public override void OnStartServer()
    {
        Invoke(nameof(DestroySelf), destroyAfter);
    }

    void FixedUpdate()
    {
		CheckAliveNew();
    }


    void CheckAliveNew()
	{
		if (!GetComponent<ParticleSystem>().IsAlive(true))
		{
			if (OnlyDeactivate)
			{
				gameObject.SetActive(false);
			}
			else
				DestroySelf();
		}
    }
}
