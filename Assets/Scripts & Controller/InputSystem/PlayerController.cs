using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class PlayerController : MonoBehaviour, ICustomUpdatable
{
    #region Movement Variables 

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
    [Tooltip("The distance the player will move up or down when crouching")]
    [SerializeField] float crouchYPosition = 1f;

    #endregion

    #region Movement Sounds

    [Space(10)]
    [Header("Movement Sounds")]
    [Tooltip("The pitch of the footstep sound")]
    [SerializeField] float normalPitch = 1f;
    [Tooltip("The volume of the footstep sound")]
    [SerializeField] float normalVolume = 1f;
    [Tooltip("The pitch of the footstep sound while sprinting")]
    [SerializeField] float sprintPitch = 1.25f;
    [Tooltip("The volume of the footstep sound while sprinting")]
    [SerializeField] float sprintVolume = 1.1f;
    [Tooltip("The pitch of the footstep sound while crouching")]
    [SerializeField] float crouchPitch = 0.75f;
    [Tooltip("The volume of the footstep sound while crouching")]
    [SerializeField] float crouchVolume = 0.9f;

    #endregion

    #region Cinemachine Variables

    [Space(10)]
    [Header("Cinemachine")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    [SerializeField] GameObject camRoot;
    [Tooltip("How far in degrees can you move the camera up")]
    [SerializeField] float topClamp = 90.0f;
    [Tooltip("How far in degrees can you move the camera down")]
    [SerializeField] float bottomClamp = -90.0f;
    [Tooltip("The player follow camera set in Cinemachine Brain")]
    public CinemachineVirtualCamera cmVirtualCamera;
    [Tooltip("Additional degrees to override the camera when aiming.")]
    [SerializeField] float focalLength = 33f;
    [Tooltip("The default target for LookAt")]
    [SerializeField] Transform defaultTarget;
    [Tooltip("Mesh of the Arms")]
    [SerializeField] Renderer fpsArms;
    [Tooltip("Flashlight attached to the player")]
    [SerializeField] GameObject flashlight;

    #endregion

    #region Player Components and Input

    // The weapon container attached to the player.
    GameObject weaponContainer;
    // The ObjectHandler script attached to the InventoryManager.
    ObjectHandler objectHandler;
    // The weaponSwitcher script attached to the weapon container.
    WeaponSwitcher weaponSwitcher;
    Player player;
    // Canvas used for showing items that are picked up.
    [SerializeField] GameObject pickupCanvas;

    float standardSpotAngle = 50f;
    float newSpotAngle = 30f;
    float standardIntensity = 1f;
    float newIntensity = 1.5f;

    Rigidbody rb;
    float lookRotation;
    float speed;
    float startYPosition;
    float startFocalLength;
    bool isOutOfStamina = false;
    bool isDizzy = false;

    #endregion

    #region Player Input Variables

    // Player Input
    Vector2 move, look;
    bool sprint, crouch, aim, fire, interact, reload, inventory, menu, lightVar;
    float weaponSwitch;

    #endregion

    #region Event Functions

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        player = GetComponent<Player>();
        weaponContainer = player.transform.GetChild(0).GetChild(0).gameObject;
        weaponSwitcher = weaponContainer.transform.GetComponentInChildren<WeaponSwitcher>();
        weaponContainer = weaponSwitcher.gameObject;
        objectHandler = InventoryManager.Instance.GetComponent<ObjectHandler>();

        startYPosition = camRoot.transform.localPosition.y;
        startFocalLength = cmVirtualCamera.m_Lens.FieldOfView;
    }

    public void CustomUpdate(float deltaTime)
    {
        if (GameManager.Instance.CurrentGameState == GameManager.GameState.Gameplay)
        {
            Move();
            if (fire) Fire();
            else if (interact && !aim) Interact();
            if (weaponSwitch != 0) WeaponSwitch();
            if (reload) Reload();
            if (inventory) Inventory();
            if (menu) Menu();
            if (lightVar) Light();
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

    #endregion


    #region Camera Functions

    // Set this function to be called when you want to make the camera look at a specific direction
    public void LookAtDirection(Transform direction)
    {
        cmVirtualCamera.LookAt = direction;
        player.transform.LookAt(direction, Vector3.up);
    }

    // Set this function to be called when you want to reset the camera to look at the default target
    public void LookAtReset()
    {
        cmVirtualCamera.LookAt = defaultTarget;
        player.transform.LookAt(defaultTarget, Vector3.up);
    }

    public void SetFollowTarget(Transform target = null) 
    {
        if (target == null)
        {
            target = camRoot.transform;
            cmVirtualCamera.Follow = target;
            //LookAtReset();
            Debug.Log("Follow target is null, setting to camRoot");
        }
        else
        {
            cmVirtualCamera.Follow = target;
            LookAtDirection(null);
        }
        
        Debug.Log("Setting follow target to " + target.name);
    }

    public void ToggleArms(bool active)
    {
        fpsArms.enabled = active;
        if (flashlight.activeSelf)
        {
            flashlight.GetComponent<Renderer>().enabled = active;
        }
        Transform[] weapons = weaponContainer.GetComponentsInChildren<Transform>();
        foreach (Transform weapon in weapons)
        {
            Renderer renderer = weapon.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.enabled = active;
            }
            else
            {
                Renderer[] renderers = weapon.GetComponentsInChildren<Renderer>();
                foreach (Renderer r in renderers)
                {
                    r.enabled = active;
                }
            }
        }
    }

    #endregion


    #region Input Functions

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
        if (aim) interact = false;
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
        lightVar = context.ReadValueAsButton();
    }

    #endregion

    #region GameManager Functions

    public void GamePlayEvent()
    {
        // Disable player movement
        if (crouch)
        {
            crouch = false;
            Crouch();
        }
        sprint = false;
        aim = false;
        fire = false;
        interact = false;
        reload = false;
        lightVar = false;
    }

    public void MoveSpeedChange(float speedChangeRate, float duration)
    {
        float moveSpeedSave = moveSpeed;
        float sprintSpeedSave = sprintSpeed;
        float crouchSpeedSave = crouchSpeed;

        moveSpeed *= speedChangeRate;
        sprintSpeed *= speedChangeRate;
        crouchSpeed *= speedChangeRate;

        StartCoroutine(ResetMoveSpeed(moveSpeedSave, sprintSpeedSave, crouchSpeedSave, duration));
    }

    IEnumerator ResetMoveSpeed(float moveSpeedSave, float sprintSpeedSave, float crouchSpeedSave, float duration)
    {
        yield return new WaitForSeconds(duration);
        moveSpeed = moveSpeedSave;
        sprintSpeed = sprintSpeedSave;
        crouchSpeed = crouchSpeedSave;
    }

    #endregion

    #region Gameplay Functions

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
        int audioSourceID = gameObject.GetInstanceID();

        if (sprint && !crouch && !player.isOutOfStamina)
        {
            speed = sprintSpeed;
            if (NoiseManager.Instance.noiseData.runModifier != NoiseManager.Instance.noiseLevelMultiplier)
            {
                NoiseManager.Instance.noiseLevelMultiplier = NoiseManager.Instance.noiseData.runModifier;
            }
            player.ReduceStamina();

            AudioManager.Instance.SetVolumeOverTime(audioSourceID, sprintVolume, 0.1f);
            AudioManager.Instance.SetAudioPitch(audioSourceID, sprintPitch);
        }
        else if (crouch)
        {
            speed = crouchSpeed;
            if (NoiseManager.Instance.noiseData.crouchModifier != NoiseManager.Instance.noiseLevelMultiplier)
            {
                NoiseManager.Instance.noiseLevelMultiplier = NoiseManager.Instance.noiseData.crouchModifier;
            }
            player.IncreaseStamina();

            AudioManager.Instance.SetVolumeOverTime(audioSourceID, crouchVolume, 0.1f);
            AudioManager.Instance.SetAudioPitch(audioSourceID, crouchPitch);
        }
        else
        {
            speed = moveSpeed;
            if (NoiseManager.Instance.noiseData.walkModifier != NoiseManager.Instance.noiseLevelMultiplier)
            {
                NoiseManager.Instance.noiseLevelMultiplier = NoiseManager.Instance.noiseData.walkModifier;
            }
            player.IncreaseStamina();

            AudioManager.Instance.SetVolumeOverTime(audioSourceID, normalVolume, 0.1f);
            AudioManager.Instance.SetAudioPitch(audioSourceID, normalPitch);
        }

        // Find target velocity
        Vector3 currentVelocity = rb.velocity;
        Vector3 targetVelocity = new Vector3(move.x, 0, move.y) * speed;

        // Align direction
        targetVelocity = transform.TransformDirection(targetVelocity);

        // Calculate forces
        Vector3 velocityChange = targetVelocity - currentVelocity;
        velocityChange = new Vector3(velocityChange.x, 0, velocityChange.z);

        if (rb.velocity.magnitude < 0.1f)
        {
            AudioManager.Instance.StopAudio(audioSourceID);
        }
        else
        {
            if (!AudioManager.Instance.IsPlaying(audioSourceID))
            {
                AudioManager.Instance.PlayAudio(audioSourceID);
            }
        }

        // Limit Force
        Vector3.ClampMagnitude(velocityChange, maxForce);

        // Move rigidbody with acceleration instead of force)
        rb.AddForce(velocityChange * SpeedChangeRate, ForceMode.Acceleration);
        transform.GetComponent<NoiseController>().UpdateNoiseLevel(rb.velocity.magnitude);
    }

    public void PushPlayerBack(float directionBack = 0.5f, float force = 10f)
    {
        Vector3 direction = new Vector3(directionBack, 0, 0);
        rb.AddForce(direction * force, ForceMode.Impulse);
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
        rotationSpeed = standardRotationSpeed;

        // Turn
        transform.Rotate(Vector3.up * look.x * rotationSpeed * Time.deltaTime);

        // Look
        lookRotation -= look.y * rotationSpeed * Time.deltaTime;
        lookRotation = Mathf.Clamp(lookRotation, bottomClamp, topClamp);
        camRoot.transform.eulerAngles = new Vector3(lookRotation, camRoot.transform.eulerAngles.y, camRoot.transform.eulerAngles.z);
    }

    void Crouch()
    {
        float targetCrouchY = crouch ? startYPosition - crouchYPosition : startYPosition;

        // Calculate the target position
        Vector3 targetPosition = new Vector3(camRoot.transform.localPosition.x, targetCrouchY, camRoot.transform.localPosition.z);

        // Set the camRoot's position to the target crouch position
        camRoot.transform.localPosition = targetPosition;
    }

    void Aim()
    {
        GameManager.Instance.playerAnimController.AimAnimation(aim);

        // Increase camera FOV smoothly when aiming
        cmVirtualCamera.m_Lens.FieldOfView = Mathf.Lerp(cmVirtualCamera.m_Lens.FieldOfView, aim ? focalLength : startFocalLength, Time.deltaTime * 10f);

        Light flashlight = GetComponent<Player>().flashlight;
        if (flashlight.enabled)
        {
            flashlight.spotAngle = Mathf.Lerp(flashlight.spotAngle, aim ? newSpotAngle : standardSpotAngle, Time.deltaTime * 10f);
            flashlight.intensity = Mathf.Lerp(flashlight.intensity, aim ? newIntensity : standardIntensity, Time.deltaTime * 10f);
        }
    }

    void Fire()
    {
        fire = false;

        // loop through weapons and execute the Weapon script that is on the active object
        foreach (Transform child in weaponContainer.transform)
        {
            if (child.gameObject.activeSelf)
            {
                Weapon weapon = child.GetComponent<Weapon>();
                weapon.Execute(weapon);
                return;
            }
        }
    }

    void Interact()
    {
        interact = false;
        if (GameManager.Instance.pickupCanvas.activeSelf || GameManager.Instance.canvasActive)
        {
            GameManager.Instance.ResumeGame();
            GameManager.Instance.canvasActive = false;
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
        lightVar = false;
        GameManager.Instance.player.LightSwitch();
    }

    #endregion

    #region UI Functions

    private void OnDrawGizmos() 
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, Vector3.down * 2f);
    }

    #endregion
}
