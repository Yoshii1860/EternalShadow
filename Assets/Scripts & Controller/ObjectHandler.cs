using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectHandler : MonoBehaviour
{
    // [SerializeField] private InputActionReference input;
    [SerializeField] float pickupDistance = 5f;
    [SerializeField] GameObject saveCanvas;
    [SerializeField] GameObject loadCanvas;
    ItemActions action;

    void Start() 
    {
        action = GetComponent<ItemActions>();
    }

    public void Execute()
    {
        DetectObjects();
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
                saveCanvas.SetActive(true);
                GameManager.Instance.PauseGame();
            }
            else if (hit.collider.gameObject.tag == "Load")
            {
                // Open UI with saved files
                Debug.Log("ObjectHandler - Load");
                loadCanvas.SetActive(true);
                GameManager.Instance.PauseGame();
            }
            else if (hit.collider.gameObject.layer == 6)
            {
                Debug.Log("ObjectHandler - Interact");
                InteractableObject intObj = hit.collider.gameObject.GetComponent<InteractableObject>();
                if (intObj != null)
                {
                    intObj.Interact();
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
