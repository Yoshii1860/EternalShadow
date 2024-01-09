using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectHandler : MonoBehaviour
{
    // [SerializeField] private InputActionReference input;
    [SerializeField] float pickupDistance = 5f;
    ItemActions action;

    void Start() 
    {
        UpdateReferences();
    }

    public void Execute()
    {
        DetectObjects();
    }

    public void UpdateReferences()
    {
        action = GetComponent<ItemActions>();
    }

    private void DetectObjects()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, pickupDistance))
        {
            Debug.Log("ObjectHandler.DetectObjects: " + hit.collider.gameObject.name);
            if (hit.collider.gameObject.GetComponent<ItemController>() != null)
            {
                // Use PickUp() method from ItemActions class
                Debug.Log("ObjectHandler - PickUp");
                action.PickUp(hit);
            }
            else if (hit.collider.gameObject.tag == "Save")
            {
                // Open UI to save game
                Debug.Log("ObjectHandler - Save");
                GameManager.Instance.OpenSaveScreen();
            }
            else if (hit.collider.gameObject.layer == 6)
            {
                Debug.Log("ObjectHandler - Interact");
                InteractableObject intObj = hit.collider.gameObject.GetComponent<InteractableObject>();
                Door door = hit.collider.gameObject.GetComponent<Door>();
                if (intObj != null)
                {
                    intObj.Interact();
                }
                else if (door != null)
                {
                    door.Interact();
                }
                else
                {
                    intObj = hit.collider.gameObject.GetComponentInParent<InteractableObject>();
                    intObj.Interact();
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * pickupDistance);
    }
}
