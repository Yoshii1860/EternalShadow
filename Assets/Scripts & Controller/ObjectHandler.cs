using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectHandler : MonoBehaviour
{
    #region Fields

    // [SerializeField] private InputActionReference input;
    [SerializeField] float pickupDistance = 5f;
    private ItemActions action;

    #endregion

    #region Unity Lifecycle Methods

    void Start()
    {
        // Initialize references on Start
        UpdateReferences();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Execute the ObjectHandler functionality.
    /// </summary>
    public void Execute()
    {
        DetectObjects();
    }

    /// <summary>
    /// Update references to other components.
    /// </summary>
    public void UpdateReferences()
    {
        action = GetComponent<ItemActions>();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Detect objects in front of the camera.
    /// </summary>
    private void DetectObjects()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, pickupDistance))
        {
            Debug.Log("ObjectHandler.DetectObjects: " + hit.collider.gameObject.name);
            ItemController itemController = hit.collider.gameObject.GetComponent<ItemController>();
            InteractableObject intObj = hit.collider.gameObject.GetComponent<InteractableObject>();
            Door door = hit.collider.gameObject.GetComponent<Door>();

            if (itemController != null)
            {
                intObj.Interact();
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

    /// <summary>
    /// Draw a ray in the Scene view for visualization.
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * pickupDistance);
    }

    #endregion
}