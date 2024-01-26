using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimController : MonoBehaviour
{
    #region Fields

    PlayerController playerController;
    public Animator animator;
    bool leftOrRight = true;

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

    #endregion

    #region Initialization Method

    void InitializeReferences()
    {
        // Initializes references, such as the player controller and animator
        playerController = GetComponent<PlayerController>();
        if (animator == null)
        {
            animator = transform.GetChild(0).GetComponentInChildren<Animator>();
        }
    }

    #endregion
}