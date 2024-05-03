using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DitzelGames.FastIK;

public class PlayerAnimManager : MonoBehaviour
{
    #region Private Fields

    // Components for the player animation controller
    private Animator _animator;
    private PlayerController _playerController;
    private string _currentWeaponName = "None";
    private bool _isLightSwitchedOn = false;
    private bool _isPistolEquipped = false;

    [Header("Weapons")]
    [Tooltip("The GameObject that contains the weapons")]
    [SerializeField] private GameObject _weaponsContainer;
    [Tooltip("The Pistol GameObject")]
    [SerializeField] private GameObject _pistolObject;
    // Muzzle Flash Particle System of the Pistol
    private ParticleSystem _muzzleFlash;
    [Space(10)]

    [Header("IK Fabrics")]
    // FastIKFabric components for the left and right shoulders
    private FastIKFabric[] _leftIKPoints;
    private FastIKFabric[] _rightIKPoints;
    [Tooltip("The GameObjects that contain the left shoulder")] 
    [SerializeField] private GameObject _leftShoulder;
    [Tooltip("The GameObjects that contain the right shoulder")]
    [SerializeField] private GameObject _rightShoulder;
    [Tooltip("The GameObjects that contain the left hand IK points on the flashlight")]
    [SerializeField] private Transform[] _flashlightPoints;
    [Tooltip("The GameObjects that contain the left hand IK points on the pistol")]
    [SerializeField] private Transform[] _leftPistolPoints;
    [Space(10)]

    [Header("Debug Mode")]
    [Tooltip("Enables or disables debug mode")]
    public bool DebugMode = false;

    #endregion




    #region Unity Lifecycle Methods

    // Start is called before the first frame update
    void Start()
    {
        InitializeReferences();
    }

    #endregion




    #region Animation Methods

    // Starts the animation of the player when starting a new game
    public void StartAnimation()
    {
        if (DebugMode) Debug.Log("PlayerAnim - StartAnimation");

        // Triggers the start animation and plays a sound
        _animator.SetTrigger("StartAnim");
        AudioManager.Instance.PlayClipOneShot(AudioManager.Instance.PlayerSpeaker, "player1", 0.8f, 1f);
    }

    // Starts and stops the aim animation of the player
    public void AimAnimation(bool isAiming)
    {
        if (DebugMode) Debug.Log("PlayerAnim - AimAnimation: " + isAiming);

        // Sets the aiming state of the animation
        if (_isPistolEquipped || _isLightSwitchedOn) 
        {
            _animator.SetBool("Aim", isAiming);
            GameManager.Instance.Player.ShowBulletsUI(isAiming);
        }
    }

    // Starts the walk animation of the player
    public void ShootAnimation()
    {
        if (DebugMode) Debug.Log("PlayerAnim - Try to shoot!");

        // Triggers the shoot animation
        if (_isPistolEquipped) 
        {
            // Check if the muzzle flash is already playing
            if (_muzzleFlash.isPlaying) return;
            // Set the trigger for the shoot animation
            _animator.SetTrigger("Shoot");
            // Play Shoot Audio
            AudioManager.Instance.PlayClipOneShot(AudioManager.Instance.PlayerSpeaker, "pistol shoot", 1f, 1f);
            // Play Muzzle Flash
            _muzzleFlash.Play();
            _muzzleFlash.transform.GetComponentInChildren<LightFlicker>().StartFlicker();
        }
    }

    // Starts the blindness animation of the player during the yard event
    public void BlindnessAnimation()
    {
        if (DebugMode) Debug.Log("PlayerAnim - BlindnessAnimation");

        // Initiates the blindness animation and plays a sound
        SetAnimLayer("None");
        _animator.SetBool("Blinded", true);
        AudioManager.Instance.PlayClipOneShot(AudioManager.Instance.PlayerSpeaker, "player2", .8f, 1f);
    }

    // Stops the blindness animation of the player during the yard event
    public void StopBlindnessAnimation()
    {
        if (DebugMode) Debug.Log("PlayerAnim - StopBlindnessAnimation");

        // Stops the blindness animation
        _animator.SetBool("Blinded", false);
    }

