using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DitzelGames.FastIK;

public class PlayerAnimController : MonoBehaviour
{
    #region Fields

    PlayerController playerController;
    public Animator animator;
    bool leftOrRight = true;

    FastIKFabric[] fastIKFabrics;
    [SerializeField] GameObject fpsArms;

    [SerializeField] Vector3 offFlashlightPosition;
    [SerializeField] Vector3 offFlashlightRotation;
    [SerializeField] Vector3 onFlashlightPosition;
    [SerializeField] Vector3 onFlashlightRotation;

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
        // Triggers the start animation and plays a sound
        animator.SetTrigger("StartAnim");
        AudioManager.Instance.PlaySoundOneShot(AudioManager.Instance.playerSpeaker, "player1", 0.8f, 1f);
    }

    public void AimAnimation(bool isAiming)
    {
        // Sets the aiming state of the animation
        animator.SetBool("Aim", isAiming);
    }

    public void ShootAnimation()
    {
        // Triggers the shoot animation, alternating between left and right
        animator.SetTrigger("Shoot");
        animator.SetBool("LeftOrRight", leftOrRight);
        leftOrRight = !leftOrRight;
    }

    public void BlindnessAnimation()
    {
        // Initiates the blindness animation and plays a sound
        animator.SetBool("Blinded", true);
        AudioManager.Instance.PlaySoundOneShot(AudioManager.Instance.playerSpeaker, "player2", .8f, 1f);
    }

    public void StopBlindnessAnimation()
    {
        // Stops the blindness animation
        animator.SetBool("Blinded", false);
    }

    public void Flashlight(GameObject flashlight, bool isOn)
    {
        if (isOn)
        {
            // Moves the flashlight to the new position and rotation
            StartCoroutine(LerpToOnPosition(flashlight, isOn));
            animator.SetLayerWeight(1, 0);
            animator.SetLayerWeight(2, 1);
        }
        else
        {
            // Moves the flashlight to the standard position and rotation
            StartCoroutine(LerpToOnPosition(flashlight, isOn));
            animator.SetLayerWeight(1, 1);
            animator.SetLayerWeight(2, 0);
        }
    }

    // Lerps the flashlight to the new position and rotation
    IEnumerator LerpToOnPosition(GameObject flashlight, bool isOn)
    {
        if (isOn)
        {
            flashlight.SetActive(true);
            IKFabrics(true);
        }

        // Sets the initial position and rotation of the flashlight
        Vector3 currentPosition = flashlight.transform.localPosition;
        Vector3 currentRotation = flashlight.transform.localEulerAngles;
        // Sets the target position and rotation of the flashlight
        Vector3 targetPosition = isOn ? onFlashlightPosition : offFlashlightPosition;
        Vector3 targetRotation = isOn ? onFlashlightRotation : offFlashlightRotation;

        float lerpTime = isOn ? 0.5f : 0.1f;
        float positionDistance = isOn ? 0.01f : 40f;
        float rotationDistance = isOn ? 0.1f : 100f;

        while (Vector3.Distance(currentPosition, targetPosition) > positionDistance ||
            Vector3.Distance(currentRotation, targetRotation) > rotationDistance)
        {
            currentPosition = Vector3.Lerp(currentPosition, targetPosition, lerpTime);
            currentRotation = Vector3.Lerp(currentRotation, targetRotation, lerpTime);

            flashlight.transform.localPosition = currentPosition;
            flashlight.transform.localEulerAngles = currentRotation;

            yield return null;
        }

        // Sets the final position and rotation of the flashlight
        flashlight.transform.localPosition = targetPosition;
        flashlight.transform.localEulerAngles = currentRotation;

        if (!isOn)
        {
            flashlight.SetActive(false);
            IKFabrics(false);
        }
    }

    // Sets the base layer (no weapon no flashlight) of the animation
    public void SetBaseLayers()
    {
        // Sets the base layers of the animation
        animator.SetLayerWeight(1, 1);
        animator.SetLayerWeight(2, 0);
        //animator.SetLayerWeight(3, 1);
        //animator.SetLayerWeight(4, 1);
    }

    // Enables or disables all IK fabrics
    void IKFabrics(bool onOrOff)
    {
        // Enables the IK fabrics
        foreach (FastIKFabric fastIKFabric in fastIKFabrics)
        {
            fastIKFabric.enabled = onOrOff;
        }
    }

    #endregion

    #region Initialization Method

    void InitializeReferences()
    {
        fastIKFabrics = fpsArms.GetComponentsInChildren<FastIKFabric>();
        IKFabrics(false);
        
        // Initializes references, such as the player controller and animator
        playerController = GetComponent<PlayerController>();
        if (animator == null)
        {
            animator = transform.GetChild(0).GetComponentInChildren<Animator>();
        }
    }

    #endregion
}