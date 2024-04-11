using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DitzelGames.FastIK;

public class PlayerAnimController : MonoBehaviour
{
    #region Fields

    PlayerController playerController;
    public Animator animator;
    bool lightSwitch = false;
    string currentWeapon = "None";
    bool pistolBool = false;

    [SerializeField] GameObject weaponsObj;
    [SerializeField] GameObject pistol;
    ParticleSystem muzzleFlash;

    FastIKFabric[] fastIKFabricsLeft;
    FastIKFabric[] fastIKFabricsRight;
    [SerializeField] GameObject shoulderLeft;
    [SerializeField] GameObject shoulderRight;
    [SerializeField] Transform[] flashlightPoints;
    [SerializeField] Transform[] pistolPoints;

    // Flashlight positions and rotations
    [SerializeField] Vector3 offFlashlightPosition;
    [SerializeField] Vector3 offFlashlightRotation;
    [SerializeField] Vector3 onFlashlightPosition;
    [SerializeField] Vector3 onFlashlightRotation;

    // Pistol positions and rotations
    [SerializeField] Vector3 offPistolPosition;
    [SerializeField] Vector3 offPistolRotation;
    [SerializeField] Vector3 onPistolPosition;
    [SerializeField] Vector3 onPistolRotation;

    public bool debugMode = false;

    #endregion

    #region Unity Lifecycle Methods

    // Start is called before the first frame update
    void Start()
    {
        InitializeReferences();
    }

    #endregion

    #region Animation Methods

    public void StartAnimation()
    {
        if (debugMode) Debug.Log("PlayerAnim - StartAnimation");

        // Triggers the start animation and plays a sound
        animator.SetTrigger("StartAnim");
        AudioManager.Instance.PlaySoundOneShot(AudioManager.Instance.playerSpeaker, "player1", 0.8f, 1f);
    }

    public void AimAnimation(bool isAiming)
    {
        if (debugMode) Debug.Log("PlayerAnim - AimAnimation: " + isAiming);

        // Sets the aiming state of the animation
        if (pistolBool || lightSwitch) 
        {
            animator.SetBool("Aim", isAiming);
            GameManager.Instance.player.ShowBulletsUI(isAiming);
        }
    }

    public void ShootAnimation()
    {
        if (debugMode) Debug.Log("PlayerAnim - Try to shoot!");

        // Triggers the shoot animation
        if (pistolBool) 
        {
            // Check if the muzzle flash is already playing
            if (muzzleFlash.isPlaying) return;
            // Set the trigger for the shoot animation
            animator.SetTrigger("Shoot");
            // Play Shoot Audio
            AudioManager.Instance.PlaySoundOneShot(AudioManager.Instance.playerSpeaker, "pistol shoot", 1f, 1f);
            // Play Muzzle Flash
            muzzleFlash.Play();
            muzzleFlash.transform.GetComponentInChildren<LightFlicker>().StartFlicker();
        }
    }

    public void BlindnessAnimation()
    {
        if (debugMode) Debug.Log("PlayerAnim - BlindnessAnimation");

        // Initiates the blindness animation and plays a sound
        SetAnimLayer("None");
        animator.SetBool("Blinded", true);
        AudioManager.Instance.PlaySoundOneShot(AudioManager.Instance.playerSpeaker, "player2", .8f, 1f);
    }

    public void StopBlindnessAnimation()
    {
        if (debugMode) Debug.Log("PlayerAnim - StopBlindnessAnimation");

        // Stops the blindness animation
        animator.SetBool("Blinded", false);
    }

    public void PenholderAnimation()
    {
        if (debugMode) Debug.Log("PlayerAnim - PenholderAnimation");

        // Initiates the penholder animation
        if (GameManager.Instance.player.flashlight.enabled) GameManager.Instance.player.LightSwitch();
        if (pistolBool) animator.gameObject.GetComponentInChildren<WeaponSwitcher>().Execute(1);
    }

    public void ReloadAnimation()
    {
        if (debugMode) Debug.Log("PlayerAnim - ReloadAnimation");
        // Initiates the reload animation
        animator.SetTrigger("Reload");
        AudioManager.Instance.PlaySoundOneShot(AudioManager.Instance.playerSpeaker2, "reload pistol", 0.6f, 1f);
    }

    #endregion

    #region Inverse Kinematics

