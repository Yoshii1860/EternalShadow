using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class PlayerController : MonoBehaviour, ICustomUpdatable
{

    ///////////////////////////////////////////////////////////////////////////////////////////////
    // Variables
    ///////////////////////////////////////////////////////////////////////////////////////////////

    [Header("Movement")]
	[Tooltip("Move speed of the character in m/s")]
	[SerializeField] float moveSpeed = 4.0f;
    [Tooltip("Maximum force applied when moving in m/s")]
    [SerializeField] float maxForce = 1f;
	[Tooltip("Sprint speed of the character in m/s")]
	[SerializeField] float sprintSpeed = 6.0f;
	[Tooltip("Crouch speed of the character in m/s")]
	[SerializeField] float crouchSpeed = 0.6f;
	[Tooltip("Rotation speed of the character")]
	public float rotationSpeed = 1.0f;
    [Tooltip("Rotation speed of the character during Gameplay")]
    public float standardRotationSpeed = 1.0f;
	[Tooltip("Acceleration and deceleration")]
	[SerializeField] float SpeedChangeRate = 10.0f;
	[Tooltip("height of the camera while in crouch state")]
	[SerializeField] float crouchYScale = 0.5f;

    [Space(10)]
    [Header("Cinemachine")]
	[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    [SerializeField] GameObject camRoot;
    [Tooltip("How far in degrees can you move the camera up")]
	[SerializeField] float topClamp = 90.0f;
	[Tooltip("How far in degrees can you move the camera down")]
	[SerializeField] float bottomClamp = -90.0f;
    [Tooltip("The the player follow camera set in Cinemachine Brain")]

    [Space(10)]
	[SerializeField] CinemachineVirtualCamera cmVirtualCamera;
    [Tooltip("Additional degress to override the camera when aiming.")]
	[SerializeField] float focalLength = 33f;
    [Tooltip("The default target for LookAt")]
    [SerializeField] Transform defaultTarget;

    // The weapon container attached to the player.
    GameObject weaponContainer;
    // The ObjectHandler script attached to the InventoryManager.
    ObjectHandler objectHandler;
    // The weaponSwitcher script attached to the weapon container.
    WeaponSwitcher weaponSwitcher;
    Player player;
    // Canvas used for showing items that are picked up.
    [SerializeField] GameObject pickupCanvas;

    Rigidbody rb;
    float lookRotation;
    float speed;
    bool crouchReleased = true;
    float startYScale;
    float startFocalLength;
    bool isOutOfStamina = false;
    bool isDizzy = false;

    // Player Input
    Vector2 move, look;
    bool sprint, crouch, aim, fire, interact, reload, inventory, menu;
    float weaponSwitch;

    ///////////////////////////////////////////////////////////////////////////////////////////////
    // Event Functions
    ///////////////////////////////////////////////////////////////////////////////////////////////

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        player = GetComponent<Player>();
        weaponContainer = player.transform.GetChild(0).GetChild(0).gameObject;
        weaponSwitcher = weaponContainer.transform.GetComponent<WeaponSwitcher>();
        objectHandler = InventoryManager.Instance.GetComponent<ObjectHandler>();

        startYScale = transform.localScale.y;
        startFocalLength = cmVirtualCamera.m_Lens.FieldOfView;
    }

    public void CustomUpdate(float deltaTime)
    {
        if (GameManager.Instance.CurrentGameState == GameManager.GameState.Gameplay)
        {
            if (fire)                   Fire();
            else if (interact && !aim)  Interact();
            if (!aim)                   GameManager.Instance.playerAnimController.AimAnimation(false);
            if (weaponSwitch != 0)      WeaponSwitch();
            if (reload)                 Reload();
            if (inventory)              Inventory();
            if (menu)                   Menu();
        }
    }

    void FixedUpdate() 
    {
        if (GameManager.Instance.CurrentGameState == GameManager.GameState.Gameplay)
        {
            Move();            
        }
    }

    private void LateUpdate() 
    {
        if (GameManager.Instance.CurrentGameState == GameManager.GameState.Gameplay)
        {
            Look();
            Aim();
        }
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////
    // Camera Functions
    ///////////////////////////////////////////////////////////////////////////////////////////////

    // Set this function to be called when you want to make the camera look at a specific direction
    public void LookAtDirection(Transform direction)
    {
        cmVirtualCamera.LookAt = direction;
    }

    // Set this function to be called when you want to make the camera look at a specific direction
    public void LookAtReset()
    {
        transform.rotation = cmVirtualCamera.transform.rotation;
        cmVirtualCamera.LookAt = defaultTarget;
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////
    // Input Functions
    ///////////////////////////////////////////////////////////////////////////////////////////////

    public void OnMove(InputAction.CallbackContext context)
    {
        move = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        look = context.ReadValue<Vector2>();
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        sprint = context.ReadValueAsButton();
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            crouch = !crouch;
            Crouch();
        }
    }

    public void OnAim(InputAction.CallbackContext context)
    {
        aim = context.ReadValueAsButton();
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        fire = context.ReadValueAsButton();
    }


    public void OnInteract(InputAction.CallbackContext context)
    {
        interact = context.ReadValueAsButton();
    }

    public void OnWeaponSwitch(InputAction.CallbackContext context)
    {
        weaponSwitch = context.ReadValue<float>();
    }

    public void OnReload(InputAction.CallbackContext context)
    {
        reload = context.ReadValueAsButton();
    }

    public void OnInventory(InputAction.CallbackContext context)
    {
        inventory = context.ReadValueAsButton();
    }

    public void OnMenu(InputAction.CallbackContext context)
    {
        menu = context.ReadValueAsButton();
    }

    public void OnLight(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Light();
        }
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////
    // Gameplay Functions
    ///////////////////////////////////////////////////////////////////////////////////////////////

    void Move()
    {
        // Make Player move slower for 3 seconds
        if (player.isOutOfStamina && !isOutOfStamina)   
        {
            MoveOutOfStamina();
        }
        // Make Player stop once every 3 minutes
        else if (player.isDizzy)
        {
            MoveDizzy();
            MoveDefault();
        }
        else
        {   
            MoveDefault();
        }
    }

    void MoveDefault()
    {
        if (sprint && !crouch && !player.isOutOfStamina) 
        {
            speed = sprintSpeed;
            if (NoiseManager.Instance.noiseData.runModifier != NoiseManager.Instance.noiseLevelMultiplier)
            {
                NoiseManager.Instance.noiseLevelMultiplier = NoiseManager.Instance.noiseData.runModifier;
            }
            player.ReduceStamina();
        }
        else if (crouch) 
        {
            speed = crouchSpeed;
            if (NoiseManager.Instance.noiseData.crouchModifier != NoiseManager.Instance.noiseLevelMultiplier)
            {
                NoiseManager.Instance.noiseLevelMultiplier = NoiseManager.Instance.noiseData.crouchModifier;
            }
            player.IncreaseStamina();
        }
        else 
        {
            speed = moveSpeed;
            if (NoiseManager.Instance.noiseData.walkModifier != NoiseManager.Instance.noiseLevelMultiplier)
            {
                NoiseManager.Instance.noiseLevelMultiplier = NoiseManager.Instance.noiseData.walkModifier;
            }
            player.IncreaseStamina();
        }

        // Find target velocity
        Vector3 currentVelocity = rb.velocity;
        Vector3 targetVelocity = new Vector3(move.x, 0, move.y) * speed;

        // Allign direction
        targetVelocity = transform.TransformDirection(targetVelocity);

        // Calculate forces
        Vector3 velocityChange = targetVelocity - currentVelocity;
        velocityChange = new Vector3(velocityChange.x, 0, velocityChange.z);

        // Limit Force
        Vector3.ClampMagnitude(velocityChange, maxForce);

        // Move rigidbody with acceleration instead of force
        rb.AddForce(velocityChange * SpeedChangeRate, ForceMode.Acceleration);
    }

    void MoveOutOfStamina()
    {
        Debug.Log("Out of Stamina!");
        isOutOfStamina = true;
        // Disable sprint and slow down movement for 3 seconds
        sprint = false;
        moveSpeed *= 0.5f;

        // Timer to reset the isOutOfStamina state after 3 seconds
        StartCoroutine(ResetOutOfStamina());
    }

    IEnumerator ResetOutOfStamina()
    {
        Debug.Log("Resetting Out of Stamina");
        yield return new WaitForSeconds(3f);
        moveSpeed *= 2f;
        player.isOutOfStamina = false;
        isOutOfStamina = false;
    }

    void MovePoisoned()
    {
        player.isPoisoned = true;
    }

    void MoveDizzy()
    {
        // Randomly slow down the player by 80% once every 3 minutes
        if (Time.time % 120f < Time.fixedDeltaTime && !isDizzy)
        {
            Debug.Log("Dizzy! Stop moving!");
            moveSpeed = 1f;
            sprintSpeed = 1.5f;
            isDizzy = true;
        }
        // Reset the speed to default when not dizzy
        else if (isDizzy)
        {
            StartCoroutine(Dizzy());
        }
        else if (!isDizzy)
        {
            moveSpeed = 4f;
            sprintSpeed = 7f;
        }
    }

    IEnumerator Dizzy()
    {
        yield return new WaitForSeconds(3f);
        isDizzy = false;
        Debug.Log("Not Dizzy anymore!");
    }

    void Look()
    {
        // Turn
        transform.Rotate(Vector3.up * look.x * rotationSpeed * Time.deltaTime);

        // Look
        lookRotation -= look.y * rotationSpeed * Time.deltaTime;
        lookRotation = Mathf.Clamp(lookRotation, bottomClamp, topClamp);
        camRoot.transform.eulerAngles = new Vector3(lookRotation, camRoot.transform.eulerAngles.y, camRoot.transform.eulerAngles.z);
    }

    void Crouch()
    {
        /* Avoid scaling the player while in air
        if (!Physics.Raycast(transform.position, Vector3.down, 3f))
        {
            Debug.Log("Crouch: Not on ground");
            return;
        }
        */

        if (crouch)
        {
            Debug.Log("Crouch: crouch");
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
        }
        else if (!crouch)
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }

    void Aim()
    {
        GameManager.Instance.playerAnimController.AimAnimation(true);

        // Increase camera FOV smoothly when aiming
        cmVirtualCamera.m_Lens.FieldOfView = Mathf.Lerp(cmVirtualCamera.m_Lens.FieldOfView, aim ? focalLength : startFocalLength, Time.deltaTime * 10f);
    }

    void Fire()
    {
        fire = false;

        GameManager.Instance.playerAnimController.ShootAnimation();

        // loop through weapons and execute the Weapon script that is on the active object
        foreach (Transform child in weaponContainer.transform)
        {
            if (child.gameObject.activeSelf)
            {
                child.GetComponent<Weapon>().Execute();
            }
        }
    }

    void Interact()
    {
        interact = false;
        if (GameManager.Instance.pickupCanvas.activeSelf)
        {
            GameManager.Instance.ResumeGame();
        }
        else
        {
            objectHandler.Execute();
        }
    }

    void WeaponSwitch()
    {
        weaponSwitcher.Execute(weaponSwitch);
    }

    void Reload()
    {
        reload = false;

        // loop through weapons and execute the Weapon script that is on the active object
        foreach (Transform child in weaponContainer.transform)
        {
            if (child.gameObject.activeSelf)
            {
                child.GetComponent<Weapon>().ExecuteReload();
            }
        }
    }

    void Inventory()
    {
        inventory = false;

        if (GameManager.Instance.player.GetComponent<InventoryController>().inventoryClosing)
        {
            return;
        }
        GameManager.Instance.Inventory();
    }

    void Menu()
    {
        menu = false;
        GameManager.Instance.OpenMenu();
    }

    void Light()
    {
        GameManager.Instance.player.LightSwitch();
    }

    private void OnDrawGizmos() 
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, Vector3.down * 2f);
    }
}
