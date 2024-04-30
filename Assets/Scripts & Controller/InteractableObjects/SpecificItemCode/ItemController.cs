using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(UniqueIDComponent))]
public class ItemController : InteractableObject
{
    private Vector3 _originalPosition;
    private bool _pickedUpState;

    public bool IsPickedUp
    {
        get => _pickedUpState;
        set
        {
            _pickedUpState = value;
            // Whenever isPickedUp is changed, update the transform.position
            if (_pickedUpState)
            {
                // Move the object to a position far away
                transform.position = new Vector3(0, -1000f, 0);
            }
            else
            {
                // If it's not picked up, reset its position to its initial position.
                transform.position = _originalPosition;
            }
        }
    }

    void Start() 
    {
        _pickedUpState = false;
        _originalPosition = transform.position;
    }

    protected override void RunItemCode()
    {
        if (IsPickup) return;
        if (ClipNameOnPickup == "") ClipNameOnPickup = "pickup item";
        if (!IsPickup) GameManager.Instance.DisplayMessage("Picked up " + Item.DisplayName, 2f);
        AudioManager.Instance.PlayClipOneShot(AudioManager.Instance.PlayerSpeaker2, ClipNameOnPickup, 0.6f, 1f);
    }
}
