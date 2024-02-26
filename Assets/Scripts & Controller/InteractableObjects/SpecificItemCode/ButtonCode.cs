using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonCode : InteractableObject
{
    [Space(10)]
    [Header("RUN ITEM CODE")]
    [Tooltip("The door to open")]
    [SerializeField] Door doorToOpen;
    [Tooltip("The door to unlock")]
    [SerializeField] Door doorOne;
    [Tooltip("The door to unlock")]
    [SerializeField] Door doorTwo;
    Animator animator;

    // Override the base class method for specific implementation
    protected override void RunItemCode()
    {
        animator = GetComponent<Animator>();
        animator.SetTrigger("Button");
        AudioManager.Instance.PlaySoundOneShot(AudioManager.Instance.playerSpeaker, "push button", 1f, 1f);
        doorToOpen.locked = false;
        doorOne.locked = false;
        doorTwo.locked = false;
        doorToOpen.Interact();
    }
}
