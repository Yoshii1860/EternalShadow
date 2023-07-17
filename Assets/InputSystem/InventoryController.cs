using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using Cinemachine;

public class InventoryController : MonoBehaviour
{
    private List<RaycastResult> m_RaycastResults = new List<RaycastResult>();
    bool interact, exit;

    void Update()
    {
        if (GameManager.Instance.CurrentGameState == GameManager.GameState.Inventory)
        {
            if (interact) Interact();
            if (exit) Exit();
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        interact = context.ReadValueAsButton();
    }

    public void OnExit(InputAction.CallbackContext context)
    {
        exit = context.ReadValueAsButton();
    }

    void Interact()
    {
        interact = false;
        Debug.Log("Interact");

        // Check if the current pointer event is over a UI element
        if (EventSystem.current.IsPointerOverGameObject())
        {
            // Retrieve information about the object under the pointer
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            pointerData.position = Mouse.current.position.ReadValue();
            EventSystem.current.RaycastAll(pointerData, m_RaycastResults);

            // Retrieve the root element of the UI element under the pointer
            if (m_RaycastResults.Count > 0)
            {
                GameObject pointerTarget = m_RaycastResults[0].gameObject;
                Item item = null;
                // If pointerTarget has no component called ItemController, get the parent object
                if (pointerTarget.GetComponent<ItemController>() == null)
                {
                    pointerTarget = pointerTarget.transform.parent.gameObject;
                    // Get the ItemController component from child object
                    foreach (Transform child in pointerTarget.transform)
                    {
                        if (child.GetComponent<ItemController>() != null)
                        {
                            pointerTarget = child.gameObject;
                            // Get item from ItemController
                            item = pointerTarget.GetComponent<ItemController>().item;
                            break;
                        }
                    }
                }
                else
                {
                    // Get item from ItemController
                    item = pointerTarget.GetComponent<ItemController>().item;
                }
                // Send item to InventoryManager to handle display
                InventoryManager.Instance.ShowItemDisplay(item);
            }

            m_RaycastResults.Clear();
        }
    }

    void Exit()
    {
        exit = false;
        Debug.Log("Exit");

        GameManager.Instance.ResumeGame();
    }
}