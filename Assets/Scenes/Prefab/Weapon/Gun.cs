using Mirror;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Gun : NetworkBehaviour
{
    [SerializeField]
    private bool AddBulletSpread = true;
    [SerializeField]
    private Vector3 BulletSpreadVariance = new Vector3(0.1f, 0.1f, 0.1f);
    [SerializeField]
    public Transform BulletSpawnPoint;
    [SerializeField]
    private GameObject ImpactParticleSystem;
    [SerializeField]
    private GameObject BulletTrail;
    [SerializeField]
    private float ShootDelay = 0.5f;
    [SerializeField]
    private LayerMask Mask;
    [SerializeField]
    private float BulletSpeed = 100;
    [SyncVar] public int VerifyDelete;

    private float LastShootTime;

    private GameObject trail;
    private Vector3 startPosition;
    private float distance;
    private float remainingDistance;
    private Vector3 hitPoint;
    private Vector3 hitNormal;
    private bool madeImpact;

    public override void OnStartLocalPlayer()
    { 
    }

    public bool VerifyShotDelay()
    {
        if (LastShootTime + ShootDelay < Time.time) return true; else return false;
    }

    void DestroySelf()
    {
        Destroy(trail, trail.GetComponent<TrailRenderer>().time);
    }

    private void Update()
    {
        //Debug.DrawLine(startPosition, hitPoint, Color.cyan);

        if (trail != null)
        {
            trail.transform.position = Vector3.Lerp(startPosition, hitPoint, 1 - (remainingDistance / distance));
            remainingDistance -= BulletSpeed * Time.deltaTime;
            if (remainingDistance <= 0 & trail.transform.position!=hitPoint)
            {
                trail.transform.position = hitPoint;
                if (madeImpact)
                {
                    GameObject ImpactGo = Instantiate(ImpactParticleSystem, hitPoint, Quaternion.LookRotation(hitNormal));
                    NetworkServer.Spawn(ImpactGo);
                    madeImpact = false;
                }
                DestroySelf();
            }
        }
    }

    [Command]
    private void cmdTrail_Create(Vector3 point, Vector3 normal, bool MadeImpact) 
    {
        Debug.Log(BulletSpawnPoint.transform.position);
        trail = Instantiate(BulletTrail, BulletSpawnPoint.position, BulletSpawnPoint.rotation);
        NetworkServer.Spawn(trail);
        startPosition = trail.transform.position;
        distance = Vector3.Distance(trail.transform.position, point);
        remainingDistance = distance;
        hitPoint = point;
        hitNormal = normal;
        madeImpact = MadeImpact;
    }

    public void Shoot()
    {
        if (LastShootTime + ShootDelay < Time.time)
        {
            Vector3 direction = GetDirection(BulletSpawnPoint.transform.forward);

            //Debug.DrawLine(BulletSpawnPoint.transform.position, BulletSpawnPoint.transform.position + direction * 1000, Color.cyan);

            if (Physics.Raycast(BulletSpawnPoint.position, direction, out RaycastHit hit, float.MaxValue, Mask))
            {
                cmdTrail_Create(hit.point,hit.normal,true);
                LastShootTime = Time.time;
            }
            else
            {
                cmdTrail_Create(BulletSpawnPoint.position + GetDirection(BulletSpawnPoint.transform.forward) * 100, Vector3.zero, false);
                LastShootTime = Time.time;
            }
        }
    }

    private Vector3 GetDirection(Vector3 direction)
    {
        if (AddBulletSpread)
        {
            direction += new Vector3(
                Random.Range(-BulletSpreadVariance.x, BulletSpreadVariance.x),
                Random.Range(-BulletSpreadVariance.y, BulletSpreadVariance.y),
                Random.Range(-BulletSpreadVariance.z, BulletSpreadVariance.z)
            );

            direction.Normalize();
        }

        return direction;
    }
}
