using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.IO;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Timers;

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
        EventScene,
        PickUp
    }

    public GameState CurrentGameState { get; private set; }
    public SubGameState CurrentSubGameState { get; private set; }

    public GameState LastGameState;

    public GameObject inventoryCanvas;
    public GameObject inventory;
    public GameObject pickupCanvas;
    public Player player;
    public PlayerController playerController;
    public PlayerAnimController playerAnimController;
    public Transform objectPool;
    public Transform enemyPool;
    public Transform interactObjectPool;
    public GameObject blackScreen;
    [SerializeField] Image progressBar;
    [SerializeField] Item[] startGameItems;

    CustomUpdateManager customUpdateManager;
    PlayerInput playerInput;

    public bool isPaused;

    bool referencesUpdated = false;



/////////////////////////////////////
// DEBUG STATEMENT - Delete Files  //
/////////////////////////////////////
    [Header("Debug")]
    [Tooltip("Debug mode for the enemy. If true, the enemy will not attack the player. Set during gamplay.")]
    public bool noAttackMode = false;
    [Tooltip("Debug mode for the enemy. If true, the enemy will not hear. Set during gamplay.")]
    public bool noNoiseMode = false;
    [Tooltip("When set to true, all saved files will be deleted and the bool sets back to false. Set during gameplay.")]
    [SerializeField] bool _deleteSaveFiles = false;

    // Public property for isPoisoned
    public bool deleteSaveFiles
    {
        get => _deleteSaveFiles;
        set
        {
            if (_deleteSaveFiles != value)
            {
                _deleteSaveFiles = value;
            }
        }
    }
/////////////////////////////////////
/////////////////////////////////////
/////////////////////////////////////



/////////////////////////////////////
// Singleton Pattern               //
/////////////////////////////////////
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

        // Get the black screen reference
        blackScreen = transform.GetChild(0).gameObject;

        // Get the CustomUpdateManager instance
        customUpdateManager = GetComponent<CustomUpdateManager>();
    }
/////////////////////////////////////
/////////////////////////////////////
/////////////////////////////////////



/////////////////////////////////////
// Reference Management            //
/////////////////////////////////////
    void UpdateReferences()
    {
        Debug.Log("Reference will be updated!");

        player = FindObjectOfType<Player>();
        if (playerAnimController == null) playerAnimController = FindObjectOfType<PlayerAnimController>();
        if (playerController == null)  playerController = FindObjectOfType<PlayerController>();
        if (pickupCanvas == null) pickupCanvas = GameObject.FindWithTag("PickupCanvas");
        pickupCanvas.SetActive(false);

        inventory = GameObject.FindWithTag("Inventory");
        inventoryCanvas = inventory.transform.GetChild(0).gameObject;

        objectPool = GameObject.FindWithTag("ObjectPool").transform;
        enemyPool = GameObject.FindWithTag("EnemyPool").transform;
        interactObjectPool = GameObject.FindWithTag("InteractObjectPool").transform;

        if(InventoryManager.Instance != null)
        {
            InventoryManager.Instance.UpdateReferences();
        }

        // Update CustomUpdatable references
        UpdateCustomUpdatables();
    }

    void UpdateCustomUpdatables()
    {
        customUpdateManager.AddCustomUpdatable(MenuController.Instance);
        
        if (player != null) 
        {
            customUpdateManager.AddCustomUpdatable(player.GetComponent<InventoryController>());
            customUpdateManager.AddCustomUpdatable(player.GetComponent<PlayerController>());
            customUpdateManager.AddCustomUpdatable(player.GetComponent<Player>());
        }
        else Debug.LogWarning("GameManager.cs: Player is null!");
        
        if (enemyPool != null)
        {
            foreach (AISensor enemy in enemyPool.GetComponentsInChildren<AISensor>())
            {
                customUpdateManager.AddCustomUpdatable(enemy);
            } 
        }
        else Debug.LogWarning("GameManager.cs: EnemyPool is null!");

        FlickeringLight[] flickeringLights = FindObjectsOfType<FlickeringLight>();
        if (flickeringLights != null)
        {
            foreach (FlickeringLight flickeringLight in flickeringLights)
            {
                customUpdateManager.AddCustomUpdatable(flickeringLight);
            }
        }
        else Debug.LogWarning("GameManager.cs: FlickeringLights is null!");
        
        string debugLog = customUpdateManager.GetCustomUpdatables();
        Debug.Log("GameManager.cs: CustomUpdatables: " + debugLog);

        referencesUpdated = true;
    }
