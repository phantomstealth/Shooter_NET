using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hottrail_M : NetworkBehaviour
{
    private Vector3 startPosition;
    private float distance;
    [SerializeField]
    private LayerMask Mask;
    [SerializeField]
    private float BulletSpeed = 100;
    private float remainingDistance;
    private Vector3 hitPoint;
    private Vector3 hitNormal;
    private bool madeImpact;
    [SerializeField]
    private GameObject ImpactParticleSystem;


    [Server]
    void DestroySelf()
    {
        NetworkServer.Destroy(gameObject);
    }

    private void Update()
    {
        transform.position = Vector3.Lerp(startPosition, hitPoint, 1 - (remainingDistance / distance));
        remainingDistance -= BulletSpeed * Time.deltaTime;
        if (remainingDistance <= 0 & transform.position != hitPoint)
        {
            transform.position = hitPoint;
            if (madeImpact)
            {
                GameObject ImpactGo = Instantiate(ImpactParticleSystem, hitPoint, Quaternion.LookRotation(hitNormal));
                NetworkServer.Spawn(ImpactGo);
                madeImpact = false;
            }
            //DestroySelf();
        }
        
    }


    private void Start()
    {
        Vector3 direction = transform.forward;
        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, float.MaxValue, Mask))
        {
            cmdTrail_Create(hit.point, hit.normal, true);
            Debug.DrawLine(transform.position, hit.point);
        }
        else
        {
            cmdTrail_Create(transform.position + transform.forward * 100, Vector3.zero, false);
            Debug.DrawLine(transform.position, transform.position + transform.forward * 100);

        }
    }

    private void cmdTrail_Create(Vector3 point, Vector3 normal, bool MadeImpact)
    {
        startPosition = transform.position;
        distance = Vector3.Distance(transform.position, point);
        remainingDistance = distance;
        hitPoint = point;
        hitNormal = normal;
        madeImpact = MadeImpact;
    }
}
