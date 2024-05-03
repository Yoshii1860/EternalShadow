using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MirrorDoorEvent : InteractableObject
{
    #region References

    [Space(10)]
    [Header("References")]
    [Tooltip("The railings container")]
    [SerializeField] private Transform _railingContainer;
    [Tooltip("The door to unlock")]
    [SerializeField] private Door _door;
    [Tooltip("The crowbar object")]
    [SerializeField] private GameObject _crowbar;
    [Tooltip("The target to follow for the camera")]
    [SerializeField] private Transform _followTarget;
    [Tooltip("The message to display when the player doesn't have the crowbar")]
    [SerializeField] private string _displayMessage;

    private GameObject _firstRailing;
    private GameObject _secondRailing;

    #endregion




    #region Unity Methods

    protected override void Start()
    {
        _firstRailing = _railingContainer.GetChild(0).gameObject;
        _secondRailing = _railingContainer.GetChild(1).gameObject;
        _crowbar.SetActive(false);
    }

    #endregion




    #region Base Class Methods

    // Override the base class method for specific implementation
    protected override void RunItemCode()
    {
        // Check if the player has the crowbar
        Item crowbarItem = InventoryManager.Instance.FindItem("Crowbar");
        if (crowbarItem != null)
        {
            // Remove the crowbar from the inventory
            InventoryManager.Instance.RemoveItem(crowbarItem);

            // Start a gameplay event
            GameManager.Instance.PlayerController.ToggleArms(false);
            GameManager.Instance.PlayerController.SetFollowTarget(_followTarget);
            GameManager.Instance.GameplayEvent();

            // Break the railings
            _crowbar.SetActive(true);
            _crowbar.GetComponent<Animator>().SetTrigger("Break");

            // Unlock the door
            _door.IsLocked = false;

            Debug.Log("You broke the railing with the crowbar");
        }
        // If the player doesn't have the crowbar, display a message
        else
        {
            Debug.Log("You need a crowbar to break the railing");
            GameManager.Instance.DisplayMessage(_displayMessage, 2f);
        }
    }

    #endregion
}
