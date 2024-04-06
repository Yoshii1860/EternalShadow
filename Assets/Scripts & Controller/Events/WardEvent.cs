using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WardEvent : InteractableObject
{
    [Space(10)]
    [Header("RUN ITEM CODE")]
    [SerializeField] Transform railings;
    [SerializeField] Door door;
    [SerializeField] GameObject crowbar;
    [SerializeField] Transform followTarget;
    [SerializeField] string displayMessage;
    GameObject firstRailing;
    GameObject secondRailing;

    void Start()
    {
        firstRailing = railings.GetChild(0).gameObject;
        secondRailing = railings.GetChild(1).gameObject;
        crowbar.SetActive(false);
    }

    // Override the base class method for specific implementation
    protected override void RunItemCode()
    {
        Item crowbarItem = InventoryManager.Instance.FindItem("Crowbar");
        if (crowbarItem != null)
        {
            InventoryManager.Instance.RemoveItem(crowbarItem);
            GameManager.Instance.playerController.ToggleArms(false);
            crowbar.SetActive(true);
            crowbar.GetComponent<Animator>().SetTrigger("Break");
            GameManager.Instance.playerController.SetFollowTarget(followTarget);
            GameManager.Instance.GameplayEvent();
            door.locked = false;
            Debug.Log("You broke the railing with the crowbar");
        }
        else
        {
            Debug.Log("You need a crowbar to break the railing");
            GameManager.Instance.DisplayMessage(displayMessage, 2f);
        }
    }
}
