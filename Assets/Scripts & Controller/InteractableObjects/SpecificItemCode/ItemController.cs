using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(UniqueIDComponent))]
public class ItemController : InteractableObject
{
    public Item item;
    public Vector3 originalPosition;
    bool pickedUpState;

    public bool isPickedUp
    {
        get => pickedUpState;
        set
        {
            pickedUpState = value;
            // Whenever isPickedUp is changed, update the transform.position
            if (pickedUpState)
            {
                // Move the object to a position far away
                transform.position = new Vector3(0, -1000f, 0);
            }
            else
            {
                // If it's not picked up, reset its position to its initial position.
                transform.position = originalPosition;
            }
        }
    }

    void Start() 
    {
        pickedUpState = false;
        originalPosition = transform.position;
    }

    protected override void RunItemCode()
    {
        if (isPickup) return;
        if (clipName == "") clipName = "pickup item";
        if (!isPickup) GameManager.Instance.DisplayMessage("Picked up " + item.displayName, 2f);
        AudioManager.Instance.PlaySoundOneShot(AudioManager.Instance.playerSpeaker2, clipName, 0.6f, 1f);
    }
}
