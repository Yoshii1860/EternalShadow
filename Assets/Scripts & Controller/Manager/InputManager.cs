using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    // Singleton instance
    static InputManager instance;

    PlayerInput playerInput;
    PlayerController playerController;
    MenuController menuController;
    InventoryController inventoryController;

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

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this.gameObject);

        playerInput = GetComponent<PlayerInput>();
        playerController = FindObjectOfType<PlayerController>();
        menuController = FindObjectOfType<MenuController>();
        inventoryController = FindObjectOfType<InventoryController>();

        // Iterate through all action maps
        foreach (var actionMap in playerInput.actions.actionMaps)
        {
            // Check if it's the action map you're interested in
            if (actionMap.name == "Player" && playerController != null)
            {
                // Check if it's the action you're interested in
                BindPlayerActions(actionMap);
            }
            else if (actionMap.name == "Menu" && menuController != null)
            {
                BindMenuActions(actionMap);
            }
            else if (actionMap.name == "Inventory" && inventoryController != null)
            {
                BindInventoryActions(actionMap);
            }
            else if (actionMap.name == "Pickup" && playerController != null)
            {
                BindPickupActions(actionMap);
            }
            else
            {
                Debug.LogWarning("Action map available but not bound: " + actionMap.name);
            }
        }
    }

    public void Rebind(string actionMapName)
    {
        foreach (var actionMap in playerInput.actions.actionMaps)
        {
            // Check if it's the action map you're interested in
            if (actionMap.name == "Player" && actionMapName == "Player")
            {
                // Check if it's the action you're interested in
                BindPlayerActions(actionMap);
            }
            else if (actionMap.name == "Menu" && actionMapName == "Menu")
            {
                BindMenuActions(actionMap);
            }
            else if (actionMap.name == "Inventory" && actionMapName == "Inventory")
            {
                BindInventoryActions(actionMap);
            }
            else if (actionMap.name == "Pickup" && actionMapName == "Pickup")
            {
                BindPickupActions(actionMap);
            }
            else
            {
                Debug.LogWarning("Action map available but not bound: " + actionMap.name);
            }
        }
    }

    void BindPlayerActions(InputActionMap actionMap)
    {
        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>();
        }
        // Subscribe to actions within this action map
        foreach (var action in actionMap.actions)
        {
            if (action.name == "Move")
            {
                action.performed += playerController.OnMove;
                action.canceled += playerController.OnMove;
            }
            else if (action.name == "Look")
            {
                action.performed += playerController.OnLook;
                action.canceled += playerController.OnLook;
            }
            else if (action.name == "Fire")
            {
                action.performed += playerController.OnFire;
            }
            else if (action.name == "Sprint")
            {   
                action.performed += playerController.OnSprint;
            }
            else if (action.name == "Crouch")
            {
                action.performed += playerController.OnCrouch;
            }
            else if (action.name == "Aim")
            {
                action.performed += playerController.OnAim;
            }
            else if (action.name == "Interact")
            {
                action.performed += playerController.OnInteract;
            }
            else if (action.name == "Light")
            {
                action.performed += playerController.OnLight;
            }
            else if (action.name == "WeaponSwitch")
            {
                action.performed += playerController.OnWeaponSwitch;
            }
            else if (action.name == "Reload")
            {
                action.performed += playerController.OnReload;
            }
            else if (action.name == "Inventory")
            {
                action.performed += playerController.OnInventory;
            }
            else if (action.name == "Menu")
            {
                action.performed += playerController.OnMenu;
            }
            else
            {
                Debug.LogWarning("Action available but not bound on " + actionMap.name + ": " + action.name);
            }
        }
    }

    void BindMenuActions(InputActionMap actionMap)
    {
        if (menuController == null)
        {
            menuController = FindObjectOfType<MenuController>();
        }
        // Subscribe to actions within this action map
        foreach (var action in actionMap.actions)
        {
            // Check if it's the action you're interested in
            if (action.name == "Move")
            {
                action.performed += menuController.OnMove;
            }
            else if (action.name == "Interact")
            {
                action.performed += menuController.OnInteract;
            }
            else if (action.name == "Back")
            {
                action.performed += menuController.OnBack;
            }
            else
            {
                Debug.LogWarning("Action available but not bound on " + actionMap.name + ": " + action.name);
            }
        }
    }

    void BindInventoryActions(InputActionMap actionMap)
    {
        if (inventoryController == null)
        {
            inventoryController = FindObjectOfType<InventoryController>();
        }
        // Subscribe to actions within this action map
        foreach (var action in actionMap.actions)
        {
            // Check if it's the action you're interested in
            if (action.name == "Move")
            {
                action.performed += inventoryController.OnMove;
            }
            else if (action.name == "Interact")
            {
                action.performed += inventoryController.OnInteract;
            }
            else if (action.name == "Back")
            {
                action.performed += inventoryController.OnBack;
            }
            else if (action.name == "Exit")
            {
                action.performed += inventoryController.OnExit;
            }
            else
            {
                Debug.LogWarning("Action available but not bound on " + actionMap.name + ": " + action.name);
            }
        }
    }

    void BindPickupActions(InputActionMap actionMap)
    {
        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>();
        }
        // Subscribe to actions within this action map
        foreach (var action in actionMap.actions)
        {
            // Check if it's the action you're interested in
            if (action.name == "Interact")
            {
                action.performed += playerController.OnInteract;
            }
            else
            {
                Debug.LogWarning("Action available but not bound on " + actionMap.name + ": " + action.name);
            }
        }
    }
}