    // Starts the penholder animation of the player during the paintings event
    public void PenholderAnimation()
    {
        if (DebugMode) Debug.Log("PlayerAnim - PenholderAnimation");

        // Initiates the penholder animation
        //if (GameManager.Instance.Player.Flashlight.enabled) GameManager.Instance.Player.LightSwitch();
        //if (_isPistolEquipped) _animator.gameObject.GetComponentInChildren<WeaponSwitcher>().Execute(1);
    }

    // Starts the reload animation of the player
    public void ReloadAnimation()
    {
        if (DebugMode) Debug.Log("PlayerAnim - ReloadAnimation");
        // Initiates the reload animation
        _animator.SetTrigger("Reload");
        AudioManager.Instance.PlayClipOneShot(AudioManager.Instance.PlayerSpeaker2, "reload pistol", 0.6f, 1f);
    }

    #endregion




    #region State Changes

    // Sets the weapon of the player to the next weapon
    public void SetWeapon(Transform currentWeapon, Transform nextWeapon)
    {
        if (DebugMode) Debug.Log("PlayerAnim - SetWeapon: " + nextWeapon.name + " - LightSwitch: " + _isLightSwitchedOn);

        switch(nextWeapon.name)
        {
            case "None":
                _isPistolEquipped = false;
                StartCoroutine(WeaponStateChange(currentWeapon.gameObject, false));
                this._currentWeaponName = nextWeapon.name;
                SetAnimLayer(this._currentWeaponName);
                break;

            case "Pistol":
                _isPistolEquipped = true;
                StartCoroutine(WeaponStateChange(nextWeapon.gameObject, true));
                this._currentWeaponName = nextWeapon.name;
                SetAnimLayer(this._currentWeaponName);
                break;
        }
    }

    // Sets the flashlight of the player to the new position and rotation
    public void Flashlight(GameObject flashlight, bool isOn)
    {
        if (DebugMode) Debug.Log("PlayerAnim - Flashlight: " + isOn);

        // Moves the flashlight to the new position and rotation
        _isLightSwitchedOn = isOn;
        StartCoroutine(WeaponStateChange(flashlight, isOn));
        SetAnimLayer(_currentWeaponName);
    }

    // Lerps the flashlight to the new position and rotation
    IEnumerator WeaponStateChange(GameObject weapon, bool isOn)
    {
        if (DebugMode) Debug.Log("PlayerAnim - WeaponStateChange: " + weapon.name + " - LightSwitch: " + isOn);

        // If weapon will be enabled
        if (isOn)
        {
            weapon.SetActive(true);

            switch (weapon.name)
            {
                case "Pistol":
                    IKFabrics(_rightIKPoints, true);
                    AudioManager.Instance.PlayClipOneShot(_weaponsContainer.GetInstanceID(), "pistol on", 0.5f, 1.2f);
                    if (!_isLightSwitchedOn)
                    {
                        IKFabrics(_leftIKPoints, true);
                        IKFabricsTargetChange(weapon.name);
                    }
                    else
                    {
                        IKFabrics(_leftIKPoints, true);
                        IKFabricsTargetChange("Flashlight");
                    }
                    break;
                case "None":
                    IKFabrics(_rightIKPoints, false);
                    if (!_isLightSwitchedOn) IKFabrics(_leftIKPoints, false);
                    break;
                case "Flashlight":
                    AudioManager.Instance.PlayClipOneShot(_weaponsContainer.GetInstanceID(), "flashlight on", 0.5f);
                    IKFabrics(_leftIKPoints, true);
                    IKFabricsTargetChange("Flashlight");
                    break;
            }
        }
        // If weapon will be disabled
        else
        {
            // Set Weapon inactive and disable IK fabrics
            weapon.SetActive(false);

            switch(weapon.name)
            {
                case "Pistol":
                    AudioManager.Instance.PlayClipOneShot(_weaponsContainer.GetInstanceID(), "pistol off", 0.5f);
                    IKFabrics(_rightIKPoints, false);
                    if (!_isLightSwitchedOn) IKFabrics(_leftIKPoints, false);
                    break;

                case "None":
                    IKFabrics(_rightIKPoints, false);
                    if (!_isLightSwitchedOn) IKFabrics(_leftIKPoints, false);
                    break;

                default:
                    AudioManager.Instance.PlayClipOneShot(_weaponsContainer.GetInstanceID(), "flashlight off", 0.5f);
                    if (!_isPistolEquipped) IKFabrics(_leftIKPoints, false);
                    else IKFabricsTargetChange("Pistol");
                    break;
            }
        }

        yield return null;
    }

