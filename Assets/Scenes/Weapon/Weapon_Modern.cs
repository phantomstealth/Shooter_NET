using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static Unity.VisualScripting.Member;

public class Weapon_Modern : NetworkBehaviour
{
    [Header("GameObjects")]
    //public GameObject muzzleFlash;
    public GameObject impactEffect;
    public GameObject startBullet;
    public GameObject bullet_prefab;

    [Header("Audio")]
    public AudioClip Shot;
    public AudioClip EmptyClip;
    public AudioClip rocketLaunch;
    public AudioClip reloadClip;

    [Header("Weapon_Characteristics")]
    public float weaponRange = 1000f;
    public bool automaticGun;
    public int maxClipAmmo;
    [SyncVar] public int currentAmmo;
    [SyncVar] public int currentClipAmmo;
    public int maxDamage;

    [SerializeField]
    private float LastShootTime;
    [SerializeField]
    private float ShootDelay = 0.5f;


    [Header("Player_Current")]

    private AudioSource source;
    private RaycastHit hitWeapon;

    void Start()
    {
        source = GetComponent<AudioSource>();
    }

    void PlayAudio(AudioClip clip, bool loop)
    {
        source.loop = loop;
        source.clip = clip;
        source.Play();
    }


    void OnGUI()
    {
        if (!isLocalPlayer) return;
        GUI.Label(new Rect(25, 25, 100, 30), "Ammo: " + currentClipAmmo + " / " + currentAmmo);
        //GUI.Label(new Rect(25, Screen.height - 30, 100, 30), "Health: "+playerLocal.health+"%");
    }

    void Fire()
    {
        if (currentClipAmmo <= 0)
        {
            PlayAudio(EmptyClip, false);
            return;
        }

        if (hitWeapon.transform != null)
        {
            //GameObject ImpactGO = Instantiate(impactEffect, hitWeapon.point, Quaternion.LookRotation(hitWeapon.normal));
            //NetworkServer.Spawn(ImpactGO);
        }
        if (VerifyShotDelay())
        {
            Shoot();
            PlayAudio(Shot, false);
            LastShootTime = Time.time;
            currentAmmo = currentAmmo - 1;
            currentClipAmmo = currentClipAmmo - 1;
        }
    }

    public bool VerifyShotDelay()
    {
        if (LastShootTime + ShootDelay < Time.time) return true; else return false;
    }

    [Command]
    public void Shoot()
    {
        GameObject projectile = Instantiate(bullet_prefab, startBullet.transform.position, startBullet.transform.rotation);
        NetworkServer.Spawn(projectile);
    }



    [Command]
    void Fire_1()
    {
        //Debug.Log(startBullet.transform.position);
        GameObject projectile = Instantiate(bullet_prefab, startBullet.transform.position, startBullet.transform.rotation);
        NetworkServer.Spawn(projectile);
        PlayAudio(rocketLaunch, false);
    }


    // Update is called once per frame
    void CheckRaycast()
    {
        Physics.Raycast(startBullet.transform.position, startBullet.transform.forward, out hitWeapon, weaponRange, 1);
        //if (hitWeapon.transform != null) Debug.DrawLine(startBullet.transform.position, hitWeapon.point);
    }

    void Reload_Weapon()
    {
        if (currentAmmo <= 0 || maxClipAmmo==currentClipAmmo) return;
        if (maxClipAmmo < currentAmmo)
        {
            currentClipAmmo = maxClipAmmo;
            //currentAmmo = currentAmmo - maxClipAmmo;
        }
        else
        {
            currentClipAmmo = currentAmmo;
            //currentAmmo = 0;
        }
        PlayAudio(reloadClip, false);
    }

    void Update()
    {
        if (!isLocalPlayer) return;
        CheckRaycast();
        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.Mouse0)) Fire();
        if (Input.GetKeyDown(KeyCode.R)) Reload_Weapon();
    }
}
