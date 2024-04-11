using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.IO;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Timers;
using TMPro;

public class GameManager : MonoBehaviour
{
    #region Singleton Pattern

    public static GameManager Instance { get; private set; }

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

    #endregion

    #region Enums

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
        PickUp,
        Painting
    }

    #endregion

    #region Fields

    // Game state variables
    public GameState CurrentGameState { get; private set; }
    public SubGameState CurrentSubGameState { get; private set; }
    public GameState LastGameState;
    public SubGameState LastSubGameState;

    // Game objects and components
    [Header("Game Objects")]
    public GameObject inventoryCanvas;
    public GameObject inventory;
    public GameObject pickupCanvas;
    public GameObject textCanvas;
    public Player player;
    public PlayerController playerController;
    public PlayerAnimController playerAnimController;
    public Transform enemyPool;
    public Transform interactableObjectPool;    
    public Transform interactStaticObjectPool;
    public Transform doorObjectPool;
    public EventData eventData;
    public Transform autoSavePool;
    public GameObject blackScreen;
    public GameObject fpsArms;

    public GameObject messageCanvas;
    public TextMeshProUGUI messageText;

    [Header("UI Components")]
    [SerializeField] Image progressBar;

    // Custom Update Manager and Input System
    [Header("Custom Update System")]
    public CustomUpdateManager customUpdateManager;
    PlayerInput playerInput;
    public bool debugUpdateManager = false;
    public bool Loading = false;

    // Pause and debug flags
    [Header("Pause Flag")]
    public bool isPaused = false;
    [Tooltip("When Pickup is active, but another canvas is used. Set during gameplay.")]
    public bool canvasActive = false;

    bool referencesUpdated = false;

    #endregion

    #region Debug

    [Header("Debug")]
    [Tooltip("Debug mode for the enemy. If true, the enemy will not attack the player. Set during gameplay.")]
    public bool noAttackMode = false;
    [Tooltip("Debug mode for the enemy. If true, the enemy will not hear. Set during gameplay.")]
    public bool noNoiseMode = false;
    [Tooltip("When set to true, all saved files will be deleted and the bool sets back to false. Set during gameplay.")]
    [SerializeField] bool _deleteSaveFiles = false;
    [Tooltip("When set to true, the game will be saved and the bool sets back to false. Set during gameplay.")]
    public bool saveGame = false;
    [Tooltip("When set to true, the game will be loaded and the bool sets back to false. Set during gameplay.")]
    public bool loadGame = false;

    // Public property for deleteSaveFiles
    public bool DeleteSaveFiles
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

    #endregion

    #region Reference Management

    void UpdateReferences()
    {
        Debug.Log("Updating references...");

        // Find and assign player and related components
        player = FindObjectOfType<Player>();
        if (playerAnimController == null) playerAnimController = FindObjectOfType<PlayerAnimController>();
        if (playerController == null)  playerController = FindObjectOfType<PlayerController>();
        if (pickupCanvas == null) pickupCanvas = GameObject.FindWithTag("PickupCanvas");
        pickupCanvas.SetActive(false);
        if (textCanvas == null) 
        {
            TextCanvasCode textCanvasCode = Resources.FindObjectsOfTypeAll<TextCanvasCode>()[0];
            textCanvas = textCanvasCode.gameObject;
        }
        pickupCanvas.SetActive(false);

        // Find and assign inventory components
        inventory = GameObject.FindWithTag("Inventory");
        inventoryCanvas = inventory.transform.GetChild(0).gameObject;

        // Find and assign object pools
        enemyPool = GameObject.FindWithTag("EnemyPool").transform;
        interactableObjectPool = GameObject.FindWithTag("InteractableObjectPool").transform;
        interactStaticObjectPool = GameObject.FindWithTag("InteractStaticObjectPool").transform;
        doorObjectPool = GameObject.FindWithTag("DoorObjectPool").transform;
        eventData = GameObject.FindWithTag("EventData").GetComponent<EventData>();
        autoSavePool = GameObject.FindWithTag("SaveGamePool").transform;
        fpsArms = player.transform.GetChild(0).GetChild(0).gameObject;

        // Update references in InventoryManager
        if(InventoryManager.Instance != null)
        {
            InventoryManager.Instance.UpdateReferences();
        }
        else Debug.LogWarning("GameManager.cs: InventoryManager is null!");

        if (InputManager.Instance != null)
        {
            InputManager.Instance.UpdateReferences();
        } 
        else Debug.LogWarning("GameManager.cs: InputManager is null!");

        AudioManager.Instance.LoadAudioSources();

        // Update CustomUpdatable references
        UpdateCustomUpdatables();
    }

    void UpdateCustomUpdatables()
    {
        customUpdateManager.ClearCustomUpdatables();

        // Add relevant components to CustomUpdateManager
        customUpdateManager.AddCustomUpdatable(MenuController.Instance);
        PaintingController paintingController = FindObjectOfType<PaintingController>();
        if (paintingController != null)
        {
            customUpdateManager.AddCustomUpdatable(paintingController);
        }
        
        if (player != null) 
        {
            customUpdateManager.AddCustomUpdatable(player.GetComponent<InventoryController>());
            customUpdateManager.AddCustomUpdatable(player.GetComponent<PlayerController>());
            customUpdateManager.AddCustomUpdatable(player.GetComponent<Player>());
        }
        else Debug.LogWarning("GameManager.cs: Player is null!");
        
        if (enemyPool != null)
        {
            AddEnemyCustomUpdatables();
        }
        else Debug.LogWarning("GameManager.cs: EnemyPool is null!");
        
        string debugLog = customUpdateManager.GetCustomUpdatables();
        Debug.Log("GameManager.cs: CustomUpdatables: " + debugLog);

        referencesUpdated = true;
    }

    public void RemoveEnemyCustomUpdatables(bool exit = true)
    {
        foreach (AISensor enemy in enemyPool.GetComponentsInChildren<AISensor>(exit))
        {
            if (exit) customUpdateManager.RemoveCustomUpdatable(enemy);
            enemy.PausePlayerInSight();
        }

        foreach (Mannequin mannequin in enemyPool.GetComponentsInChildren<Mannequin>(exit))
        {
            customUpdateManager.RemoveCustomUpdatable(mannequin);
        }
    }

    public void AddEnemyCustomUpdatables(bool start = true)
    {
        foreach (AISensor enemy in enemyPool.GetComponentsInChildren<AISensor>(start))
        {
            if (start) customUpdateManager.AddCustomUpdatable(enemy);
            enemy.ResumePlayerInSight();
        }

        foreach (Mannequin mannequin in enemyPool.GetComponentsInChildren<Mannequin>(start))
        {
            customUpdateManager.AddCustomUpdatable(mannequin);
        }
    }

    #endregion

    #region Start and Initialization

    private void Start()
    {
        // Check if the current scene is the main menu
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            UpdateCustomUpdatables();
            InputManager.Instance.UpdateReferences();
            Debug.Log("GameManager - Start - MainMenu: " + SceneManager.GetActiveScene().name);
            SetGameState(GameState.MainMenu);
            // AudioManager.Instance.SetAudioClip(AudioManager.Instance.environment, "menu music");
            // AudioManager.Instance.PlayAudio(AudioManager.Instance.environment, 1f, 1f, true);
        }
    }

    IEnumerator StartWithUpdatedReferences()
    {
        // Wait until references are updated before starting the game
        yield return new WaitUntil(() => referencesUpdated);

        // Set initial audio and add starting items
        AudioManager.Instance.SetAudioClip(AudioManager.Instance.environment, "night sound");
        AudioManager.Instance.PlayAudio(AudioManager.Instance.environment, 0.5f, 1f, true);

        referencesUpdated = false;
    }

    #endregion

    #region DEBUG STATEMENT - Delete Files

    private void Update()
    {
        // Check if deleteSaveFiles flag is true and delete save files
        if (DeleteSaveFiles)
        {
            DeleteAllSaveFiles();
            DeleteSaveFiles = false;
        }

        if (saveGame)
        {
            saveGame = false;
            SaveData("Asylum-debugSaveName");
        }

        if (loadGame)
        {
            loadGame = false;
            LoadData("Asylum-debugSaveName");
        }

        if (debugUpdateManager)
        {
            debugUpdateManager = false;
            customUpdateManager.debugBool = true;
        }
    }

    public void DeleteAllSaveFiles()
    {
        // Delete all save files with .shadow extension
        string[] saveFiles = Directory.GetFiles(Application.persistentDataPath, "*.shadow");

        foreach (string saveFile in saveFiles)
        {
            File.Delete(saveFile);
        }

        Debug.Log("##############################");
        Debug.Log("##### All Save Files Deleted");
        Debug.Log("##############################");
    }

    #endregion

    #region Start Management

    public void StartGame()
    {
        // Add code to initialize the gameplay
        StartCoroutine(StartGameWithBlackScreen());
    }

    public IEnumerator StartGameWithBlackScreen()
    {
        Debug.Log("BlackScreen Start");
        // Set the black screen to active

        // Wait for 2 seconds before starting the fade-out
        yield return new WaitForSecondsRealtime(2f);

        // Start the fade-out process
        StartCoroutine(LoadGameWithBlackScreen(true));

        // AudioManager.Instance.LoadAudioSources();

        // Set the game state to Gameplay after the fade-out
        yield return new WaitForSeconds(5f);
        
        Debug.Log("BlackScreen Stop");
    }

    public IEnumerator LoadGameWithBlackScreen(bool newGame = false)
    {
        SetGameState(GameState.Gameplay, SubGameState.EventScene);

        float duration = 3f; 
        float elapsedTime = 0f;
        Color startColor = blackScreen.GetComponent<Image>().color;
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
                if (newGame) playerAnimController.StartAnimation();
                unique = true;
            }

            yield return null;
        }

        // Deactivate the black screen once the fade-out is complete
        blackScreen.SetActive(false);
        blackScreen.GetComponent<Image>().color = new Color(0f, 0f, 0f, 1f);

        float wait = newGame ? 5f : 0f;
        yield return new WaitForSeconds(wait);

        // Set the game state to Gameplay after the fade-out
        SetGameState(GameState.Gameplay, SubGameState.Default);
    }

    #endregion

    #region Game State Management

    public void Inventory()
    {
        InventoryManager.Instance.StatsUpdate();
        AudioManager.Instance.PlaySoundOneShot(AudioManager.Instance.playerSpeaker2, "open inventory", 1f, 1f);
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
        AudioManager.Instance.PlaySoundOneShot(AudioManager.Instance.playerSpeaker2, "click", 1f, 1f);
        MenuController.Instance.customTimer = 0f;
        MenuController.Instance.canCloseMenu = false;
        MenuController.Instance.menuCanvas.SetActive(true);
        SetGameState(GameState.MainMenu);
    }

    public void OpenDeathScreen()
    {
        MenuController.Instance.customTimer = 0f;
        MenuController.Instance.canCloseMenu = false;
        MenuController.Instance.menuCanvas.SetActive(true);
        MenuController.Instance.DeathScreen();
        SetGameState(GameState.MainMenu);
    }

    public void PauseGame()
    {
        if (CurrentGameState == GameState.MainMenu) return;
        SetGameState(GameState.Paused);
        // Add code to pause the game
        RemoveEnemyCustomUpdatables(false);
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
        RemoveEnemyCustomUpdatables(false);
    }

    public void PaintingEvent()
    {
        PaintingEvent paintingEvent = FindObjectOfType<PaintingEvent>();
        if (!paintingEvent.active)
        {
            DisplayMessage("It seems like you can control something with it, but it has no power.", 3f);
            return;
        }

        SetGameState(GameState.Gameplay, SubGameState.Painting);
        RemoveEnemyCustomUpdatables(false);
        paintingEvent.StartPaintingEvent();
    }

    public void ResumeGame()
    {
        SetGameState(GameState.Gameplay, SubGameState.Default);
        // Add code to resume the game
        if (inventoryCanvas.activeSelf) inventoryCanvas.SetActive(false);
        AddEnemyCustomUpdatables(false);
    }

    public void ResumeFromEventScene()
    {
        SetGameState(GameState.Gameplay, SubGameState.Default);
    }

    public void GameOver()
    {
        SetGameState(GameState.GameOver);
        // Add code for game over logic and display game over screen
    }

    private void SetGameState(GameState newState, SubGameState newSubGameState = SubGameState.Default)
    {
        LastGameState = CurrentGameState;
        LastSubGameState = CurrentSubGameState;
        CurrentGameState = newState;
        CurrentSubGameState = newSubGameState;

        Debug.Log("GameManager.cs: SetGameState: " + CurrentGameState + "." + CurrentSubGameState);

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
                        if (agent.gameObject.activeSelf)
                        {
                            agent.isStopped = true;
                        }
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

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                switch (CurrentSubGameState)
                {
                    case SubGameState.Default:
                        foreach (UnityEngine.AI.NavMeshAgent agent in enemyPool.GetComponentsInChildren<UnityEngine.AI.NavMeshAgent>())
                        {
                            if (agent.gameObject.activeSelf)
                            {
                                agent.isStopped = false;
                                Debug.Log("GameManager.cs: Move " + agent.name);
                            }
                        }
                        isPaused = false;
                        playerInput.SwitchCurrentActionMap("Player");
                        Debug.Log("GameManager.cs: Switch to GamePlay.Default");
                        break;

                    case SubGameState.EventScene:
                        foreach (UnityEngine.AI.NavMeshAgent agent in enemyPool.GetComponentsInChildren<UnityEngine.AI.NavMeshAgent>())
                        {
                            if (agent.gameObject.activeSelf)
                            {
                                agent.isStopped = true;
                                Debug.Log("GameManager.cs: Move " + agent.name);
                            }
                        }
                        isPaused = true;
                        playerInput.SwitchCurrentActionMap("Event");
                        Debug.Log("GameManager.cs: Switch to GamePlay.EventScene");
                        break;

                    case SubGameState.PickUp:
                        foreach (UnityEngine.AI.NavMeshAgent agent in enemyPool.GetComponentsInChildren<UnityEngine.AI.NavMeshAgent>())
                        {
                            if (agent.gameObject.activeSelf)
                            {
                                agent.isStopped = true;
                                Debug.Log("GameManager.cs: Move " + agent.name);
                            }
                        }
                        isPaused = true;
                        playerInput.SwitchCurrentActionMap("PickUp");
                        Debug.Log("GameManager.cs: Switch to GamePlay.PickUp");
                        break;
                    case SubGameState.Painting:
                        foreach (UnityEngine.AI.NavMeshAgent agent in enemyPool.GetComponentsInChildren<UnityEngine.AI.NavMeshAgent>())
                        {
                            if (agent.gameObject.activeSelf)
                            {
                                agent.isStopped = true;
                                Debug.Log("GameManager.cs: Move " + agent.name);
                            }
                        }
                        isPaused = true;
                        playerInput.SwitchCurrentActionMap("Painting");
                        Debug.Log("GameManager.cs: Switch to GamePlay.Painting");
                        break;
                }
                break;

            case GameState.Inventory:
                // Add code for inventory behavior
                foreach (UnityEngine.AI.NavMeshAgent agent in enemyPool.GetComponentsInChildren<UnityEngine.AI.NavMeshAgent>())
                {
                    if (agent.gameObject.activeSelf)
                    {
                        agent.isStopped = true;
                    }
                }
                isPaused = true;
                playerInput.SwitchCurrentActionMap("Inventory");
                inventoryCanvas.SetActive(true);
                InventoryManager.Instance.ListItems();
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                break;

            case GameState.Paused:
                // Add code for paused behavior
                foreach (UnityEngine.AI.NavMeshAgent agent in enemyPool.GetComponentsInChildren<UnityEngine.AI.NavMeshAgent>())
                {
                    if (agent.gameObject.activeSelf)
                    {
                        agent.isStopped = true;
                    }
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
    #endregion

    #region Message Management

    public void DisplayMessage(string message, float duration = 3f)
    {
        messageText.text = message;
        if (!messageCanvas.activeSelf) messageCanvas.SetActive(true);
        StartCoroutine(HideMessage(duration, message));
    }

    IEnumerator HideMessage(float duration, string message)
    {
        yield return new WaitForSeconds(duration);
        // string compare: if message is same as messageText
        if (string.Compare(message, messageText.text) == 0)
        {
            messageCanvas.SetActive(false);
        }
    }

    #endregion

    #region Save and Load

    public void LoadNewGame()
    {
        // Add code to initialize a new game
        Loading = true;
        LoadNewScene("Asylum", true); // Load the Sandbox scene and set "new Game" flag to new    
        Debug.Log("GameManager.cs: New Game Loaded!");
    }

    public void SaveData(string filename = "Asylum-autosave")
    {
        SaveSystem.SaveGameFile(filename, player, InventoryManager.Instance.weapons.transform, enemyPool, interactableObjectPool, interactStaticObjectPool, doorObjectPool, autoSavePool);
        Debug.Log("GameManager.cs: Player data saved!");
    }

    public void LoadData(string filename = "Asylum-autosave")
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
            // Load Enemies
            ////////////////////////////
            Enemy[] enemiesPool = enemyPool.GetComponentsInChildren<Enemy>(true);
            Mannequin[] mannequinsPool = enemyPool.GetComponentsInChildren<Mannequin>(true);

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
                        if (enemyData.isActive) enemy.GetComponent<EnemyBT>().ResetTree();
                        else enemy.gameObject.SetActive(false);
                    }
                }

                foreach (Mannequin mannequin in mannequinsPool)
                {
                    if (mannequin.GetComponent<UniqueIDComponent>().UniqueID == enemyData.uniqueID)
                    {
                        mannequin.isDead = enemyData.isDead;
                        mannequin.transform.position = new Vector3(enemyData.position[0], enemyData.position[1], enemyData.position[2]);
                        mannequin.transform.rotation = Quaternion.Euler(enemyData.rotation[0], enemyData.rotation[1], enemyData.rotation[2]);
                        if (mannequin.isDead)
                        {
                            mannequin.gameObject.SetActive(false);
                        }
                        else if (enemyData.isActive)
                        {
                            mannequin.gameObject.SetActive(true);
                        }
                        else mannequin.gameObject.SetActive(false);
                    }
                }
            }

            ////////////////////////////
            // Load Interactable Objects
            ////////////////////////////
            InteractableObject[] intObjectsPool = interactableObjectPool.GetComponentsInChildren<InteractableObject>();
            Duplicate[] duplicatesPool = interactableObjectPool.GetComponentsInChildren<Duplicate>();

            foreach (InteractableObjectData intObjectsData in data.interactableObjects)
            {
                foreach (InteractableObject pickupObject in intObjectsPool)
                {
                    if (pickupObject.GetComponent<UniqueIDComponent>().UniqueID == intObjectsData.uniqueID)
                    {
                        pickupObject.active = intObjectsData.active;
                        if (!pickupObject.active)
                        {
                            // Move the object to a position far away
                            pickupObject.transform.position = new Vector3(0, -1000f, 0);
                        }
                    }
                }

                int count = 0;

                foreach (InteractableObject obj in intObjectsPool)
                {                    
                    if (obj is HorrorDollCode horrorDoll)
                    {
                        if (!horrorDoll.active)
                        {
                            count++;
                        }
                    }
                }

                InventoryManager.Instance.horrorDollCount.text = count.ToString();

                foreach (Duplicate duplicate in duplicatesPool)
                {
                    if (duplicate.duplicateID == intObjectsData.uniqueID)
                    {
                        duplicate.duplicateObject.GetComponent<InteractableObject>().active = intObjectsData.active;
                        if (!duplicate.duplicateObject.GetComponent<InteractableObject>().active)
                        {
                            // Move the object to a position far away
                            duplicate.duplicateObject.transform.position = new Vector3(0, -1000f, 0);
                        }
                    }
                }
            }

            ////////////////////////////
            // Load Interactable Static Objects
            ////////////////////////////
            Drawer[] intObjPool;

            foreach (Transform staticObject in interactStaticObjectPool.GetComponentsInChildren<Transform>())
            {
                intObjPool = staticObject.GetComponentsInChildren<Drawer>();

                foreach (InteractStaticObjectData intObjData in data.interactStaticObjects)
                {
                    foreach (Drawer drawer in intObjPool)
                    {
                        if (drawer.GetComponent<UniqueIDComponent>().UniqueID == intObjData.uniqueID)
                        {
                            drawer.open = intObjData.open;
                            if (drawer.isDrawer && intObjData.open) drawer.transform.position = new Vector3(intObjData.position[0], intObjData.position[1], intObjData.position[2]);
                            else if (intObjData.open) drawer.transform.rotation = Quaternion.Euler(intObjData.rotation[0], intObjData.rotation[1], intObjData.rotation[2]);
                        }
                    }
                }
            }

            ////////////////////////////
            // Load Door Objects
            ////////////////////////////

            foreach (DoorObjectData doorData in data.doors)
            {
                foreach (Duplicate door in doorObjectPool.GetComponentsInChildren<Duplicate>())
                {
                    if (door.duplicateID == doorData.uniqueID)
                    {
                        Door doorScript = door.duplicateObject.GetComponent<Door>();
                        doorScript.locked = doorData.locked;
                        if (doorData.open) doorScript.OpenDoor(false);
                    }
                }
            }

            ////////////////////////////
            // Load Event Data
            ////////////////////////////

            foreach (SavedEventData savedEventData in data.events)
            {
                foreach (EventDataEntry eventDataEntry in eventData.eventDataEntries)
                {
                    if (eventDataEntry.EventName == savedEventData.EventName)
                    {
                        if (savedEventData.Active)
                        {
                            eventDataEntry.Active = true;
                            eventData.TriggerEvent(eventDataEntry.EventName);
                        }
                    }
                }
            }

            ////////////////////////////
            // Load Various Data
            ////////////////////////////

            AudioManager.Instance.SetAudioClip(AudioManager.Instance.environment, data.variousData.environmentMusicClip);
            AudioManager.Instance.PlayAudio(AudioManager.Instance.environment, 0.1f, 1f, true);
            player.lightAvail = data.variousData.flashlightEnabled;
            if (player.lightAvail) player.LightSwitch();

            ////////////////////////////
            // AutoSave Data
            ////////////////////////////

            foreach (AutoSaveData autoSaveData in data.autoSaveData)
            {
                foreach (AutoSave autoSave in autoSavePool.GetComponentsInChildren<AutoSave>())
                {
                    if (autoSave.GetComponent<UniqueIDComponent>().UniqueID == autoSaveData.uniqueID)
                    {
                        if (!autoSaveData.active)
                        {
                            autoSave.active = false;
                            autoSave.GetComponent<Collider>().enabled = false;
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
        Loading = true;

        string sceneNameToLoad = filename.Split('-')[0].Trim();

        StartCoroutine(LoadSceneAsync(sceneNameToLoad, filename, newGame));
    }

    IEnumerator LoadSceneAsync(string sceneName, string filename, bool newGame = false)
    {
        blackScreen.SetActive(true);

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

        yield return new WaitForSeconds(0.5f);

        UpdateReferences();

        yield return new WaitForSeconds(1f);

        if (!newGame)
        {
            LoadData(filename);
        }
        else
        {
            Debug.Log("GameManager - Start - Gameplay: " + SceneManager.GetActiveScene().name);
            StartGame();
            StartCoroutine(StartWithUpdatedReferences());
        }

        yield return new WaitForSeconds(1f);
        
        if (!newGame)
        {
            Debug.Log("GameManager - Start - LoadGame: " + SceneManager.GetActiveScene().name);
            StartCoroutine(LoadGameWithBlackScreen());
        }

        // Scene is fully loaded
        Debug.Log("Scene " + sceneName + " is fully loaded!");
        Loading = false;
    }
    #endregion
}