    // Sets the base layer (no weapon no flashlight) of the animation
    public void SetAnimLayer(string layerName)
    {
        if (DebugMode) Debug.Log("PlayerAnim - SetAnimLayer: " + layerName + " - LightSwitch: " + _isLightSwitchedOn);

        switch (layerName)
        {
            // If the player has no weapon
            case "None":
                _animator.SetLayerWeight(1, _isLightSwitchedOn ? 0 : 1);
                _animator.SetLayerWeight(2, _isLightSwitchedOn ? 1 : 0);
                _animator.SetLayerWeight(3, 0);
                _animator.SetLayerWeight(4, 0);
                break;

            // If the player has a pistol
            case "Pistol":
                _animator.SetLayerWeight(1, 0);
                _animator.SetLayerWeight(2, 0);
                _animator.SetLayerWeight(3, _isLightSwitchedOn ? 0 : 1);
                _animator.SetLayerWeight(4, _isLightSwitchedOn ? 1 : 0);
                break;
        }
    }

    #endregion




    #region Inverse Kinematics Methods

    // Enables or disables all IK fabrics
    void IKFabrics(FastIKFabric[] leftOrRight, bool onOrOff)
    {
        if (DebugMode) Debug.Log("PlayerAnim - IKFabrics: " + onOrOff);

        // Enables the IK fabrics
        foreach (FastIKFabric ikFabric in leftOrRight)
        {
            ikFabric.enabled = onOrOff;
        }
    }

    // Changes the target of the IK fabrics from the flashlight to the pistol and vice versa
    void IKFabricsTargetChange(string weaponName)
    {
        if (DebugMode) Debug.Log("PlayerAnim - IKFabricsTargetChange: " + weaponName + " - LightSwitch: " + _isLightSwitchedOn);

        // Changes the target of the IK fabrics
        foreach (FastIKFabric ikFabric in _leftIKPoints)
        {
            // Extract the base GameObject name before "#.L_end"
            string baseName = ikFabric.transform.name.Substring(0, ikFabric.transform.name.Length - 7);

            if (string.Compare(weaponName, "Pistol") == 0)
            {
                foreach (Transform pistolPoint in _leftPistolPoints)
                {
                    if (pistolPoint.name.Contains(baseName))
                    {
                        if (pistolPoint.name.Contains("Target"))
                        {
                            ikFabric.Target = pistolPoint;
                        }
                        else
                        {
                            ikFabric.Pole = pistolPoint;
                        }
                    }
                }
            }
            // If the weapon is the flashlight
            else
            {
                foreach (Transform flashlightPoint in _flashlightPoints)
                {
                    
                    if (flashlightPoint.name.Contains(baseName))
                    {
                        if (flashlightPoint.name.Contains("Target"))
                        {
                            ikFabric.Target = flashlightPoint;
                        }
                        else
                        {
                            ikFabric.Pole = flashlightPoint;
                        }
                    }
                }
            }
        }
    }

    #endregion




    #region Initialization Method

    // Initializes the references of the player animation controller
    void InitializeReferences()
    {
        _muzzleFlash = _pistolObject.GetComponentInChildren<ParticleSystem>();
        _leftIKPoints = _leftShoulder.GetComponentsInChildren<FastIKFabric>();
        _rightIKPoints = _rightShoulder.GetComponentsInChildren<FastIKFabric>();
        IKFabrics(_leftIKPoints, false);
        IKFabrics(_rightIKPoints, false);
        
        // Initializes references, such as the player controller and _animator
        _playerController = GetComponent<PlayerController>();
        if (_animator == null)
        {
            _animator = transform.GetChild(0).GetComponentInChildren<Animator>();
        }
    }

    #endregion
}