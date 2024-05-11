using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    #region Singleton

    // Singleton instance
    static InputManager instance;

    // Create a static reference to the instance
    public static InputManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<InputManager>();

                if (instance == null)
                {
                    GameObject obj = new GameObject("InputManager");
                    instance = obj.AddComponent<InputManager>();
                }
            }

            return instance;
        }
    }

    #endregion




    #region Fields

    private PlayerInput _playerInput;
    private PlayerController _playerController;
    private MenuController _menuController;
    private InventoryController _inventoryController;
    private PaintingController _paintingController;
    [SerializeField] private bool _debugMode;

    #endregion




    #region Initialization

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this.gameObject);

        _playerInput = GetComponent<PlayerInput>();
    }

    public void UpdateReferences()
    {
        _playerController = FindObjectOfType<PlayerController>();
        _menuController = FindObjectOfType<MenuController>();
        _inventoryController = FindObjectOfType<InventoryController>();
        _paintingController = FindObjectOfType<PaintingController>();
        BindAllActionMaps();
    }

    #endregion




    #region Action Map Binding

    void BindAllActionMaps()
    {
        // Iterate through all action maps
        foreach (var actionMap in _playerInput.actions.actionMaps)
        {
            // Check if it's the action map you're interested in
            if (actionMap.name == "Player" && _playerController != null)
            {
                // Check if it's the action you're interested in
                BindPlayerActions(actionMap);
            }
            else if (actionMap.name == "Menu" && _menuController != null)
            {
                BindMenuActions(actionMap);
            }
            else if (actionMap.name == "Inventory" && _inventoryController != null)
            {
                BindInventoryActions(actionMap);
            }
            else if (actionMap.name == "Pickup" && _playerController != null)
            {
                BindPickupActions(actionMap);
            }
            else if (actionMap.name == "Painting" && _paintingController != null)
            {
                BindPaintingActions(actionMap);
            }
            else
            {
                if (_debugMode) Debug.LogWarning("Action map available but not bound: " + actionMap.name);
            }
        }

        if (_debugMode) Debug.Log("Bind all action maps");
    }

    #endregion




    #region Player Input Actions

    void BindPlayerActions(InputActionMap actionMap)
    {
        _playerController = FindObjectOfType<PlayerController>();
        
        // Subscribe to actions within this action map
        foreach (var action in actionMap.actions)
        {
            if (action.name == "Move")
            {
                action.performed += _playerController.OnMove;
                action.canceled += _playerController.OnMove;
            }
            else if (action.name == "Look")
            {
                action.performed += _playerController.OnLook;
                action.canceled += _playerController.OnLook;
            }
            else if (action.name == "Fire")
            {
                action.performed += _playerController.OnFire;
            }
            else if (action.name == "Sprint")
            {   
                action.performed += _playerController.OnSprint;
            }
            else if (action.name == "Crouch")
            {
                action.performed += _playerController.OnCrouch;
            }
            else if (action.name == "Aim")
            {
                action.performed += _playerController.OnAim;
            }
            else if (action.name == "Interact")
            {
                action.performed += _playerController.OnInteract;
            }
            else if (action.name == "Light")
            {
                action.performed += _playerController.OnLight;
            }
            else if (action.name == "WeaponSwitch")
            {
                action.performed += _playerController.OnWeaponSwitch;
            }
            else if (action.name == "Reload")
            {
                action.performed += _playerController.OnReload;
            }
            else if (action.name == "Inventory")
            {
                action.performed += _playerController.OnInventory;
            }
            else if (action.name == "Menu")
            {
                action.performed += _playerController.OnMenu;
            }
            else
            {
                if (_debugMode) Debug.LogWarning("Action available but not bound on " + actionMap.name + ": " + action.name);
            }
        }

        if (_debugMode) Debug.Log("Player actions bound");
    }

    #endregion




    #region Menu Input Actions

    void BindMenuActions(InputActionMap actionMap)
    {
        if (_menuController == null)
        {
            _menuController = FindObjectOfType<MenuController>();
        }
        // Subscribe to actions within this action map
        foreach (var action in actionMap.actions)
        {
            // Check if it's the action you're interested in
            if (action.name == "Move")
            {
                action.performed += _menuController.OnMove;
            }
            else if (action.name == "Interact")
            {
                action.performed += _menuController.OnInteract;
            }
            else if (action.name == "Back")
            {
                action.performed += _menuController.OnBack;
            }
            else if (action.name == "Scroll")
            {
                action.performed += _menuController.OnScroll;
            }
            else
            {
                if (_debugMode) Debug.LogWarning("Action available but not bound on " + actionMap.name + ": " + action.name);
            }
        }

        if (_debugMode) Debug.Log("Menu actions bound");
    }

    #endregion




    #region Inventory Input Actions

    void BindInventoryActions(InputActionMap actionMap)
    {
        if (_inventoryController == null)
        {
            _inventoryController = FindObjectOfType<InventoryController>();
        }
        // Subscribe to actions within this action map
        foreach (var action in actionMap.actions)
        {
            // Check if it's the action you're interested in
            if (action.name == "Move")
            {
                action.performed += _inventoryController.OnMove;
            }
            else if (action.name == "Interact")
            {
                action.performed += _inventoryController.OnInteract;
            }
            else if (action.name == "Back")
            {
                action.performed += _inventoryController.OnBack;
            }
            else if (action.name == "Exit")
            {
                action.performed += _inventoryController.OnExit;
            }
            else if (action.name == "Look")
            {
                action.performed += _inventoryController.OnLook;
            }
            else if (action.name == "Scroll")
            {
                action.performed += _inventoryController.OnScroll;
            }
            else if (action.name == "InspectInteract")
            {
                action.performed += _inventoryController.OnInspectInteract;
            }
            else
            {
                if (_debugMode) Debug.LogWarning("Action available but not bound on " + actionMap.name + ": " + action.name);
            }
        }

        if (_debugMode) Debug.Log("Inventory actions bound");
    }

    #endregion




    #region Pickup Input Actions

    void BindPickupActions(InputActionMap actionMap)
    {
        if (_playerController == null)
        {
            _playerController = FindObjectOfType<PlayerController>();
        }
        // Subscribe to actions within this action map
        foreach (var action in actionMap.actions)
        {
            // Check if it's the action you're interested in
            if (action.name == "Interact")
            {
                action.performed += _playerController.OnInteract;
            }
            else
            {
                if (_debugMode) Debug.LogWarning("Action available but not bound on " + actionMap.name + ": " + action.name);
            }
        }

        if (_debugMode) Debug.Log("Pickup actions bound");
    }

    #endregion




    #region Painting Input Actions

    void BindPaintingActions(InputActionMap actionMap)
    {
        if (_paintingController == null)
        {
            _paintingController = FindObjectOfType<PaintingController>();
        }
        // Subscribe to actions within this action map
        foreach (var action in actionMap.actions)
        {
            // Check if it's the action you're interested in
            if (action.name == "Move")
            {
                action.performed += _paintingController.OnMove;
            }
            else if (action.name == "Interact")
            {
                action.performed += _paintingController.OnInteract;
            }
            else if (action.name == "Back")
            {
                action.performed += _paintingController.OnBack;
            }
            else
            {
                Debug.LogWarning("Action available but not bound on " + actionMap.name + ": " + action.name);
            }
        }

        if (_debugMode) Debug.Log("Painting actions bound");
    }

    #endregion
}
