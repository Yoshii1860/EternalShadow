using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimController : MonoBehaviour
{
    PlayerController playerController;
    public Animator animator;
    bool leftOrRight = true;

    // Start is called before the first frame update
    void Start()
    {
        playerController = GetComponent<PlayerController>();
        if (animator == null)
        {
            animator = transform.GetChild(0).GetComponentInChildren<Animator>();
        }
    }

    public void StartAnimation()
    {
        animator.SetTrigger("StartAnim");
        AudioManager.Instance.PlaySoundOneShot(AudioManager.Instance.playerSpeaker, "player1", 0.8f, 1f, false);
    }

    public void AimAnimation(bool isAiming)
    {
        animator.SetBool("Aim", isAiming);
    }

    public void ShootAnimation()
    {
        animator.SetTrigger("Shoot");
        animator.SetBool("LeftOrRight", leftOrRight);
        leftOrRight = !leftOrRight;
    }

    public void BlindnessAnimation()
    {
        animator.SetBool("Blinded", true);
        AudioManager.Instance.PlaySoundOneShot(AudioManager.Instance.playerSpeaker, "player2", .8f, 1f, false);
    }

    public void StopBlindnessAnimation()
    {
        animator.SetBool("Blinded", false);
    }
}
