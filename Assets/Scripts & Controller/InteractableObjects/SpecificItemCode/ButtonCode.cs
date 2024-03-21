using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonCode : InteractableObject
{
    [Space(10)]
    [Header("RUN ITEM CODE")]
    [Tooltip("The door to open")]
    [SerializeField] Door doorToOpen;
    [Tooltip("The slenderman to spawn when the button is pushed")]
    [SerializeField] GameObject slenderman;
    Animator animator;

    // Override the base class method for specific implementation
    protected override void RunItemCode()
    {
        animator = GetComponent<Animator>();
        animator.SetTrigger("Button");
        AudioManager.Instance.PlaySoundOneShot(AudioManager.Instance.playerSpeaker, "push button", 1f, 1f);
        doorToOpen.locked = false;
        doorToOpen.Interact();
        slenderman.SetActive(true);
        slenderman.GetComponent<AISensor>().PlayerInSightForced(6f);
    }
}
