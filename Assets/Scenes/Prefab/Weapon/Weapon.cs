using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEditor;
using UnityEngine;
using static Unity.VisualScripting.Member;

public class Weapon : NetworkBehaviour
{
    [SerializeField]
    private Gun Gun;

    [Header("GameObjects")]
    public GameObject muzzleFlash;
    public GameObject impactEffect;
    public GameObject startBullet;
    public GameObject rocket;

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
    public P_Controller_Anim playerLocal;

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
        //GUILayout.Label("Ammo: " +currentClipAmmo +" / "+ currentAmmo);
        if (!isLocalPlayer) return;
        GUI.Label(new Rect(25, 25, 100, 30), "Ammo: " + currentClipAmmo + " / " + currentAmmo);
        //GUI.Label(new Rect(25, Screen.height - 30, 100, 30), "Health: "+playerLocal.health+"%");
        GUI.Label(new Rect(25, Screen.height - 30, 200, 30), "StartPoint: " + playerLocal.startBullet.transform.position + "%");
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
        if (Gun.VerifyShotDelay())
        {
            Gun.Shoot();
            PlayAudio(Shot, false);
            currentAmmo = currentAmmo - 1;
            currentClipAmmo = currentClipAmmo - 1;
        }
    }

    [Command]
    void Fire_1()
    {
        //Debug.Log(startBullet.transform.position);
        GameObject projectile = Instantiate(rocket, startBullet.transform.position, startBullet.transform.rotation);
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
        if (Input.GetKeyDown(KeyCode.Mouse1)) Fire_1();

        if (Input.GetKeyDown(KeyCode.R)) Reload_Weapon();
    }
}
