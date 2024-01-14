using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimController : MonoBehaviour
{
    PlayerController playerController;
    [SerializeField] Animator animator;
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
    }

    public void StopBlindnessAnimation()
    {
        animator.SetBool("Blinded", false);
    }
}