/////////////////////////////////////
/////////////////////////////////////
/////////////////////////////////////



/////////////////////////////////////
// Start Game                      //
/////////////////////////////////////
    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            UpdateCustomUpdatables();
            Debug.Log("GameManager - Start - MainMenu: " + SceneManager.GetActiveScene().name);
            SetGameState(GameState.MainMenu);
            AudioManager.Instance.SetAudioClip(AudioManager.Instance.environment, "menu music");
            AudioManager.Instance.PlayAudio(AudioManager.Instance.environment, 1f, 1f, true);
        }
        else
        {
            Debug.Log("GameManager - Start - Gameplay: " + SceneManager.GetActiveScene().name);
            UpdateReferences();
            StartCoroutine(StartWithUpdatedReferences());
        }
    }

    IEnumerator StartWithUpdatedReferences()
    {
        yield return new WaitUntil(() => referencesUpdated);
        AudioManager.Instance.SetAudioClip(AudioManager.Instance.environment, "night sound");
        AudioManager.Instance.PlayAudio(AudioManager.Instance.environment, 0.5f, 1f, true);
        foreach (Item item in startGameItems)
        {
            InventoryManager.Instance.AddItem(item);
        }
        referencesUpdated = false;
    }
/////////////////////////////////////
/////////////////////////////////////
/////////////////////////////////////



/////////////////////////////////////
// DEBUG STATEMENT - Delete Files //
/////////////////////////////////////
    private void Update()
    {
        if (deleteSaveFiles)
        {
            DeleteAllSaveFiles();
            deleteSaveFiles = false;
        }
    }

    public void DeleteAllSaveFiles()
    {
        string[] saveFiles = Directory.GetFiles(Application.persistentDataPath, "*.shadow");

        foreach (string saveFile in saveFiles)
        {
            File.Delete(saveFile);
        }

        Debug.Log("##############################");
        Debug.Log("##### All Save Files Deleted");
        Debug.Log("##############################");
    }
/////////////////////////////////////
/////////////////////////////////////
/////////////////////////////////////



