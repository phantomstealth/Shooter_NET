using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Runtime.ConstrainedExecution;
using UnityEngine.SceneManagement;
using static Unity.VisualScripting.Member;

public class P_Controller : NetworkBehaviour
{
    [Header("Stats")]
    [SyncVar] public int health = 4;

    [Header("Components")]
    public TextMesh healthBar;

    [Header("Base setup")]
    public float walkingSpeed = 7.5f;
    public float runningSpeed = 11.5f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 45.0f;
    
    [Header("Animation Object")]
    public GameObject Head;
    public GameObject Riffle;
    public GameObject bullet;
    public GameObject rocket;
    public Transform position_spawnbullet;

    [Header("Character Controller")]
    CharacterController characterController;
    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;

    [Header("Audio")]
    public AudioClip Shot;
    public AudioClip Walk;
    public AudioClip RocketLaunch;
    public AudioClip Hitme;

    [HideInInspector]
    public bool canMove = true;

    [SerializeField]
    private float cameraYOffset = 1.6f;
    [SerializeField]
    private float cameraZOffset = 0.35f;

    private Camera playerCamera;
    private GameObject canvasCross;
    private AudioSource source;

    // Start is called before the first frame update
    void Start()
    {
        source = GetComponent<AudioSource>();
        characterController = GetComponent<CharacterController>();
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

    void Awake()
    {
        playerCamera = Camera.main;
        //playerCamera.transform.position = new Vector3(transform.position.x, transform.position.y + cameraYOffset, transform.position.z);
        //playerCamera.transform.SetParent(transform);
    }

    public override void OnStartLocalPlayer()
    {
        if (playerCamera != null)
        {
            // configure and make camera a child of player with 3rd person offset
            playerCamera.transform.position = new Vector3(transform.position.x, transform.position.y + cameraYOffset, transform.position.z+ cameraZOffset);
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

    [Command]
        void Fire()
    {
        GameObject projectile = Instantiate(bullet, position_spawnbullet.position, position_spawnbullet.rotation);
        NetworkServer.Spawn(projectile);
        PlayAudio(Shot, false);
    }
    // Update is called once per frame
    [Command]
    void Fire_1()
    {
        GameObject projectile = Instantiate(rocket, position_spawnbullet.position, position_spawnbullet.rotation);
        NetworkServer.Spawn(projectile);
        PlayAudio(RocketLaunch, false);
    }

    [ServerCallback]
    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Bullet>() != null)
        {
            --health;
            PlayAudio(Hitme, false);
            if (health == 0)
                NetworkServer.Destroy(gameObject);
        }
    }


    void movePersonal()
    {
        bool isRunning = false;

        // Press Left Shift to run
        isRunning = Input.GetKey(KeyCode.LeftShift);

        // We are grounded, so recalculate move direction based on axis
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        float curSpeedX = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.Mouse0)) Fire();
        if (Input.GetKeyDown(KeyCode.RightControl) || Input.GetKeyDown(KeyCode.Mouse1)) Fire_1();

        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpSpeed;
        }
        else
        {
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
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
            Head.transform.rotation = playerCamera.transform.rotation;
            Riffle.transform.rotation = playerCamera.transform.rotation;
        }

    }
    void Update()
    {
        healthBar.text = new string('-', health);
        if (!isLocalPlayer) return;
        movePersonal();
    }
}
