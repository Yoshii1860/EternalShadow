using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using Cinemachine;
using UnityEngine.UI;

public class InventoryController : MonoBehaviour, ICustomUpdatable
{
    #region Fields

    [Header("Input Settings")]
    public float moveDebounceTime = 0.3f;
    public LayerMask pushLayers;
    public bool canPush;
    [Range(0.5f, 5f)] public float strength = 1.1f;

    private ItemActions itemActions;

    private bool interact, exit, back, inspectInteract;
    private Vector2 move, look;
    private float scroll;

    public bool inventoryClosing = false;
    private float inventoryCounter = 0f;
    private float inventoryTimer = 0.5f;

    private float inspectorTimer;

    #endregion

    #region Unity Callbacks

    // Called when the script instance is being loaded
    void Start() 
    {
        // Get the ItemActions component from the InventoryManager
        itemActions = InventoryManager.Instance.GetComponent<ItemActions>();
    }

    // Called every frame if the script is enabled
    public void CustomUpdate(float deltaTime)
    {       
        if (InventoryManager.Instance.isInspecting)
        {
            if (interact || back || exit)
            {
                Debug.Log("InventoryController.CustomUpdate: ResumeFromInspector");
                InventoryManager.Instance.ResumeFromInspector();
                inspectorTimer = 0.5f;
            } else Inspect();

            if (inspectInteract) InspectInteract();
            
            return;
        }

        // Check if the game is in the Inventory state
        if (GameManager.Instance.CurrentGameState == GameManager.GameState.Inventory)
        {
            if (inspectorTimer > 0)
            {
                inspectorTimer -= deltaTime;
                interact = false;
                exit = false;
                back = false;
                return;
            }
            
            // Process input actions related to the inventory
            if (interact)   Interact();
            if (exit)       Exit();
            if (back)       Back();
        }

        // Check if the inventory is in the closing state
        if (inventoryClosing)
        {
            // Update the inventory closing timer
            inventoryCounter += deltaTime;

            // Check if the timer exceeds the specified time
            if (inventoryCounter >= inventoryTimer)
            {
                // Reset the timer and exit the closing state
                inventoryCounter = 0f;
                inventoryClosing = false;
            }
        }
    }

    #endregion

    #region Input Callbacks

    // Called when the interact input action is triggered
    public void OnInteract(InputAction.CallbackContext context)
    {
        // Read the value of the interact button
        interact = context.ReadValueAsButton();
    }

    // Called when the exit input action is triggered
    public void OnExit(InputAction.CallbackContext context)
    {
        // Read the value of the exit button
        exit = context.ReadValueAsButton();
    }

    // Called when the move input action is triggered
    public void OnMove(InputAction.CallbackContext context)
    {
        // Read the value of the move vector
        move = context.ReadValue<Vector2>();

        if (InventoryManager.Instance.isInspecting) return;
        if (InventoryManager.Instance.Items.Count == 0) return;

        // Debounce the move input
        float deltaTime = Time.deltaTime;
        if (Time.time - moveDebounceTime > deltaTime)
        {
            moveDebounceTime = Time.time;
            Move();
        }
    }

    // Called when the back input action is triggered
    public void OnBack(InputAction.CallbackContext context)
    {
        // Read the value of the back button
        back = context.ReadValueAsButton();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        look = context.ReadValue<Vector2>();
    }

    public void OnScroll(InputAction.CallbackContext context)
    {
        scroll = context.ReadValue<float>();
        Debug.Log("InventoryController.OnScroll = " + scroll);
    }

    public void OnInspectInteract(InputAction.CallbackContext context)
    {
        inspectInteract = context.ReadValueAsButton();
    }

    #endregion

    #region Input Processing Methods

    // Process the inspect action
    private void Inspect()
    {
        InventoryManager.Instance.RotateInspectorItem(look.x, look.y, scroll);
        look = Vector2.zero;
        scroll = 0f;
    }

    private void InspectInteract()
    {
        Debug.Log("InventoryController.InspectInteract");
        inspectInteract = false;
        Interaction objInteraction = InventoryManager.Instance.ReturnInspectorItem().GetComponent<Interaction>();
        if (objInteraction != null)
        {
            Debug.Log("InventoryController.InspectInteract: Interact");
            objInteraction.Interact();
        }
    }