/////////////////////////////////////
// Game State Management           //
/////////////////////////////////////
    public void StartGame()
    {
        // Add code to initialize the gameplay
        StartCoroutine(StartGameWithBlackScreen());
    }

    public IEnumerator StartGameWithBlackScreen()
    {
        Debug.Log("BlackScreen Start");
        // Set the black screen to active
        CurrentSubGameState = SubGameState.EventScene;
        if (!blackScreen.activeSelf) blackScreen.SetActive(true);

        // Wait for 2 seconds before starting the fade-out
        yield return new WaitForSecondsRealtime(2f);

        // Start the fade-out process
        float duration = 2f; // You can adjust the duration of the fade-out here
        float elapsedTime = 0f;
        Color startColor = Color.black;
        Color targetColor = new Color(0f, 0f, 0f, 0f); // Fully transparent
        bool unique = false;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            Color newColor = Color.Lerp(startColor, targetColor, t);
            blackScreen.GetComponent<Image>().color = newColor;

            if (elapsedTime >= 1f && !unique) 
            {
                playerAnimController.StartAnimation();
                unique = true;
            }

            yield return null;
        }

        // Deactivate the black screen once the fade-out is complete
        blackScreen.SetActive(false);
        blackScreen.GetComponent<Image>().color = Color.black;
        AudioManager.Instance.LoadAudioSources();

        // Set the game state to Gameplay after the fade-out
        SetGameState(GameState.Gameplay, SubGameState.Default);
        Debug.Log("BlackScreen Stop");
    }

    public void Inventory()
    {
        SetGameState(GameState.Inventory);
        // Add code to display the inventory
    }

    public void OpenSaveScreen()
    {
        MenuController.Instance.menuCanvas.SetActive(true);
        MenuController.Instance.SaveGame();
        SetGameState(GameState.MainMenu);
    }

    public void OpenMenu()
    {
        MenuController.Instance.customTimer = 0f;
        MenuController.Instance.canCloseMenu = false;
        MenuController.Instance.menuCanvas.SetActive(true);
        SetGameState(GameState.MainMenu);
    }

    public void PauseGame()
    {
        if (CurrentGameState == GameState.MainMenu) return;
        SetGameState(GameState.Paused);
        // Add code to pause the game
        foreach (AISensor enemy in enemyPool.GetComponentsInChildren<AISensor>())
        {
            customUpdateManager.RemoveCustomUpdatable(enemy);
        } 
    }

    public void GameplayEvent()
    {
        SetGameState(GameState.Gameplay, SubGameState.EventScene);
        playerController.GamePlayEvent();
        // Add code for gameplay event
    }

    public void PickUp()
    {
        SetGameState(GameState.Gameplay, SubGameState.PickUp);
        // Add code for picking up an item
        foreach (AISensor enemy in enemyPool.GetComponentsInChildren<AISensor>())
        {
            customUpdateManager.RemoveCustomUpdatable(enemy);
        }
    }

    public void ResumeGame()
    {
        SetGameState(GameState.Gameplay, SubGameState.Default);
        // Add code to resume the game
        if (inventoryCanvas.activeSelf) inventoryCanvas.SetActive(false);
        foreach (AISensor enemy in enemyPool.GetComponentsInChildren<AISensor>())
        {
            customUpdateManager.AddCustomUpdatable(enemy);
        } 
    }

    public void GameOver()
    {
        SetGameState(GameState.GameOver);
        // Add code for game over logic and display game over screen
    }

    private void SetGameState(GameState newState, SubGameState newSubGameState = SubGameState.Default)
    {
        LastGameState = CurrentGameState;
        CurrentGameState = newState;
        CurrentSubGameState = newSubGameState;

        // Handle state-specific actions
        switch (CurrentGameState)
        {
            case GameState.MainMenu:
                // Add code for main menu behavior
                if (playerInput == null)
                {
                    playerInput = FindObjectOfType<PlayerInput>();
                }
                if (enemyPool != null)
                {
                    foreach (UnityEngine.AI.NavMeshAgent agent in enemyPool.GetComponentsInChildren<UnityEngine.AI.NavMeshAgent>())
                    {
                        agent.isStopped = true;
                    }
                }
                isPaused = true;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                playerInput.SwitchCurrentActionMap("Menu");
                break;

            case GameState.Gameplay:
                // Add code for common gameplay behavior (for SubGameState.Default)
                // This will be executed for all variations unless overridden in substates.
                if (playerInput == null)
                {
                    playerInput = FindObjectOfType<PlayerInput>();
                }
                foreach (UnityEngine.AI.NavMeshAgent agent in enemyPool.GetComponentsInChildren<UnityEngine.AI.NavMeshAgent>())
                {
                    agent.isStopped = false;
                    Debug.Log("GameManager.cs: Move " + agent.name);
                }
                isPaused = false;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                switch (CurrentSubGameState)
                {
                    case SubGameState.Default:
                        playerInput.SwitchCurrentActionMap("Player");
                        Debug.Log("GameManager.cs: Switch to GamePlay.Default");
                        break;

                    case SubGameState.EventScene:
                        playerInput.SwitchCurrentActionMap("Event");
                        Debug.Log("GameManager.cs: Switch to GamePlay.EventScene");
                        break;

                    case SubGameState.PickUp:
                        isPaused = true;
                        playerInput.SwitchCurrentActionMap("PickUp");
                        Debug.Log("GameManager.cs: Switch to GamePlay.PickUp");
                        break;
                }
                break;

            case GameState.Inventory:
                // Add code for inventory behavior
                foreach (UnityEngine.AI.NavMeshAgent agent in enemyPool.GetComponentsInChildren<UnityEngine.AI.NavMeshAgent>())
                {
                    agent.isStopped = true;
                }
                isPaused = true;
                playerInput.SwitchCurrentActionMap("Inventory");
                inventoryCanvas.SetActive(true);
                InventoryManager.Instance.ListItems();
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                break;

            case GameState.Paused:
                // Add code for paused behavior
                foreach (UnityEngine.AI.NavMeshAgent agent in enemyPool.GetComponentsInChildren<UnityEngine.AI.NavMeshAgent>())
                {
                    agent.isStopped = true;
                }
                isPaused = true;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                break;

            case GameState.GameOver:
                // Add code for game over behavior
                isPaused = true;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                break;
        }
    }
