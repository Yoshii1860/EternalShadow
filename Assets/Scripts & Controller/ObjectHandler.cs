using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectHandler : MonoBehaviour
{
    #region Fields

    [Header("Object Detection Settings")]
    [Tooltip("The distance to detect objects in front of the camera")]
    [SerializeField] private float _interactionDistance  = 5f;
    // Reference to the ItemActions component
    private ItemActions _itemAction;
    // Layer mask to ignore the character layer
    private LayerMask _allLayersExceptCharacter = ~(1 << 7);
    // Dictionary to map object tags to actions
    private Dictionary<string, Action> _interactionMap;
    // Raycast hit information
    private RaycastHit interactionHit;  

    #endregion




    #region Unity Lifecycle Methods

    void Start()
    {
        // Initialize references on Start
        UpdateReferences();
    }

    #endregion




    #region Public Methods

    // Performs the object interaction logic based on raycast hit
    // This method is called from the PlayerController component.
    public void Execute()
    {
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out interactionHit, _interactionDistance, _allLayersExceptCharacter))
        {
            if (_interactionMap.TryGetValue(interactionHit.collider.gameObject.tag, out Action Interaction))
            {
                Interaction();
            }
        }
    }

    /// Update references to other components.
    /// To be called when the GameManager is initialized.
    public void UpdateReferences()
    {
        _itemAction = GetComponent<ItemActions>();

        _interactionMap = new Dictionary<string, Action>()
        {
            { "Save", () => GameManager.Instance.OpenSaveScreen() },
            { "Painting", () => GameManager.Instance.PaintingEvent() },
            { "Item", () => 
                {
                    var itemController = interactionHit.collider.GetComponent<ItemController>();
                    if (itemController != null)
                    {
                        if (itemController.InventoryItem)
                        {
                            if (InventoryManager.Instance.InventorySpaceCheck()) 
                            {
                                AudioManager.Instance.PlayClipOneShot(AudioManager.Instance.PlayerSpeaker2, "error", 0.6f, 1f);
                                return;
                            }
                        }
                        itemController.Interact();
                        _itemAction.PickUp(interactionHit);
                    }   
                }
            },
            { "Door", () => 
                {
                    var door = interactionHit.collider.GetComponent<Door>();
                    if (door != null)
                    {
                        door.Interact();
                    }
                }
            },
            { "Text", () => 
                {
                    var textCode = interactionHit.collider.GetComponent<TextCode>();
                    if (textCode != null)
                    {
                        textCode.ReadText();
                    }
                }
            },
            { "Locker", () => 
                {
                    var lockerCode = interactionHit.collider.GetComponent<LockerCode>();
                    if (lockerCode != null)
                    {
                        lockerCode.Interact();
                    }
                    else
                    {
                        interactionHit.collider.transform.parent.GetComponent<LockerCode>().Interact();
                    }
                }
            },
            { "LightSwitch", () => 
                {
                    var lightSwitch = interactionHit.collider.GetComponent<LightSwitch>();
                    if (lightSwitch != null)
                    {
                        lightSwitch.SwitchLights();
                    }
                }
            },
            { "EventTrigger", () =>
                {
                    var interactableEventObject = interactionHit.collider.GetComponent<InteractableObject>();
                    if (interactableEventObject != null)
                    {
                        interactableEventObject.Interact();
                    }
                }

            },
            { "Untagged", () => 
                {
                    var interactableObject = interactionHit.collider.GetComponent<InteractableObject>();
                    if (interactableObject != null)
                    {
                        interactableObject.Interact();
                    }
                }
            }
        };
    }

    #endregion




    #region Gizmos

    /// Draw a ray in the Scene view for visualization.
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * _interactionDistance);
    }

    #endregion
}