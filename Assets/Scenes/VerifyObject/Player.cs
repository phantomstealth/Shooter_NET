using Mirror;
using Mirror.Examples.AdditiveScenes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.VisualScripting.Member;

public class Player : NetworkBehaviour
{
    private Rigidbody rigidBody;
    public float jumpForce = 5f;
    public float moveForce = 1f;
    [Header("Components")]
    public TextMesh healthBar;

    [SyncVar] public int health = 5;
    [SyncVar] private float horizontal;
    [SyncVar] private float vertical;

    [SerializeField] private bool jump;
    public GameObject bullet;
    private AudioSource source;
    [SerializeField]
    private bool startSound=false;


    [Header("Audio")]
    public AudioClip boxGround;
    public AudioClip boxSound;
    public AudioClip shotSound;

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        source = GetComponent<AudioSource>();
    }

    void PlayAudio(AudioClip clip, bool loop)
    {
        //if (source.isPlaying) return;
        source.loop = loop;
        source.clip = clip;
        source.Play();
    }

    void verify_Sound()
    {
        if ((startSound) &((Math.Round(transform.eulerAngles.x,2) == 0 )|| (Math.Round(transform.eulerAngles.y, 2)==0)|| (Math.Round(transform.eulerAngles.z, 2)==0)))
        {
            PlayAudio(boxSound, false);
            startSound = false;
        }

        if ((rigidBody.velocity.x != 0 || rigidBody.velocity.y != 0) & (transform.position.y < 1) & (transform.position.y > 0))
        {
            startSound = true;
            Debug.Log(transform.eulerAngles);
        }
    }

    void MovePersonal()
    {
        rigidBody.AddForce(horizontal * moveForce, 0, vertical * moveForce);
        if (jump) rigidBody.AddForce(new Vector3(0, jumpForce, 0));
        //verify_Sound();
    }
    private void FixedUpdate()
    {
        if (!isLocalPlayer) return;
        MovePersonal();
        VerifyFall();
    }

    void VerifyFall()
    {

        if (transform.position.y < -20)
        {
            Respawn();
            Damage();
        }
    }

    // Update is called once per frame
    void Update()
    {
        healthBar.text = new string('-', health);
        if (!isLocalPlayer) return;
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        jump = Input.GetKeyDown(KeyCode.Space);
        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.Mouse0)) Shoot();
        if (Input.GetKeyDown(KeyCode.Backspace)) Respawn();
    }

    void Respawn()
    {
        rigidBody.velocity = Vector3.zero;
        transform.position = new Vector3(0, 15, 0);
    }


    [Command]
    void Shoot()
    {
        PlayAudio(shotSound, false);
        GameObject projectile = Instantiate(bullet, transform.position+new Vector3(horizontal*2,0,vertical*2), transform.rotation);
        NetworkServer.Spawn(projectile);
        projectile.GetComponent<Bullet_ver>().direction =new Vector3(horizontal, 0,vertical);
        //Debug.Log(horizontal + " " + vertical);
    }

    [Server]
    void Death()
    {
        NetworkServer.Destroy(gameObject);
    }

    void Damage()
    {
        PlayAudio(boxSound, false);
        --health;
        if (health == 0) Death();
    }

    [ServerCallback]
    void OnCollisionEnter(Collision other)
    {
        //Debug.Log(other.gameObject.name);
        if (other.gameObject.name == "Plane") PlayAudio(boxGround, false);
        if (other.gameObject.GetComponent<Bullet_ver>() != null)
        {
            Damage();
            if (health == 0)
                NetworkServer.Destroy(gameObject);
        }
    }

}
