using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class PlayerController : MonoBehaviour
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
	[SerializeField] float rotationSpeed = 1.0f;
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

    [Space(10)]
    [Header("Execution Scripts/Objects")]
    [Tooltip("The weapon container attached to the player.")]
    [SerializeField] GameObject weaponContainer;
    [Tooltip("The inventory object attached to the canvas.")]
    [SerializeField] GameObject inventoryObject;
    [Tooltip("The ObjectHandler script attached to the InventoryManager.")]
    [SerializeField] ObjectHandler objectHandler;
    // The weaponSwitcher script attached to the weapon container.
    WeaponSwitcher weaponSwitcher;

    Rigidbody rb;
    float lookRotation;
    float speed;
    bool crouchReleased = true;
    float startYScale;
    float startFocalLength;

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
        weaponSwitcher = weaponContainer.transform.GetComponent<WeaponSwitcher>();
        inventoryObject.SetActive(false);
        startYScale = transform.localScale.y;
        startFocalLength = cmVirtualCamera.m_Lens.FieldOfView;
    }

    void Update()
    {
        if (GameManager.Instance.CurrentGameState == GameManager.GameState.Gameplay)
        {
                                        Crouch();
            if (fire)                   Fire();
            else if (interact & !aim)   Interact();
            if (weaponSwitch != 0)      WeaponSwitch();
            if (reload)                 Reload();
            if (inventory)              Inventory();
            //if (menu)                   Menu();
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
        if (context.started && crouchReleased)
        {
            crouch = true;
            crouchReleased = false;
        }
        else if (context.started && !crouchReleased)
        {
            crouch = false;
            crouchReleased = true;
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

    ///////////////////////////////////////////////////////////////////////////////////////////////
    // Gameplay Functions
    ///////////////////////////////////////////////////////////////////////////////////////////////

    void Move()
    {
        if (sprint && !crouch) speed = sprintSpeed;
        else if (crouch) speed = crouchSpeed;
        else speed = moveSpeed;

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
        // Avoid scaling the player while in air
        if (!Physics.Raycast(transform.position, Vector3.down, 2f))
        {
            return;
        }

        if (crouch)
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
        }
        else if (!crouch)
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }

    void Aim()
    {
        // Increase camera FOV smoothly when aiming
        cmVirtualCamera.m_Lens.FieldOfView = Mathf.Lerp(cmVirtualCamera.m_Lens.FieldOfView, aim ? focalLength : startFocalLength, Time.deltaTime * 10f);
    }

    void Fire()
    {
        fire = false;

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
        Debug.Log("Player Controller: Interact");
        objectHandler.Execute();
    }

    void WeaponSwitch()
    {
        Debug.Log("Player Controller: Weapon Switch");
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
        GameManager.Instance.Inventory();
    }

    void Menu()
    {
        menu = false;

        if (GameManager.Instance.isPaused)
        {
            GameManager.Instance.ResumeGame();
        }
        else
        {
            GameManager.Instance.PauseGame();
        }
    }

    private void OnDrawGizmos() 
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, Vector3.down * 2f);
    }
}
