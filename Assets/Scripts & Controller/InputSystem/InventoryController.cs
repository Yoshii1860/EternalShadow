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
    [Tooltip("The time between each move input")]
    [SerializeField] float _moveDebounceTime = 0.3f;

    [HideInInspector]
    public bool IsInventoryClosing = false;
    
    [Space(10)]
    [SerializeField] private bool _debugMode = false;


    private ItemActions _itemActions;

    // Input actions variables
    private bool _isInteracting, _isExiting, _isGoingBack, _isInteractingInspector;
    private Vector2 _movePos, _lookPos;
    private float _scrollValue;

    // Inventory counter variables
    private const float INVENTORY_CLOSE_TIME = 0.5f;
    private float _inventoryClosingCounter = 0f;
    private float _inspectorClosingCounter;

    #endregion




    #region Unity Callbacks

    // Called when the script instance is being loaded
    void Start() 
    {
        // Get the ItemActions component from the InventoryManager
        _itemActions = InventoryManager.Instance.GetComponent<ItemActions>();
    }

    #endregion




    #region Custom Update

    // Called every frame if the script is enabled
    public void CustomUpdate(float deltaTime)
    {       

        if (InventoryManager.Instance.IsInspecting)
        {
            HandleInspectorInput();
            return;
        }

        // Check if the game is in the Inventory state
        if (GameManager.Instance.CurrentGameState == GameManager.GameState.INVENTORY)
        {
            HandleInventoryInput(deltaTime);
        }

        // Check if the inventory is in the closing state
        if (IsInventoryClosing)
        {
            // Update the inventory closing timer
            _inventoryClosingCounter += deltaTime;

            // Check if the timer exceeds the specified time
            if (_inventoryClosingCounter >= INVENTORY_CLOSE_TIME)
            {
                // Reset the timer and exit the closing state
                _inventoryClosingCounter = 0f;
                IsInventoryClosing = false;
            }
        }
    }

    #endregion




    #region Input Handling Methods

    // Handle the input actions related to the inventory
    private void HandleInventoryInput(float deltaTime)
    {
        if (_inspectorClosingCounter > 0)
        {
            _inspectorClosingCounter -= deltaTime;
            _isInteracting = false;
            _isExiting = false;
            _isGoingBack = false;
            return;
        }
        
        // Process input actions related to the inventory
        if (_isInteracting)     Interact();
        if (_isExiting)         Exit();
        if (_isGoingBack)       Back();
    }

    // Handle the input actions related to the inspector
    private void HandleInspectorInput()
    {
        if (_isInteracting || _isGoingBack || _isExiting)
        {
            if (_debugMode) Debug.Log("InventoryController.CustomUpdate: ResumeFromInspector");
            InventoryManager.Instance.ResumeFromInspector();
            _inspectorClosingCounter = 0.5f;
        } 
        else 
        {
            LookAtInspectorItem();
        }

        if (_isInteractingInspector) InteractWithInspectorItem();
    }

    #endregion




    #region Input Callbacks

    // Called when the interact input action is triggered
    public void OnInteract(InputAction.CallbackContext context)
    {
        // Read the value of the interact button
        _isInteracting = context.ReadValueAsButton();
    }

    // Called when the exit input action is triggered
    public void OnExit(InputAction.CallbackContext context)
    {
        // Read the value of the exit button
        _isExiting = context.ReadValueAsButton();
    }

    // Called when the move input action is triggered
    public void OnMove(InputAction.CallbackContext context)
    {
        // Read the value of the move vector
        _movePos = context.ReadValue<Vector2>();

        if (InventoryManager.Instance.IsInspecting) return;
        if (InventoryManager.Instance.Items.Count == 0) return;

        // Debounce the move input
        float deltaTime = Time.deltaTime;
        if (Time.time - _moveDebounceTime > deltaTime)
        {
            _moveDebounceTime = Time.time;
            Move();
        }
    }

    // Called when the back input action is triggered
    public void OnBack(InputAction.CallbackContext context)
    {
        // Read the value of the back button
        _isGoingBack = context.ReadValueAsButton();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        _lookPos = context.ReadValue<Vector2>();
    }

    public void OnScroll(InputAction.CallbackContext context)
    {
        _scrollValue = context.ReadValue<float>();
        if (_debugMode) Debug.Log("InventoryController.OnScroll = " + _scrollValue);
    }

    public void OnInspectInteract(InputAction.CallbackContext context)
    {
        _isInteractingInspector = context.ReadValueAsButton();
    }

    #endregion




    #region Input Processing Methods

    // Process the inspect action
    private void LookAtInspectorItem()
    {
        InventoryManager.Instance.RotateInspectorItem(_lookPos.x, _lookPos.y, _scrollValue);
        _lookPos = Vector2.zero;
        _scrollValue = 0f;
    }

    // Process the interact action with the inspector item
    private void InteractWithInspectorItem()
    {
        if (_debugMode) Debug.Log("InventoryController.InteractWithInspectorItem");
        _isInteractingInspector = false;
        Interaction objInteraction = InventoryManager.Instance.ReturnInspectorItem().GetComponent<Interaction>();
        if (objInteraction != null)
        {
            if (_debugMode) Debug.Log("InventoryController.InteractWithInspectorItem: Interact");
            objInteraction.Interact();
        }
    }

    // Process the interact action
    private void Interact()
    {
        _isInteracting = false;
        if (_debugMode) Debug.Log("InventoryController.Interact");

        AudioManager.Instance.PlayClipOneShot(AudioManager.Instance.PlayerSpeaker2, "clicker", 0.3f);

        // Check if item actions are not open
        if (!InventoryManager.Instance.IsItemActionsOpen)
        {
            if (_debugMode) Debug.Log("InventoryController.Interact: ItemActionsNotOpen");
            InventoryManager.Instance.ShowItemDisplay();
        }
        else
        {
            if (_debugMode) Debug.Log("InventoryController.Interact: ItemActionsOpen");
            // Perform the selected item action
            switch (InventoryManager.Instance.ItemActionNumber)
            {
                case 0:
                    _itemActions.Use(InventoryManager.Instance.SelectedItem);
                    break;
                case 1:
                    _itemActions.Inspect(InventoryManager.Instance.SelectedItem);
                    return;
                case 2:
                    _itemActions.ThrowAway(InventoryManager.Instance.SelectedItem);
                    break;
            }

            InventoryManager.Instance.BackToSelection();
        }
    }

    // Process the exit action
    private void Exit()
    {
        if (_debugMode) Debug.Log("InventoryController.Exit");
        if (InventoryManager.Instance.IsInspecting)
        {
            if (_debugMode) Debug.Log("InventoryController.Exit: ResumeFromInspector");
            InventoryManager.Instance.ResumeFromInspector();
            _inspectorClosingCounter = 0.5f;
            return;
        }
        if (InventoryManager.Instance.IsItemActionsOpen)
        {
            InventoryManager.Instance.BackToSelection();
        }
        _isExiting = false;
        IsInventoryClosing = true;

        // Resume the game
        AudioManager.Instance.PlayClipOneShot(AudioManager.Instance.PlayerSpeaker2, "close inventory", 1f, 1f);
        GameManager.Instance.ResumeGame();
    }

    // Process the move action
    private void Move()
    {
        if (!InventoryManager.Instance.IsItemActionsOpen)
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
        _isGoingBack = false;
        InventoryManager.Instance.BackToSelection();
    }

    #endregion




    #region Movement Methods

    // Move the selection within the inventory
    private void MoveSelection()
    {
        if (_movePos.y > 0.5f)
        {
            MoveSelection(-4);
        }
        else if (_movePos.y < -0.5f)
        {
            MoveSelection(4);
        }
        else if (_movePos.x > 0.5f)
        {
            MoveSelection(1);
        }
        else if (_movePos.x < -0.5f)
        {
            MoveSelection(-1);
        }
    }

    // Move the selection up or down
    private void MoveSelection(int step)
    {
        if (InventoryManager.Instance.HighlightNumber + step >= 0 &&
            InventoryManager.Instance.HighlightNumber + step < InventoryManager.Instance.Items.Count)
        {
            AudioManager.Instance.PlayClipOneShot(AudioManager.Instance.PlayerSpeaker2, "click", 0.3f);
            InventoryManager.Instance.ChangeSelectedItemColor(true, false);
            InventoryManager.Instance.HighlightNumber += step;
            InventoryManager.Instance.ChangeSelectedItemColor(true, true);
        }
    }

    // Move the actions within the item actions menu
    private void MoveActions()
    {
        if (_movePos.y > 0.5f)
        {
            MoveActions(-1);
        }
        else if (_movePos.y < -0.5f)
        {
            MoveActions(1);
        }
    }

    // Move the actions up or down
    private void MoveActions(int step)
    {
        if (InventoryManager.Instance.ItemActionNumber + step >= 0 &&
            InventoryManager.Instance.ItemActionNumber + step < InventoryManager.Instance.ActionsChildCount)
        {
            AudioManager.Instance.PlayClipOneShot(AudioManager.Instance.PlayerSpeaker2, "click", 0.3f);
            InventoryManager.Instance.ChangeSelectedActionColor(false);
            InventoryManager.Instance.ItemActionNumber += step;
            InventoryManager.Instance.ChangeSelectedActionColor(true);
        }
    }

    #endregion
}