    // Process the interact action
    private void Interact()
    {
        interact = false;
        Debug.Log("InventoryController.Interact");

        // Check if item actions are not open
        if (!InventoryManager.Instance.itemActionsOpen)
        {
            Debug.Log("InventoryController.Interact: ItemActionsNotOpen");
            InventoryManager.Instance.ShowItemDisplay();
        }
        else
        {
            Debug.Log("InventoryController.Interact: ItemActionsOpen");
            // Perform the selected item action
            switch (InventoryManager.Instance.itemActionNumber)
            {
                case 0:
                    itemActions.Use(InventoryManager.Instance.selectedItem);
                    break;
                case 1:
                    itemActions.Inspect(InventoryManager.Instance.selectedItem);
                    return;
                case 2:
                    itemActions.ThrowAway(InventoryManager.Instance.selectedItem);
                    break;
                /*
                case 2:
                    itemActions.Combine(InventoryManager.Instance.selectedItem);
                    break;
                */
            }

            InventoryManager.Instance.BackToSelection();
        }
    }

    // Process the exit action
    private void Exit()
    {
        Debug.Log("InventoryController.Exit");
        if (InventoryManager.Instance.isInspecting)
        {
            Debug.Log("InventoryController.Exit: ResumeFromInspector");
            InventoryManager.Instance.ResumeFromInspector();
            inspectorTimer = 0.5f;
            return;
        }
        if (InventoryManager.Instance.itemActionsOpen)
        {
            InventoryManager.Instance.BackToSelection();
        }
        exit = false;
        inventoryClosing = true;

        // Resume the game
        AudioManager.Instance.PlaySoundOneShot(AudioManager.Instance.playerSpeaker2, "close inventory", 1f, 1f);
        GameManager.Instance.ResumeGame();
    }

    // Process the move action
    private void Move()
    {
        if (!InventoryManager.Instance.itemActionsOpen)
        {
            MoveSelection();
        }
        else
        {
            MoveActions();
        }
    }

    // Process the back action
    private void Back()
    {
        back = false;
        InventoryManager.Instance.BackToSelection();
    }

    #endregion

    #region Movement Methods

    // Move the selection within the inventory
    private void MoveSelection()
    {
        if (move.y > 0.5f)
        {
            MoveSelectionUpDown(-4);
        }
        else if (move.y < -0.5f)
        {
            MoveSelectionUpDown(4);
        }
        else if (move.x > 0.5f)
        {
            MoveSelectionRightLeft(1);
        }
        else if (move.x < -0.5f)
        {
            MoveSelectionRightLeft(-1);
        }
    }

    // Move the selection up or down
    private void MoveSelectionUpDown(int step)
    {
        if (InventoryManager.Instance.highlightNumber + step >= 0 &&
            InventoryManager.Instance.highlightNumber + step < InventoryManager.Instance.Items.Count)
        {
            InventoryManager.Instance.ChangeSelectedItemColor(true, false);
            InventoryManager.Instance.highlightNumber += step;
            InventoryManager.Instance.ChangeSelectedItemColor(true, true);
        }
    }

    // Move the selection right or left
    private void MoveSelectionRightLeft(int step)
    {
        if (InventoryManager.Instance.highlightNumber + step >= 0 &&
            InventoryManager.Instance.highlightNumber + step < InventoryManager.Instance.Items.Count)
        {
            InventoryManager.Instance.ChangeSelectedItemColor(true, false);
            InventoryManager.Instance.highlightNumber += step;
            InventoryManager.Instance.ChangeSelectedItemColor(true, true);
        }
    }

    // Move the actions within the item actions menu
    private void MoveActions()
    {
        if (move.y > 0.5f)
        {
            MoveActionsUpDown(-1);
        }
        else if (move.y < -0.5f)
        {
            MoveActionsUpDown(1);
        }
    }

    // Move the actions up or down
    private void MoveActionsUpDown(int step)
    {
        if (InventoryManager.Instance.itemActionNumber + step >= 0 &&
            InventoryManager.Instance.itemActionNumber + step < InventoryManager.Instance.actionsChildCount)
        {
            InventoryManager.Instance.ChangeSelectedActionColor(false);
            InventoryManager.Instance.itemActionNumber += step;
            InventoryManager.Instance.ChangeSelectedActionColor(true);
        }
    }

    #endregion
}