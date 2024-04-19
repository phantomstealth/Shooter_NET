using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Runtime.ConstrainedExecution;
using UnityEngine.SceneManagement;
using static Unity.VisualScripting.Member;
using JetBrains.Annotations;
using System;
using UnityEngine.Animations.Rigging;

public class P_Controller_Anim : NetworkBehaviour
{
    [Header("Stats")]
    [SyncVar] public int health = 100;
    public bool hitme_bool;

    [Header("Components")]
    public TextMesh healthBar;

    //public GameObject VerifyGravity;
    [HideInInspector]
    public bool canMove = true;
    private float animationInterpolation = 1f;


    [Header("Base setup")]
    public float walkingSpeed = 7.5f;
    public float runningSpeed = 11.5f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 45.0f;

    [Header("Animation")]
    public Animator anim;
    private NetworkAnimator n_anim;
    private AnimatorClipInfo[] m_CurrentClipInfo;
    private float m_CurrentClipLength;
    private string m_ClipName;
    public Rig rig;


    [Header("Character Controller")]
    CharacterController characterController;
    public Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;

    [Header("Audio")]
    public AudioClip Walk_a;
    public AudioClip hitme_a;
    public AudioClip LandingAudioClip;


    [Header("Player Grounded")]
    [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
    public bool Grounded = true;

    [Tooltip("Useful for rough ground")]
    public float GroundedOffset = -0.14f;

    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    public float GroundedRadius = 0.28f;

    [Tooltip("What layers the character uses as ground")]
    public LayerMask GroundLayers;

    private AudioSource source;

    [Header("Настройки камеры")]

    [SerializeField]
    private float cameraXOffset = 1;
    [SerializeField]
    private float cameraYOffset = 1.97f;
    [SerializeField]
    private float cameraZOffset = -2.79f;
    public bool debugDrawLine = false;
    public bool debugDrawLine_Weapon = true;

    private Camera playerCamera;


    public GameObject RaycastSphere;

    [Header("Настройки оружия")]
    public GameObject startBullet;
    public GameObject hitBullet;
    public GameObject ImpactEffect;

    //private GameObject rSphere;

    void Awake()
    {
        //rSphere = Instantiate(RaycastSphere, gameObject.transform.position, gameObject.transform.rotation);
        playerCamera = Camera.main;
        anim = GetComponent<Animator>();
        n_anim = GetComponent<NetworkAnimator>();
        //rig = GetComponent<Rig>();
        //playerCamera.transform.position = new Vector3(transform.position.x, transform.position.y + cameraYOffset, transform.position.z);
        //playerCamera.transform.SetParent(transform);
    }

    public override void OnStartLocalPlayer()
    {
        if (playerCamera != null)
        {
            // configure and make camera a child of player with 3rd person offset
            playerCamera.transform.position = new Vector3(transform.position.x + cameraXOffset, transform.position.y + cameraYOffset, transform.position.z + cameraZOffset);
            playerCamera.transform.SetParent(transform);
            //canvasCross = GameObject.FindGameObjectWithTag("Cross");
            //canvasCross.GetComponent<Image>().enabled = true;
        }
        else
            Debug.LogWarning("PlayerCamera: Could not find a camera in scene with 'MainCamera' tag.");
    }

    public override void OnStopLocalPlayer()
    {
        if (playerCamera != null)
        {
            playerCamera.transform.SetParent(null);
            SceneManager.MoveGameObjectToScene(playerCamera.gameObject, SceneManager.GetActiveScene());
            playerCamera.orthographic = true;
            playerCamera.orthographicSize = 15f;
            playerCamera.transform.localPosition = new Vector3(0f, 70f, 0f);
            playerCamera.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
        }
    }

    [ServerCallback]
    void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.tag);
        if (other.GetComponent<Bullet>() != null)
        {
            HitMe(other.GetComponent<Bullet>().maxDamage);
        }
    }

    //При вызове триггера анимации, вызываем просмотр для всех Клиентов просмотр этой анимации
    [ClientRpc]
    void Dying_Unit()
    {
        anim.SetLayerWeight(1,0);
        rig.weight = 0;
        n_anim.SetTrigger("Dying");
    }

    //При вызове триггера анимации, вызываем просмотр для всех Клиентов просмотр этой анимации
    [ClientRpc]
    void RpcHitMe()
    {
        n_anim.SetTrigger("HitToBody");
    }

    void HitMe(int maxDamage)
    {
        Debug.Log("Hitme");
        RpcHitMe();
        PlayAudio(hitme_a, false);
        health = health - maxDamage;
        if (health < 0) health = 0;
        if (health == 0)
        {
            Dying_Unit();
            //Если здоровье меньше нуля удаляем объект с поля
            //NetworkServer.Destroy(gameObject); 
        }
}

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        source = GetComponent<AudioSource>();
        // spawnobject = GetComponent<SimpleSpawn>();
        //spawnobject = GameObject.Find("World").GetComponent<SpawnObjects>();

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void PlayAudio(AudioClip clip, bool loop)
    {
        source.loop = loop;
        source.clip = clip;
        source.Play();
    }


    void Run(float x, float y)
    {
        animationInterpolation = Mathf.Lerp(animationInterpolation, 1.5f, Time.deltaTime * 3);
        anim.SetFloat("x", x * animationInterpolation);
        anim.SetFloat("y", y * animationInterpolation);
    }
    void Walk(float x, float y)
    {
        // Mathf.Lerp - отвчает за то, чтобы каждый кадр число animationInterpolation(в данном случае) приближалось к числу 1 со скоростью Time.deltaTime * 3.
        // Time.deltaTime - это время между этим кадром и предыдущим кадром. Это позволяет плавно переходить с одного числа до второго НЕЗАВИСИМО ОТ КАДРОВ В СЕКУНДУ (FPS)!!!
        animationInterpolation = Mathf.Lerp(animationInterpolation, 1f, Time.deltaTime * 3);
        anim.SetFloat("x", x * animationInterpolation);
        anim.SetFloat("y", y * animationInterpolation);
    }

    void OnGUI()
    {
        //Output the current Animation name and length to the screen
        //GUI.Label(new Rect(0, 0, 200, 20), "Clip Name : " + m_ClipName);
        //GUI.Label(new Rect(0, 0, 200, 20), GetCurrentAnimatorByName("HitToBody").ToString());
        //GUI.Label(new Rect(0, 30, 200, 20), "Clip Length : " + m_CurrentClipLength);
    }

    private bool GetCurrentAnimatorByName(string nameAnimator)
    {
        //hitme_bool = anim.GetCurrentAnimatorStateInfo(0).IsName("HitToBody");
        m_CurrentClipInfo = this.anim.GetCurrentAnimatorClipInfo(0);
        //Длина клипа в миллисекундах
        m_CurrentClipLength = m_CurrentClipInfo[0].clip.length;
        //Имя клипа текущей анимации
        m_ClipName = m_CurrentClipInfo[0].clip.name;
        return (nameAnimator == m_ClipName);
    }

    void movePersonal()
    {
        bool isRunning = false;
        // Press Left Shift to run
        isRunning = Input.GetKey(KeyCode.LeftShift);

        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");


        // We are grounded, so recalculate move direction based on axis
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        float curSpeedX = canMove ? (isRunning ? runningSpeed : walkingSpeed) * y : 0;
        float curSpeedY = canMove ? (isRunning ? runningSpeed : walkingSpeed) * x : 0;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        //Проверяем если есть попадание в тело останавливаем все скоростив ноль
        if (GetCurrentAnimatorByName("HitToBody")||GetCurrentAnimatorByName("Dying")) moveDirection = new Vector3(0, 0, 0) ;


        anim.SetFloat("Speed",(Math.Max(Math.Abs(curSpeedX), Math.Abs(curSpeedY))));
        //anim.SetFloat("Speed",0f);

        if (Input.GetKey(KeyCode.W) && isRunning)
        {
            // Зажаты ли еще кнопки A S D?
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
            {
                //anim.SetFloat("Speed", 0.5f);
                // Если да, то мы идем пешком
                Walk(x, y);
            }
            // Если нет, то тогда бежим!
            else
            {
                //anim.SetFloat("Speed", 1f);
                Run(x, y);
            }
        }
        // Если W & Shift не зажаты, то мы просто идем пешком
        else
        {
            //anim.SetFloat("Speed", 0.5f);
            Walk(x, y);
        }

        //Проверяем анимацию попадания
        if (Input.GetKeyDown(KeyCode.Y)) HitMe(5);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            n_anim.SetTrigger("Jump");
        }


        if (Input.GetButtonDown("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpSpeed;
        }
        else
        {
            //anim.SetTrigger("Jump");
            moveDirection.y = movementDirectionY;
        }

        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        // Move the controller
        characterController.Move(moveDirection * Time.deltaTime);

        // Player and Camera rotation
        if (canMove && playerCamera != null)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);

            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
            //Head.transform.rotation = playerCamera.transform.rotation;
            //Riffle.transform.rotation = playerCamera.transform.rotation;

        }
        if (Input.GetKey(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void GroundedCheck()
    {
        // set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
            transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
            QueryTriggerInteraction.Ignore);
        //VerifyGravity.transform.position = spherePosition;
        // update animator if using character
        anim.SetBool("Grounded", Grounded);
    }

    private void OnLand(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            //AudioSource.PlayClipAtPoint(LandingAudioClip, transform.position, FootstepAudioVolume);
            PlayAudio(LandingAudioClip, false);
        }
    }

    void CheckRaycast()
    {
        RaycastHit hit;
        RaycastHit hitWeapon;
        RaycastHit hitUI;
        Physics.Raycast(startBullet.transform.position, startBullet.transform.forward, out hitWeapon, 1000,1);
        Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, 1000, 1);
        Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hitUI, 1000, 5);

        float distanceHit;
        Vector3 hitTrue;
        if (hit.transform != null)
        {
            if (debugDrawLine_Weapon) Debug.DrawLine(startBullet.transform.position, hitWeapon.point, Color.red);
            if (debugDrawLine) Debug.DrawLine(playerCamera.transform.position, hit.point, Color.yellow);
            Debug.DrawLine(startBullet.transform.position, hitUI.point, Color.white);
            hitTrue = hit.point;
            distanceHit = Vector3.Distance(playerCamera.transform.position, hit.point);
            RaycastSphere.transform.position = hit.point;
        }
        else
        {
           distanceHit = 1000f;
           hitTrue = playerCamera.transform.position + playerCamera.transform.forward * 1000;
           if (debugDrawLine) Debug.DrawLine(playerCamera.transform.position, playerCamera.transform.position + playerCamera.transform.forward * 1000, Color.green);
           RaycastSphere.transform.position = playerCamera.transform.position + playerCamera.transform.forward * 1000;
        }
    }

    void Update()
    {
        healthBar.text = health.ToString();
        if (!isLocalPlayer) return;
        GroundedCheck();
        if (!GetCurrentAnimatorByName("Dying")) movePersonal();
        CheckRaycast();
    }

}
