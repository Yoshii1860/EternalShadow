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
    public bool lightSwitchActive = false;
    LayerMask allLayersExceptCharacter = ~(1 << 7);

    #endregion

    #region Unity Lifecycle Methods

    void Start()
    {
        // Initialize references on Start
        UpdateReferences();
    }

    #endregion

    #region Public Methods

    /// Execute the ObjectHandler functionality.
    public void Execute()
    {
        DetectObjects();
    }

    /// Update references to other components.
    public void UpdateReferences()
    {
        action = GetComponent<ItemActions>();
    }

    #endregion

    #region Private Methods

    /// Detect objects in front of the camera.
    private void DetectObjects()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, pickupDistance, allLayersExceptCharacter))
        {
            Debug.Log("ObjectHandler.DetectObjects: " + hit.collider.gameObject.name);

            if (hit.collider.gameObject.tag == "Save")
            {
                // Open UI to save game
                Debug.Log("ObjectHandler - Save");
                GameManager.Instance.OpenSaveScreen();
                return;
            }

            PaintingEvent paintingEvent = hit.collider.gameObject.GetComponent<PaintingEvent>();

            if (paintingEvent != null)
            {
                GameManager.Instance.PaintingEvent();
                return;
            }

            ItemController itemController = hit.collider.gameObject.GetComponent<ItemController>();

            if (itemController != null)
            {
                if (itemController.inventoryItem)
                {
                    if (InventoryCheck()) return;
                }
                itemController.Interact();
                // Use PickUp() method from ItemActions class
                Debug.Log("ObjectHandler - PickUp");
                action.PickUp(hit);
                return;
            }

            if (hit.collider.gameObject.tag == "Save")
            {
                // Open UI to save game
                Debug.Log("ObjectHandler - Save");
                GameManager.Instance.OpenSaveScreen();
                return;
            }

            if (hit.collider.gameObject.layer == 6)
            {
                Debug.Log("ObjectHandler - Interact");

                InteractableObject intObj = hit.collider.gameObject.GetComponent<InteractableObject>();
                if (intObj != null)
                {
                    if (intObj.inventoryItem)
                    {
                        if (InventoryCheck()) return;
                    }
                    intObj.Interact();
                    return;
                }
                
                Door door = hit.collider.gameObject.GetComponent<Door>();
                if (door != null)
                {
                    door.Interact();
                    return;
                }

                TextCode textCode = hit.collider.gameObject.GetComponent<TextCode>();
                if (textCode != null)
                {
                    textCode.ReadText();
                    return;
                }

                LightSwitch lightSwitch = hit.collider.gameObject.GetComponent<LightSwitch>();
                if (lightSwitch != null)
                {
                    lightSwitch.SwitchLights();
                    return;
                }
                
                intObj = hit.collider.gameObject.GetComponentInParent<InteractableObject>();
                intObj.Interact();
            }
        }
    }

    private bool InventoryCheck()
    {
        // Check if the player has space in the inventory
        if (InventoryManager.Instance.ItemCount() >= InventoryManager.Instance.maxItems)
        {
            AudioManager.Instance.PlaySoundOneShot(AudioManager.Instance.playerSpeaker2, "error", 0.6f, 1f);
            return true;
        }
        return false;
    }

    /// Draw a ray in the Scene view for visualization.
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * pickupDistance);
    }

    #endregion
}