    public void SetWeapon(Transform currentWeapon, Transform nextWeapon)
    {
        if (debugMode) Debug.Log("PlayerAnim - SetWeapon: " + nextWeapon.name + " - LightSwitch: " + lightSwitch);

        if (string.Compare(nextWeapon.name, "None") == 0)
        {
            pistolBool = false;
            StartCoroutine(LerpToOnPosition(currentWeapon.gameObject, false));
            this.currentWeapon = nextWeapon.name;
            SetAnimLayer(this.currentWeapon);
        }
        else if (string.Compare(nextWeapon.name, "Pistol") == 0)
        {
            pistolBool = true;
            StartCoroutine(LerpToOnPosition(nextWeapon.gameObject, true));
            this.currentWeapon = nextWeapon.name;
            SetAnimLayer(this.currentWeapon);
        }
    }

    public void Flashlight(GameObject flashlight, bool isOn)
    {
        if (debugMode) Debug.Log("PlayerAnim - Flashlight: " + isOn);

        // Moves the flashlight to the new position and rotation
        lightSwitch = isOn;
        StartCoroutine(LerpToOnPosition(flashlight, isOn));
        SetAnimLayer(currentWeapon);
    }

    // Lerps the flashlight to the new position and rotation
    IEnumerator LerpToOnPosition(GameObject weapon, bool isOn)
    {
        if (debugMode) Debug.Log("PlayerAnim - LerpToOnPosition: " + weapon.name + " - LightSwitch: " + isOn);

        // If weapon will be enabled
        if (isOn)
        {
            weapon.SetActive(true);

            switch (weapon.name)
            {
                case "Pistol":
                    IKFabrics(fastIKFabricsRight, true);
                    AudioManager.Instance.PlaySoundOneShot(weaponsObj.GetInstanceID(), "pistol on", 0.5f, 1.2f);
                    if (!lightSwitch)
                    {
                        IKFabrics(fastIKFabricsLeft, true);
                        IKFabricsTargetChange(weapon.name);
                    }
                    else
                    {
                        IKFabrics(fastIKFabricsLeft, true);
                        IKFabricsTargetChange("Flashlight");
                    }
                    break;
                case "None":
                    IKFabrics(fastIKFabricsRight, false);
                    if (!lightSwitch) IKFabrics(fastIKFabricsLeft, false);
                    break;
                case "Flashlight":
                    AudioManager.Instance.PlaySoundOneShot(weaponsObj.GetInstanceID(), "flashlight on", 0.5f);
                    IKFabrics(fastIKFabricsLeft, true);
                    IKFabricsTargetChange("Flashlight");
                    break;
            }
        }

        /*

        // Sets the initial position and rotation of the flashlight
        Vector3 currentPosition = weapon.transform.localPosition;
        Vector3 currentRotation = weapon.transform.localEulerAngles;

        // Sets the target position and rotation of the flashlight
        Vector3 targetPosition;
        Vector3 targetRotation;

        if (string.Compare(weapon.name, "Pistol") == 0)
        {
            targetPosition = isOn ? onPistolPosition : offPistolPosition;
            targetRotation = isOn ? onPistolRotation : offPistolRotation;
        }
        else if (string.Compare(weapon.name, "Flashlight") == 0)
        {
            targetPosition = isOn ? onFlashlightPosition : offFlashlightPosition;
            targetRotation = isOn ? onFlashlightRotation : offFlashlightRotation;
        }
        else
        {
            targetPosition = Vector3.zero;
            targetRotation = Vector3.zero;
        }

        float lerpTime = isOn ? 0.5f : 0.1f;
        float positionDistance = isOn ? 0.01f : 40f;
        float rotationDistance = isOn ? 0.1f : 100f;

        while (Vector3.Distance(currentPosition, targetPosition) > positionDistance ||
            Vector3.Distance(currentRotation, targetRotation) > rotationDistance)
        {
            Debug.Log("Lerping: " + weapon.name + " - targetPosition: " + targetPosition + " - targetRotation: " + targetRotation);
            Debug.Log(weapon.name + "lerping to" + weapon.transform.localPosition + " - " + weapon.transform.localEulerAngles);

            currentPosition = Vector3.Lerp(currentPosition, targetPosition, lerpTime);
            currentRotation = Vector3.Lerp(currentRotation, targetRotation, lerpTime);

            weapon.transform.localPosition = currentPosition;
            weapon.transform.localEulerAngles = currentRotation;

            yield return null;
        }

        // Sets the final position and rotation of the flashlight
        weapon.transform.localPosition = targetPosition;
        weapon.transform.localEulerAngles = currentRotation;

        */

        // If weapon will be disabled
        if (!isOn)
        {
            // Set Weapon inactive and disable IK fabrics
            weapon.SetActive(false);

            if (string.Compare(weapon.name, "Pistol") == 0)
            {
                AudioManager.Instance.PlaySoundOneShot(weaponsObj.GetInstanceID(), "pistol off", 0.5f);
                IKFabrics(fastIKFabricsRight, false);
                if (!lightSwitch) IKFabrics(fastIKFabricsLeft, false);
            }
            else if (string.Compare(weapon.name, "None") == 0)
            {
                IKFabrics(fastIKFabricsRight, false);
                if (!lightSwitch) IKFabrics(fastIKFabricsLeft, false);
            }
            else
            {
                AudioManager.Instance.PlaySoundOneShot(weaponsObj.GetInstanceID(), "flashlight off", 0.5f);
                if (!pistolBool) IKFabrics(fastIKFabricsLeft, false);
                else IKFabricsTargetChange("Pistol");
            }
        }

        yield return null;
    }

