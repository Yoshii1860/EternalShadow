using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class PlayerController : MonoBehaviour, ICustomUpdatable
{
    #region Movement Variables 

    [Header("Movement")]
    [Tooltip("Move movement speed of the character in m/s")]
    [SerializeField] private float _movementSpeed = 4.0f;
    [Tooltip("Maximum force applied when moving in m/s")]
    [SerializeField] private float _maxMovementForce = 1f;
    [Tooltip("Sprint movement speed of the character in m/s")]
    [SerializeField] private float _sprintMovementSpeed = 6.0f;
    [Tooltip("Crouch movement speed of the character in m/s")]
    [SerializeField] private float _crouchMovementSpeed = 0.6f;
    [Tooltip("Rotation movement speed of the character")]
    public float RotationSpeed = 5.0f;
    [Tooltip("Rotation movement speed of the character during Gameplay")]
    public float BaseRotationSpeed = 5.0f;
    [Tooltip("Rotation movement speed of the character when using the controller")]
    public float ControllerRotationSpeed = 250.0f;
    [Tooltip("Acceleration and deceleration")]
    [SerializeField] private float _acceleration = 10.0f;
    [Tooltip("The distance the player will move up or down when crouching")]
    [SerializeField] private float _crouchOffset = 1f;

    #endregion




    #region Movement Sounds

    [Space(10)]
    [Header("Movement Sounds")]
    [Tooltip("The pitch of the footstep sound")]
    [SerializeField] private float _walkPitch = 1f;
    [Tooltip("The volume of the footstep sound")]
    [SerializeField] private float _walkVolume = 1f;
    [Tooltip("The pitch of the footstep sound while sprinting")]
    [SerializeField] private float _sprintPitch = 1.25f;
    [Tooltip("The volume of the footstep sound while sprinting")]
    [SerializeField] private float _sprintVolume = 1.1f;
    [Tooltip("The pitch of the footstep sound while crouching")]
    [SerializeField] private float _crouchPitch = 0.75f;
    [Tooltip("The volume of the footstep sound while crouching")]
    [SerializeField] private float _crouchVolume = 0.9f;

    #endregion




    #region Cinemachine Variables

    [Space(10)]
    [Header("Cinemachine")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    [SerializeField] private GameObject _camRoot;
    [Tooltip("How far in degrees can you move the camera up")]
    [SerializeField] private float _topClamp = 90.0f;
    [Tooltip("How far in degrees can you move the camera down")]
    [SerializeField] private float _bottomClamp = -90.0f;
    [Tooltip("The player follow camera set in Cinemachine Brain")]
    public CinemachineVirtualCamera CMVirtualCamera;
    [Tooltip("Additional degrees to override the camera when aiming.")]
    [SerializeField] private float _focalLength = 33f;
    [Tooltip("The default target for LookAt")]
    [SerializeField] private Transform _defaultTarget;
    [Tooltip("Mesh of the Arms")]
    [SerializeField] private Renderer _fpsArmsRenderer;
    [Tooltip("Flashlight attached to the player")]
    [SerializeField] private GameObject _flashlightObject;

    #endregion




    #region Player Components and Input

    // The weapon container attached to the player.
    private GameObject _weaponContainer;
    // The ObjectHandler script attached to the InventoryManager.
    private ObjectHandler _objectHandler;
    // The weaponSwitcher script attached to the weapon container.
    private WeaponSwitcher _weaponSwitcher;
    private Player _playerScript;

    // Default and focused flashlight angles and intensities
    private float _defaultFlashlightAngle = 50f;
    private float _focusedFlashlightAngle = 30f;
    private float _defaultFlashlightIntensity = 1f;
    private float _focusedFlashlightIntensity = 1.5f;

    // The player's rigidbody component
    private Rigidbody _rb;

    // player variables
    private float _playerLookRotation;
    private float _currentMovementSpeed;
    private float _startYPosition;
    private float _startFocalLength;

    // Movement states
    private bool _isOutOfStamina = false;
    private bool _isDizzy = false;

    #endregion




    #region Player Input Variables

    // Player Input
    private Vector2 _movePos, _lookPos;
    private bool _isSprinting, _isCrouching, _isAiming, _isFiring, _isInteracting, _isReloading, _isOpeningInventory, _isOpeningMenu, _isUsingFlashlight;
    private float _weaponScrollValue;

    #endregion




    #region Unity Lifecycle Methods

    // Start is called before the first frame update
    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _playerScript = GetComponent<Player>();
        _weaponContainer = _playerScript.transform.GetChild(0).GetChild(0).gameObject;
        _weaponSwitcher = _weaponContainer.transform.GetComponentInChildren<WeaponSwitcher>();
        _weaponContainer = _weaponSwitcher.gameObject;
        _objectHandler = InventoryManager.Instance.GetComponent<ObjectHandler>();

        _startYPosition = _camRoot.transform.localPosition.y;
        _startFocalLength = CMVirtualCamera.m_Lens.FieldOfView;

        if (_camRoot == null) _camRoot = transform.GetChild(0).gameObject;
        if (_camRoot == null) Debug.LogError("No camRoot found!");
    }

    // Update is called once per frame
    private void LateUpdate()
    {
        if (GameManager.Instance.CurrentGameState == GameManager.GameState.GAMEPLAY)
        {
            Look();
            Aim();
        }
    }

    #endregion




    #region Custom Update

    // Custom Update method to call player movement and actions
    public void CustomUpdate(float deltaTime)
    {
        if (GameManager.Instance.CurrentGameState == GameManager.GameState.GAMEPLAY)
        {
            Move();
            if (_isFiring) Fire();
            else if (_isInteracting && !_isAiming) Interact();
            if (_weaponScrollValue != 0) WeaponSwitch();
            if (_isReloading) Reload();
            if (_isOpeningInventory) Inventory();
            if (_isOpeningMenu) Menu();
            if (_isUsingFlashlight) Light();
        }
    }

    #endregion




    #region Camera Functions

    // Set this function to be called when you want to make the camera look at a specific direction
    public void LookAtDirection(Transform direction)
    {
        CMVirtualCamera.LookAt = direction;
        _playerScript.transform.LookAt(direction, Vector3.up);
    }

    // Set this function to be called when you want to reset the camera to look at the default target
    public void LookAtReset()
    {
        CMVirtualCamera.LookAt = _defaultTarget;
        _playerScript.transform.LookAt(_defaultTarget, Vector3.up);
    }

    public void SetFollowTarget(Transform target = null) 
    {
        if (target == null)
        {
            target = _camRoot.transform;
            CMVirtualCamera.Follow = target;
            //LookAtReset();
            Debug.Log("Follow target is null, setting to camRoot");
        }
        else
        {
            CMVirtualCamera.Follow = target;
            LookAtDirection(null);
        }
        
        Debug.Log("Setting follow target to " + target.name);
    }

    public void ToggleArms(bool active)
    {
        _fpsArmsRenderer.enabled = active;
        if (_flashlightObject.activeSelf)
        {
            _flashlightObject.GetComponent<Renderer>().enabled = active;
        }
        Transform[] weapons = _weaponContainer.GetComponentsInChildren<Transform>();
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
        _movePos = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        _lookPos = context.ReadValue<Vector2>();
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        _isSprinting = context.ReadValueAsButton();
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _isCrouching = !_isCrouching;
            Crouch();
        }
    }

    public void OnAim(InputAction.CallbackContext context)
    {
        _isAiming = context.ReadValueAsButton();
        if (_isAiming) _isInteracting = false;
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        _isFiring = context.ReadValueAsButton();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        _isInteracting = context.ReadValueAsButton();
    }

    public void OnWeaponSwitch(InputAction.CallbackContext context)
    {
        _weaponScrollValue = context.ReadValue<float>();
    }

    public void OnReload(InputAction.CallbackContext context)
    {
        _isReloading = context.ReadValueAsButton();
    }

    public void OnInventory(InputAction.CallbackContext context)
    {
        _isOpeningInventory = context.ReadValueAsButton();
    }

    public void OnMenu(InputAction.CallbackContext context)
    {
        _isOpeningMenu = context.ReadValueAsButton();
    }

    public void OnLight(InputAction.CallbackContext context)
    {
        _isUsingFlashlight = context.ReadValueAsButton();
    }

    #endregion




    #region GameManager Functions

    public void GamePlayEvent()
    {
        // Disable player movement
        if (_isCrouching)
        {
            _isCrouching = false;
            Crouch();
        }
        _isSprinting = false;
        _isAiming = false;
        _isFiring = false;
        _isInteracting = false;
        _isReloading = false;
        _isUsingFlashlight = false;
    }

    public void MoveSpeedChange(float speedChangeRate, float duration)
    {
        float moveSpeedSave = _movementSpeed;
        float sprintSpeedSave = _sprintMovementSpeed;
        float crouchSpeedSave = _crouchMovementSpeed;

        _movementSpeed *= speedChangeRate;
        _sprintMovementSpeed *= speedChangeRate;
        _crouchMovementSpeed *= speedChangeRate;

        StartCoroutine(ResetMoveSpeed(moveSpeedSave, sprintSpeedSave, crouchSpeedSave, duration));
    }

    private IEnumerator ResetMoveSpeed(float moveSpeedSave, float sprintSpeedSave, float crouchSpeedSave, float duration)
    {
        yield return new WaitForSeconds(duration);
        _movementSpeed = moveSpeedSave;
        _sprintMovementSpeed = sprintSpeedSave;
        _crouchMovementSpeed = crouchSpeedSave;
    }

    public void PushPlayerBack(float directionBack = 0.5f, float force = 10f)
    {
        Vector3 direction = new Vector3(directionBack, 0, 0);
        _rb.AddForce(direction * force, ForceMode.Impulse);
    }

    public void SetInputDevice(int deviceChoice)
    {
        float value = PlayerPrefs.GetFloat("Sensitivity", 0);

        if (deviceChoice == 0) 
        {
            float sensitivity = BaseRotationSpeed * (1f + value);
            RotationSpeed = sensitivity;
        }
        else 
        {
            float sensitivity = ControllerRotationSpeed * (1f + value);
            RotationSpeed = sensitivity;
        }
    }

    #endregion




    #region Input Processing Methods

    private void Move()
    {
        // Make Player move slower for 3 seconds
        if (_playerScript.IsOutOfStamina && !_isOutOfStamina)
        {
            MoveOutOfStamina();
        }
        // Make Player stop once every 3 minutes
        else if (_playerScript.IsDizzy)
        {
            MoveDizzy();
            MoveDefault();
        }
        else
        {
            MoveDefault();
        }
    }

    private void MoveDefault()
    {
        int audioSourceID = gameObject.GetInstanceID();

        SetMovementSpeedAndNoise();

        // Find target velocity
        Vector3 currentVelocity = _rb.velocity;
        Vector3 targetVelocity = new Vector3(_movePos.x, 0, _movePos.y) * _currentMovementSpeed;

        // Align direction
        targetVelocity = transform.TransformDirection(targetVelocity);

        // Calculate forces
        Vector3 velocityChange = targetVelocity - currentVelocity;
        velocityChange = new Vector3(velocityChange.x, 0, velocityChange.z);

        // Stop audio if player is not moving
        if (_rb.velocity.magnitude < 0.1f)
        {
            AudioManager.Instance.StopAudio(audioSourceID);
        }
        // Play audio if player is moving
        else
        {
            if (!AudioManager.Instance.IsPlaying(audioSourceID))
            {
                AudioManager.Instance.PlayAudio(audioSourceID);
            }
        }

        // Limit Force
        Vector3.ClampMagnitude(velocityChange, _maxMovementForce);

        // Move rigidbody with acceleration instead of force)
        _rb.AddForce(velocityChange * _acceleration, ForceMode.Acceleration);
        transform.GetComponent<NoiseController>().UpdateNoiseLevel(_rb.velocity.magnitude);
    }

    // Set the movement speed and noise level based on the player's current state
    private void SetMovementSpeedAndNoise()
    {
        int audioSourceID = gameObject.GetInstanceID();

        // SPRINT
        if (_isSprinting && !_isCrouching && !_playerScript.IsOutOfStamina)
        {
            _currentMovementSpeed = _sprintMovementSpeed;
            if (NoiseManager.Instance.NoiseData.RunModifier != NoiseManager.Instance.NoiseLevelMultiplier)
            {
                NoiseManager.Instance.NoiseLevelMultiplier = NoiseManager.Instance.NoiseData.RunModifier;
            }
            _playerScript.ReduceStamina();

            AudioManager.Instance.SetVolumeOverTime(audioSourceID, _sprintVolume, 0.1f);
            AudioManager.Instance.SetAudioPitch(audioSourceID, _sprintPitch);
        }
        // CROUCH
        else if (_isCrouching)
        {
            _currentMovementSpeed = _crouchMovementSpeed;
            if (NoiseManager.Instance.NoiseData.CrouchModifier != NoiseManager.Instance.NoiseLevelMultiplier)
            {
                NoiseManager.Instance.NoiseLevelMultiplier = NoiseManager.Instance.NoiseData.CrouchModifier;
            }
            _playerScript.IncreaseStamina();

            AudioManager.Instance.SetVolumeOverTime(audioSourceID, _crouchVolume, 0.1f);
            AudioManager.Instance.SetAudioPitch(audioSourceID, _crouchPitch);
        }
        // WALK
        else
        {
            _currentMovementSpeed = _movementSpeed;
            if (NoiseManager.Instance.NoiseData.WalkModifier != NoiseManager.Instance.NoiseLevelMultiplier)
            {
                NoiseManager.Instance.NoiseLevelMultiplier = NoiseManager.Instance.NoiseData.WalkModifier;
            }
            _playerScript.IncreaseStamina();

            AudioManager.Instance.SetVolumeOverTime(audioSourceID, _walkVolume, 0.1f);
            AudioManager.Instance.SetAudioPitch(audioSourceID, _walkPitch);
        }
    }

    // Make the player move slower for 3 seconds
    private void MoveOutOfStamina()
    {
        Debug.Log("Out of Stamina!");
        _isOutOfStamina = true;

        // Disable sprint and slow down movement for 3 seconds
        _isSprinting = false;
        _movementSpeed *= 0.5f;

        // Timer to reset the _isOutOfStamina state after 3 seconds
        StartCoroutine(ResetOutOfStamina());
    }

    // Slow the player down every 3 minutes
    private void MoveDizzy()
    {
        // Randomly slow down the player by 80% once every 3 minutes
        if (Time.time % 120f < Time.fixedDeltaTime && !_isDizzy)
        {
            Debug.Log("Dizzy! Stop moving!");
            _movementSpeed = 1f;
            _sprintMovementSpeed = 1.5f;
            _isDizzy = true;
        }
        // Reset the _currentMovementSpeed to default when not dizzy
        else if (_isDizzy)
        {
            StartCoroutine(Dizzy());
        }
        // Reset the _currentMovementSpeed to default when not dizzy
        else if (!_isDizzy)
        {
            _movementSpeed = 4f;
            _sprintMovementSpeed = 7f;
        }
    }

    // Look and Turn the player
    private void Look()
    {
        // Turn
        transform.Rotate(Vector3.up * _lookPos.x * RotationSpeed * Time.deltaTime);

        // Look
        _playerLookRotation -= _lookPos.y * RotationSpeed * Time.deltaTime;
        _playerLookRotation = Mathf.Clamp(_playerLookRotation, _bottomClamp, _topClamp);
        _camRoot.transform.eulerAngles = new Vector3(_playerLookRotation, _camRoot.transform.eulerAngles.y, _camRoot.transform.eulerAngles.z);
    }

    // Set player in crouch position
    private void Crouch()
    {
        float targetCrouchY = _isCrouching ? _startYPosition - _crouchOffset : _startYPosition;

        // Calculate the target position
        Vector3 targetPosition = new Vector3(_camRoot.transform.localPosition.x, targetCrouchY, _camRoot.transform.localPosition.z);

        // Set the _camRoot's position to the target crouch position
        _camRoot.transform.localPosition = targetPosition;
    }

    // Aim and focus the camera
    private void Aim()
    {
        GameManager.Instance.PlayerAnimManager.AimAnimation(_isAiming);

        // Increase camera FOV smoothly when aiming
        CMVirtualCamera.m_Lens.FieldOfView = Mathf.Lerp(CMVirtualCamera.m_Lens.FieldOfView, _isAiming ? _focalLength : _startFocalLength, Time.deltaTime * 10f);

        Light _flashlightObject = GetComponent<Player>().Flashlight;
        if (_flashlightObject.enabled)
        {
            _flashlightObject.spotAngle = Mathf.Lerp(_flashlightObject.spotAngle, _isAiming ? _focusedFlashlightAngle : _defaultFlashlightAngle, Time.deltaTime * 10f);
            _flashlightObject.intensity = Mathf.Lerp(_flashlightObject.intensity, _isAiming ? _focusedFlashlightIntensity : _defaultFlashlightIntensity, Time.deltaTime * 10f);
        }
    }

    // Fire the weapon
    private void Fire()
    {
        _isFiring = false;

        // loop through weapons and execute the Weapon script that is on the active object
        foreach (Transform child in _weaponContainer.transform)
        {
            if (child.gameObject.activeSelf)
            {
                Weapon weapon = child.GetComponent<Weapon>();
                weapon.Execute(weapon);
                return;
            }
        }
    }

    // Interact with the object or return from pickup canvas
    private void Interact()
    {
        _isInteracting = false;
        if (GameManager.Instance.PickupCanvas.activeSelf || GameManager.Instance.IsCanvasActive)
        {
            GameManager.Instance.ResumeGame();
            GameManager.Instance.IsCanvasActive = false;
        }
        else
        {
            Debug.Log("PlayerController - Interact");
            _objectHandler.Execute();
        }
    }

    // Switch between weapons
    private void WeaponSwitch()
    {
        _weaponSwitcher.Execute(_weaponScrollValue);
    }

    // Reload the weapon
    private void Reload()
    {
        _isReloading = false;

        // loop through weapons and execute the Weapon script that is on the active object
        foreach (Transform child in _weaponContainer.transform)
        {
            if (child.gameObject.activeSelf)
            {
                child.GetComponent<Weapon>().ExecuteReload();
            }
        }
    }

    // Open the inventory
    private void Inventory()
    {
        _isOpeningInventory = false;

        if (GameManager.Instance.Player.GetComponent<InventoryController>().IsInventoryClosing)
        {
            return;
        }
        GameManager.Instance.OpenInventory();
    }

    // Open the menu
    private void Menu()
    {
        _isOpeningMenu = false;
        GameManager.Instance.OpenMenu();
    }

    // Turn the flashlight on or off
    private void Light()
    {
        _isUsingFlashlight = false;
        _playerScript.LightSwitch();
    }

    #endregion




    #region Input Processing Coroutines

    // Reset the player's movement speed after 3 seconds
    private IEnumerator ResetOutOfStamina()
    {
        Debug.Log("Resetting Out of Stamina");
        yield return new WaitForSeconds(3f);
        _movementSpeed *= 2f;
        _playerScript.IsOutOfStamina = false;
        _isOutOfStamina = false;
    }

    // Reset players dizzy state after 3 seconds
    private IEnumerator Dizzy()
    {
        yield return new WaitForSeconds(3f);
        _isDizzy = false;
        Debug.Log("Not Dizzy anymore!");
    }

    #endregion




    #region UI Functions

    // Show the ray of players sight
    private void OnDrawGizmos() 
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, Vector3.down * 2f);
    }

    #endregion
}
