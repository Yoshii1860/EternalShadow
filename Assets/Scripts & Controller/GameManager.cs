using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState
    {
        MainMenu,
        Gameplay,
        Inventory,
        Paused,
        GameOver
    }

    public enum SubGameState
    {
        Default,
        Substate1,
        Substate2
    }

    public GameState CurrentGameState { get; private set; }
    public SubGameState CurrentSubGameState { get; private set; }

    [SerializeField] GameObject inventoryObject;
    [SerializeField] PlayerInput playerInput;
    [SerializeField] Transform objectPool;
    [SerializeField] Transform enemyPool;
    [SerializeField] Transform interactableObjectsPool;

    public bool isPaused = false;

    [Header("Debug")]
    [Tooltip("Debug mode for the enemy. If true, the enemy will not attack the player.")]
    public bool noAttackMode = false;

    private void Awake()
    {
        // Singleton pattern to ensure only one instance exists
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Keep the GameManager between scene changes
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Initialize the game
        SetGameState(GameState.Gameplay);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void StartGame()
    {
        SetGameState(GameState.Gameplay);
        // Add code to initialize the gameplay
        playerInput.SwitchCurrentActionMap("Player");
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Inventory()
    {
        SetGameState(GameState.Inventory);
        // Add code to display the inventory
        playerInput.SwitchCurrentActionMap("Inventory");
        inventoryObject.SetActive(true);
        InventoryManager.Instance.ListItems();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void PauseGame()
    {
        SetGameState(GameState.Paused);
        // Add code to pause the game
        Time.timeScale = 0;
        isPaused = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        SetGameState(GameState.Gameplay);
        // Add code to resume the game
        if (inventoryObject.activeSelf) inventoryObject.SetActive(false);
        playerInput.SwitchCurrentActionMap("Player");
        Time.timeScale = 1;
        isPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
    }

    public void GameOver()
    {
        SetGameState(GameState.GameOver);
        // Add code for game over logic and display game over screen
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void SaveData(Player player)
    {
        SaveSystem.SavePlayer(player, InventoryManager.Instance.weapons.transform, objectPool, enemyPool, interactableObjectsPool);
        Debug.Log("GameManager.cs: Player data saved!");
    }

    public void LoadData(Player player)
    {
        SaveData data = SaveSystem.LoadPlayer();
        if (data != null)
        {
            ////////////////////////////
            // Load Player Stats
            ////////////////////////////
            player.health = data.health;
            Vector3 position = new Vector3(data.position[0], data.position[1], data.position[2]);
            player.transform.position = position;

            ////////////////////////////
            // Load Inventory
            ////////////////////////////
            InventoryManager.Instance.Items.Clear();

            foreach (ItemData itemData in data.items)
            {
                // Convert ItemData back to Item and add it to the inventory
                Item item = CreateItemFromData(itemData);
                InventoryManager.Instance.AddItem(item);
            }

            ////////////////////////////
            // Load Weapons
            ////////////////////////////
            foreach (WeaponData weaponData in data.weapons)
            {
                // Convert WeaponData back to Weapon and add it to the inventory
                Weapon weapon = InventoryManager.Instance.weapons.transform.GetChild(weaponData.index).GetComponent<Weapon>();
                weapon.magazineCount = weaponData.magazineCount;
                weapon.isAvailable = weaponData.isAvailable;
                if (weaponData.isEquipped) weapon.gameObject.SetActive(true);
                else weapon.gameObject.SetActive(false);
            }

            ////////////////////////////
            // Set Ammo
            ////////////////////////////
            Ammo ammoSlot = player.GetComponent<Ammo>();
            ammoSlot.ResetAmmo();

            foreach (Item item in InventoryManager.Instance.Items)
            {
                if (item.type == ItemType.Ammo)
                {
                    ammoSlot.IncreaseCurrentAmmo(item.AmmoType, item.quantity);
                }
            }

            foreach (Weapon weapon in InventoryManager.Instance.weapons.transform.GetComponentsInChildren<Weapon>(true))
            {
                if (weapon.isAvailable)
                {
                    ammoSlot.IncreaseCurrentAmmo(weapon.ammoType, weapon.magazineCount);
                }

                if (weapon.isEquipped)
                {
                    // Set the UI for the current weapon
                    int inventoryAmmo = InventoryManager.Instance.GetInventoryAmmo(weapon.ammoType);
                    player.SetBulletsUI(weapon.magazineCount, inventoryAmmo);
                }
            }


            ////////////////////////////
            // Load Pickup Objects
            ////////////////////////////
            ItemController[] pickupObjectsPool = objectPool.GetComponentsInChildren<ItemController>();

            foreach (PickupObjectData pickupObjectData in data.pickupObjects)
            {
                foreach (ItemController pickupObject in pickupObjectsPool)
                {
                    if (pickupObject.uniqueID == pickupObjectData.uniqueID)
                    {
                        pickupObject.isPickedUp = pickupObjectData.isPickedUp;
                        if (pickupObject.isPickedUp)
                        {
                            // Move the object to a position far away
                            pickupObject.transform.position = new Vector3(0, -1000f, 0);
                        }
                        else
                        {
                            // If it's not picked up, reset its position to its initial position.
                            pickupObject.transform.position = pickupObject.originalPosition;
                        }
                    }
                }
            }

            ////////////////////////////
            // Load Enemies
            ////////////////////////////
            Enemy[] enemiesPool = enemyPool.GetComponentsInChildren<Enemy>(true);

            foreach (EnemyData enemyData in data.enemies)
            {
                foreach (Enemy enemy in enemiesPool)
                {
                    if (enemy.uniqueID == enemyData.uniqueID)
                    {
                        enemy.health = enemyData.health;
                        enemy.isDead = enemyData.isDead;
                        enemy.transform.position = new Vector3(enemyData.position[0], enemyData.position[1], enemyData.position[2]);
                        enemy.transform.rotation = Quaternion.Euler(enemyData.rotation[0], enemyData.rotation[1], enemyData.rotation[2]);
                        if (enemy.isDead)
                        {
                            enemy.gameObject.SetActive(false);
                        }
                        else
                        {
                            enemy.gameObject.SetActive(true);
                        }
                        enemy.GetComponent<EnemyBT>().ResetTree();
                    }
                }
            }

            ////////////////////////////
            // Load Interactable Objects
            ////////////////////////////
            InteractableObject[] interactableObjectPool = interactableObjectsPool.GetComponentsInChildren<InteractableObject>(true);

            foreach (InteractableObjectData intObjData in data.interactableObjects)
            {
                foreach (InteractableObject intObj in interactableObjectPool)
                {
                    if (intObj.uniqueID == intObjData.uniqueID)
                    {
                        if (intObj.active != intObjData.active)
                        {
                            intObj.Interact();
                        }
                    }
                }
            }



            Debug.Log("GameManager.cs: Player data loaded!");
        }
    }

    private Item CreateItemFromData(ItemData itemData)
    {
        Item newItem = ScriptableObject.CreateInstance<Item>();
        newItem.displayName = itemData.displayName;
        newItem.description = itemData.description;
        newItem.unique = itemData.unique;
        newItem.quantity = itemData.quantity;
        newItem.type = (ItemType)System.Enum.Parse(typeof(ItemType), itemData.type);
        newItem.AmmoType = (Ammo.AmmoType)System.Enum.Parse(typeof(Ammo.AmmoType), itemData.ammoType);
        newItem.PotionType = (Potion.PotionType)System.Enum.Parse(typeof(Potion.PotionType), itemData.potionType);
        newItem.iconPath = itemData.iconPath;
        newItem.icon = Resources.Load<Sprite>(itemData.iconPath);
        newItem.prefabPath = itemData.prefabPath;
        newItem.prefab = Resources.Load<GameObject>(itemData.prefabPath);

        return newItem;
    }

    private void SetGameState(GameState newState, SubGameState newSubGameState = SubGameState.Default)
    {
        CurrentGameState = newState;
        CurrentSubGameState = newSubGameState;

        // Handle state-specific actions
        switch (CurrentGameState)
        {
            case GameState.MainMenu:
                // Add code for main menu behavior
                break;

            case GameState.Gameplay:
                // Add code for common gameplay behavior (for SubGameState.Default)
                // This will be executed for all variations unless overridden in substates.
                switch (CurrentSubGameState)
                {
                    case SubGameState.Default:
                        // Add code for default gameplay behavior
                        break;

                    case SubGameState.Substate1:
                        // Add code for modified gameplay behavior 1
                        break;

                    case SubGameState.Substate2:
                        // Add code for modified gameplay behavior 2
                        break;
                }
                break;

            case GameState.Inventory:
                // Add code for inventory behavior
                break;

            case GameState.Paused:
                // Add code for paused behavior
                break;

            case GameState.GameOver:
                // Add code for game over behavior
                break;
        }
    }
}