    // Sets the base layer (no weapon no flashlight) of the animation
    public void SetAnimLayer(string layerName)
    {
        if (debugMode) Debug.Log("PlayerAnim - SetAnimLayer: " + layerName + " - LightSwitch: " + lightSwitch);

        switch (layerName)
        {
            case "None":
                if(!lightSwitch)
                {
                    animator.SetLayerWeight(1, 1);
                    animator.SetLayerWeight(2, 0);
                    animator.SetLayerWeight(3, 0);
                    animator.SetLayerWeight(4, 0);
                }
                else
                {
                    animator.SetLayerWeight(1, 0);
                    animator.SetLayerWeight(2, 1);
                    animator.SetLayerWeight(3, 0);
                    animator.SetLayerWeight(4, 0);
                }
                break;
            case "Pistol":
                if(!lightSwitch)
                {
                    animator.SetLayerWeight(1, 0);
                    animator.SetLayerWeight(2, 0);
                    animator.SetLayerWeight(3, 1);
                    animator.SetLayerWeight(4, 0);
                }
                else
                {
                    animator.SetLayerWeight(1, 0);
                    animator.SetLayerWeight(2, 0);
                    animator.SetLayerWeight(3, 0);
                    animator.SetLayerWeight(4, 1);
                }
                break;
        }
    }

    // Enables or disables all IK fabrics
    void IKFabrics(FastIKFabric[] leftOrRight, bool onOrOff)
    {
        if (debugMode) Debug.Log("PlayerAnim - IKFabrics: " + onOrOff);

        // Enables the IK fabrics
        foreach (FastIKFabric fastIKFabric in leftOrRight)
        {
            fastIKFabric.enabled = onOrOff;
        }
    }

    void IKFabricsTargetChange(string weaponName)
    {
        if (debugMode) Debug.Log("PlayerAnim - IKFabricsTargetChange: " + weaponName + " - LightSwitch: " + lightSwitch);

        // Changes the target of the IK fabrics
        foreach (FastIKFabric fastIKFabric in fastIKFabricsLeft)
        {
            // Extract the base GameObject name before "#.L_end"
            string baseName = fastIKFabric.transform.name.Substring(0, fastIKFabric.transform.name.Length - 7);

            if (string.Compare(weaponName, "Pistol") == 0)
            {
                foreach (Transform pistolPoint in pistolPoints)
                {
                    if (pistolPoint.name.Contains(baseName))
                    {
                        if (pistolPoint.name.Contains("Target"))
                        {
                            fastIKFabric.Target = pistolPoint;
                        }
                        else
                        {
                            fastIKFabric.Pole = pistolPoint;
                        }
                    }
                }
            }
            else
            {
                foreach (Transform flashlightPoint in flashlightPoints)
                {
                    // Overwriting the flashlightPoint renderer until find the cause of its activation
                    flashlightPoint.GetComponent<Renderer>().enabled = false;
                    
                    if (flashlightPoint.name.Contains(baseName))
                    {
                        if (flashlightPoint.name.Contains("Target"))
                        {
                            fastIKFabric.Target = flashlightPoint;
                        }
                        else
                        {
                            fastIKFabric.Pole = flashlightPoint;
                        }
                    }
                }
            }
        }
    }

    #endregion

    #region Initialization Method

    void InitializeReferences()
    {
        muzzleFlash = pistol.GetComponentInChildren<ParticleSystem>();
        fastIKFabricsLeft = shoulderLeft.GetComponentsInChildren<FastIKFabric>();
        fastIKFabricsRight = shoulderRight.GetComponentsInChildren<FastIKFabric>();
        IKFabrics(fastIKFabricsLeft, false);
        IKFabrics(fastIKFabricsRight, false);
        
        // Initializes references, such as the player controller and animator
        playerController = GetComponent<PlayerController>();
        if (animator == null)
        {
            animator = transform.GetChild(0).GetComponentInChildren<Animator>();
        }
    }

    #endregion
}