/////////////////////////////////////
/////////////////////////////////////
/////////////////////////////////////



/////////////////////////////////////
// Save/Load Game                  //
/////////////////////////////////////
    public void LoadNewGame()
    {
        // Add code to initialize a new game
        LoadNewScene("Asylum", true); // Load the Sandbox scene and set "new Game" flag to new    
        Debug.Log("GameManager.cs: New Game Loaded!");
    }

    public void SaveData(string filename = "autosave")
    {
        SaveSystem.SaveGameFile(filename, player, InventoryManager.Instance.weapons.transform, objectPool, enemyPool, interactObjectPool);
        Debug.Log("GameManager.cs: Player data saved!");
    }

    public void LoadData(string filename = "autosave")
    {
        SaveData data = SaveSystem.LoadGameFile(filename);
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
                    if (pickupObject.GetComponent<UniqueIDComponent>().UniqueID == pickupObjectData.uniqueID)
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
                    if (enemy.GetComponent<UniqueIDComponent>().UniqueID == enemyData.uniqueID)
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
            InteractableObject[] intObjPool = interactObjectPool.GetComponentsInChildren<InteractableObject>(true);

            foreach (InteractableObjectData intObjData in data.interactableObjects)
            {
                foreach (InteractableObject intObj in intObjPool)
                {
                    if (intObj.GetComponent<UniqueIDComponent>().UniqueID == intObjData.uniqueID)
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

    public void LoadNewScene(string filename, bool newGame = false) // Hardcoded for now
    {
        string sceneNameToLoad = filename.Split('-')[0].Trim();

        StartCoroutine(LoadSceneAsync(sceneNameToLoad, filename, newGame));
    }

    IEnumerator LoadSceneAsync(string sceneName, string filename, bool newGame = false)
    {
        // Start loading the LoadingScreen scene asynchronously
        AsyncOperation asyncLoadLoadingScreen = SceneManager.LoadSceneAsync("LoadingScreen");

        // Wait until the LoadingScreen scene is fully loaded
        while (!asyncLoadLoadingScreen.isDone)
        {
            yield return null;
        }

        progressBar = GameObject.Find("ProgressBar").GetComponent<Image>();

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        while (!asyncLoad.isDone)
        {
            progressBar.fillAmount = asyncLoad.progress;

            if (asyncLoad.progress >= 0.95f)
            {
                progressBar.fillAmount = 1f;
                asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }

        UpdateReferences();

        StartGame();

        if (!newGame)
        {
            LoadData(filename);
        }
        else
        {
            AudioManager.Instance.SetAudioClip(AudioManager.Instance.environment, "night sound");
            AudioManager.Instance.PlayAudio(AudioManager.Instance.environment, 0.5f, 1f, true);
            foreach (Item item in startGameItems)
            {
                InventoryManager.Instance.AddItem(item);
            }
        }

        InputManager.Instance.Rebind("Player");
        InputManager.Instance.Rebind("Inventory");
        InputManager.Instance.Rebind("Pickup");

        // Scene is fully loaded
        Debug.Log("Scene " + sceneName + " is fully loaded!");
    }
/////////////////////////////////////
/////////////////////////////////////
/////////////////////////////////////



}