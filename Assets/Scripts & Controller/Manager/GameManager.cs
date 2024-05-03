using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.IO;
using UnityEngine.UI;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using System.Timers;
using TMPro;

public class GameManager : MonoBehaviour
{
    #region Singleton Pattern

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        Blackscreen = transform.GetChild(0).gameObject;
        CustomUpdateManager = GetComponent<CustomUpdateManager>();
    }

    #endregion




    #region Enums

    public enum GameState
    {
        MENU,
        GAMEPLAY,
        INVENTORY,
        PAUSE,
        GAMEOVER
    }

    public enum SubGameState
    {
        DEFAULT,
        EVENT,
        PICKUP,
        PAINT
    }

    #endregion




    #region Fields


    public GameState CurrentGameState { get; private set; }
    public SubGameState CurrentSubGameState { get; private set; }
    [Header("Game State")]
    public GameState LastGameState;
    public SubGameState LastSubGameState;
    [Space(10)]

    // Player and related components
    [HideInInspector] public Player Player;
    [HideInInspector] public PlayerController PlayerController;
    [HideInInspector] public PlayerAnimManager PlayerAnimManager;

    // Canvas and UI components
    [HideInInspector] public GameObject Inventory;
    [HideInInspector] public GameObject InventoryCanvas;
    [HideInInspector] public GameObject PickupCanvas;
    [HideInInspector] public TextMeshProUGUI PickupName;
    [HideInInspector] public TextMeshProUGUI PickupDescription;
    [HideInInspector] public GameObject PickupPositions;
    [HideInInspector] public GameObject TextCanvas;
    [HideInInspector] public GameObject Blackscreen;

    // Object Pools
    [HideInInspector] public Transform EnemyPool;
    [HideInInspector] public Transform InteractableObjectPool;    
    private Transform _interactStaticObjectPool;
    private Transform _doorObjectPool;
    private Transform _textObjectPool;
    private Transform _summonObjectPool;
    private Transform _autoSavePool;

    // Other components
    [HideInInspector] public CustomUpdateManager CustomUpdateManager;
    [HideInInspector] public EventData EventData;
    private PlayerInput _playerInput;

    [Header("Flags")]
    public bool IsGamePaused = false;
    [Tooltip("When Pickup is active, but another canvas is used. Set during gameplay.")]
    public bool IsCanvasActive = false;
    [Tooltip("When the game is loading.")]
    public bool IsLoading = false;
    private bool _areReferencesUpdated = false;

    // Message Canvas
    private GameObject _messageCanvas;
    private TextMeshProUGUI _messageText;

    // Progress Bar for Loading
    private Image _progressBar;

    #endregion




    #region Debug

    [Header("Debug")]
    [Tooltip("Debug mode for the enemy. If true, the enemy will not attack the player. Set during gameplay.")]
    public bool NoAttackMode = false;
    [Tooltip("Debug mode for the enemy. If true, the enemy will not hear. Set during gameplay.")]
    public bool NoNoiseMode = false;
    [Tooltip("Print the CustomUpdatables list.")]
    [SerializeField] private bool _printCustomUpdateables = false;
    [Tooltip("When set to true, all saved files will be deleted and the bool sets back to false. Set during gameplay.")]
    [SerializeField] private bool _deleteSaveFiles = false;
    [SerializeField] private bool _debugMode = false;

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
        if (_debugMode) Debug.Log("Updating references...");

        // Find and assign player and related components
        Player = GameObject.FindWithTag("Player").GetComponent<Player>();
        if (PlayerAnimManager == null) PlayerAnimManager = GameObject.FindWithTag("Player").GetComponent<PlayerAnimManager>();
        if (PlayerController == null)  PlayerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        if (PickupCanvas == null) PickupCanvas = GameObject.FindWithTag("PickupCanvas");
        if (PickupName == null) PickupName = PickupCanvas.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        if (PickupDescription == null) PickupDescription = PickupCanvas.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        if (PickupPositions == null) PickupPositions = PickupCanvas.transform.GetChild(3).gameObject;
        PickupCanvas.SetActive(false);
        if (TextCanvas == null) TextCanvas = GameObject.FindWithTag("TextCanvas");
        TextCanvas.SetActive(false);

        // Find and assign inventory components
        Inventory = GameObject.FindWithTag("Inventory");
        InventoryCanvas = Inventory.transform.GetChild(0).gameObject;

        _messageCanvas = transform.GetChild(1).gameObject;
        _messageText = _messageCanvas.GetComponentInChildren<TextMeshProUGUI>();

        // Find and assign object pools
        EnemyPool = GameObject.FindWithTag("EnemyPool").transform;
        InteractableObjectPool = GameObject.FindWithTag("InteractableObjectPool").transform;
        _interactStaticObjectPool = GameObject.FindWithTag("InteractStaticObjectPool").transform;
        _doorObjectPool = GameObject.FindWithTag("DoorObjectPool").transform;
        _textObjectPool = GameObject.FindWithTag("TextObjectPool").transform;
        _summonObjectPool = GameObject.FindWithTag("SummonObjectPool").transform;
        EventData = GameObject.FindWithTag("EventData").GetComponent<EventData>();
        _autoSavePool = GameObject.FindWithTag("SaveGamePool").transform;

        // Update References for Managers
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

        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.InitializeEnemyPool();
        }
        else Debug.LogWarning("GameManager.cs: EnemyManager is null!");

        AudioManager.Instance.LoadAudioSources();

        UpdateCustomUpdatables();
    }

    void UpdateCustomUpdatables()
    {
        CustomUpdateManager.ClearCustomUpdatables();
        CustomUpdateManager.AddCustomUpdatable(MenuController.Instance);
        
        if (Player != null) 
        {
            CustomUpdateManager.AddCustomUpdatable(Player.GetComponent<InventoryController>());
            CustomUpdateManager.AddCustomUpdatable(PlayerController);
            CustomUpdateManager.AddCustomUpdatable(Player);
        }
        else Debug.LogWarning("GameManager.cs: Player is null!");
        
        if (EnemyPool != null)
        {
            AddEnemyCustomUpdatables();
        }
        else Debug.LogWarning("GameManager.cs: EnemyPool is null!");
        
        string allCustomUpdateables = CustomUpdateManager.GetCustomUpdatables();
        if (_debugMode) Debug.Log("GameManager.cs: CustomUpdatables: " + allCustomUpdateables);

        _areReferencesUpdated = true;
    }

    #endregion




    #region Start and Initialization

    private void Start()
    {
        UpdateCustomUpdatables();
        InputManager.Instance.UpdateReferences();
        if (_debugMode) Debug.Log("GameManager - Start - MainMenu: " + SceneManager.GetActiveScene().name);
        SetGameState(GameState.MENU);
    }

    IEnumerator StartWithUpdatedReferences()
    {
        yield return new WaitUntil(() => _areReferencesUpdated);

        AudioManager.Instance.SetAudioClip(AudioManager.Instance.Environment, "night sound");
        AudioManager.Instance.PlayAudio(AudioManager.Instance.Environment, 0.5f, 1f, true);

        _areReferencesUpdated = false;
    }

    #endregion




    #region DEBUG STATEMENT - Delete Files

    private void Update()
    {
        if (DeleteSaveFiles)
        {
            DeleteAllSaveFiles();
            DeleteSaveFiles = false;
        }

        if (_printCustomUpdateables)
        {
            _printCustomUpdateables = false;
            CustomUpdateManager.PrintUpdatables = true;
        }
    }

    public void DeleteAllSaveFiles()
    {
        string[] saveFiles = Directory.GetFiles(Application.persistentDataPath, "*.shadow");

        foreach (string saveFile in saveFiles)
        {
            File.Delete(saveFile);
        }

        if (_debugMode) Debug.Log("##############################");
        if (_debugMode) Debug.Log("##### All Save Files Deleted");
        if (_debugMode) Debug.Log("##############################");
    }

    #endregion




    #region Start Management

    public void StartGame()
    {
        StartCoroutine(StartGameWithBlackScreen());
    }

    public IEnumerator StartGameWithBlackScreen()
    {
        if (_debugMode) Debug.Log("Blackscreen Start");

        yield return new WaitForSecondsRealtime(2f);

        StartCoroutine(LoadGameWithBlackScreen(true));

        yield return new WaitForSeconds(5f);
        
        if (_debugMode) Debug.Log("Blackscreen Stop");
    }

    public IEnumerator LoadGameWithBlackScreen(bool newGame = false)
    {
        SetGameState(GameState.GAMEPLAY, SubGameState.EVENT);

        float duration = 3f; 
        float elapsedTime = 0f;
        Color startColor = Blackscreen.GetComponent<Image>().color;
        Color targetColor = new Color(0f, 0f, 0f, 0f);
        bool unique = false;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            Color newColor = Color.Lerp(startColor, targetColor, t);
            Blackscreen.GetComponent<Image>().color = newColor;

            if (elapsedTime >= 1f && !unique) 
            {
                if (newGame) PlayerAnimManager.StartAnimation();
                unique = true;
            }

            yield return null;
        }

        Blackscreen.SetActive(false);
        Blackscreen.GetComponent<Image>().color = new Color(0f, 0f, 0f, 1f);

        float wait = newGame ? 5f : 0f;
        yield return new WaitForSeconds(wait);

        SetGameState(GameState.GAMEPLAY, SubGameState.DEFAULT);
    }

    #endregion




    #region Game State Management

    public void OpenInventory()
    {
        InventoryManager.Instance.StatsUpdate();
        AudioManager.Instance.PlayClipOneShot(AudioManager.Instance.PlayerSpeaker2, "open inventory", 0.8f, 1f);
        SetGameState(GameState.INVENTORY);
    }

    public void OpenSaveScreen()
    {
        MenuController.Instance.MenuCanvas.SetActive(true);
        MenuController.Instance.SaveGame();
        SetGameState(GameState.MENU);
    }

    public void OpenMenu()
    {
        AudioManager.Instance.PlayClipOneShot(AudioManager.Instance.PlayerSpeaker2, "clicker", 0.5f, 1f);
        MenuController.Instance.CloseMenuTimer = 0f;
        MenuController.Instance.CanCloseMenu = false;
        MenuController.Instance.MenuCanvas.SetActive(true);
        SetGameState(GameState.MENU);
    }

    public void BackToMainMenu()
    {
        MenuController.Instance.MainMenu();
        SceneManager.LoadScene("MainMenu");
        MenuController.Instance.MenuCanvas.SetActive(true);
        SetGameState(GameState.MENU);
    }

    public void OpenDeathScreen()
    {
        MenuController.Instance.CloseMenuTimer = 0f;
        MenuController.Instance.CanCloseMenu = false;
        MenuController.Instance.MenuCanvas.SetActive(true);
        MenuController.Instance.DeathScreen();
        SetGameState(GameState.MENU);
    }

    public void PauseGame()
    {
        if (CurrentGameState == GameState.MENU) return;
        SetGameState(GameState.PAUSE);
        RemoveEnemyCustomUpdatables(false);
    }

    public void GameplayEvent()
    {
        SetGameState(GameState.GAMEPLAY, SubGameState.EVENT);
        PlayerController.GamePlayEvent();
    }

    public void PickUp()
    {
        SetGameState(GameState.GAMEPLAY, SubGameState.PICKUP);
        RemoveEnemyCustomUpdatables(false);
    }

    public void PaintingEvent()
    {
        PaintingEvent paintingEvent = FindObjectOfType<PaintingEvent>();
        if (!paintingEvent.IsActive)
        {
            DisplayMessage("It seems like you can control something with it, but it has no power.", 3f);
            return;
        }

        SetGameState(GameState.GAMEPLAY, SubGameState.PAINT);
        RemoveEnemyCustomUpdatables(false);
        paintingEvent.StartPaintingEvent();
    }

    public void ResumeGame()
    {
        SetGameState(GameState.GAMEPLAY);
        if (InventoryCanvas.activeSelf) InventoryCanvas.SetActive(false);
        AddEnemyCustomUpdatables(false);
    }

    public void ResumeFromEventScene()
    {
        SetGameState(GameState.GAMEPLAY);
    }

    public void GameOver()
    {
        SetGameState(GameState.GAMEOVER);
    }

    private void SetGameState(GameState newState, SubGameState newSubGameState = SubGameState.DEFAULT)
    {
        LastGameState = CurrentGameState;
        LastSubGameState = CurrentSubGameState;
        CurrentGameState = newState;
        CurrentSubGameState = newSubGameState;

        if (_debugMode) Debug.Log("GameManager.cs: SetGameState: " + CurrentGameState + "." + CurrentSubGameState);

        switch (CurrentGameState)
        {
            case GameState.MENU:
                if (_playerInput == null)
                {
                    _playerInput = FindObjectOfType<PlayerInput>();
                }
                if (EnemyPool != null)
                {
                    StopNavMeshAgents(true);
                }
                IsGamePaused = true;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                _playerInput.SwitchCurrentActionMap("Menu");
                break;

            case GameState.GAMEPLAY:
                if (_playerInput == null) _playerInput = FindObjectOfType<PlayerInput>();
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                switch (CurrentSubGameState)
                {
                    case SubGameState.DEFAULT:
                        StopNavMeshAgents(false);
                        IsGamePaused = false;
                        _playerInput.SwitchCurrentActionMap("Player");
                        if (_debugMode) Debug.Log("GameManager.cs: Switch to GamePlay.DEFAULT");
                        break;

                    case SubGameState.EVENT:
                        StopNavMeshAgents(true);
                        IsGamePaused = true;
                        _playerInput.SwitchCurrentActionMap("Event");
                        if (_debugMode) Debug.Log("GameManager.cs: Switch to GamePlay.EVENT");
                        break;

                    case SubGameState.PICKUP:
                        StopNavMeshAgents(true);
                        IsGamePaused = true;
                        _playerInput.SwitchCurrentActionMap("PickUp");
                        if (_debugMode) Debug.Log("GameManager.cs: Switch to GamePlay.PICKUP");
                        break;
                    case SubGameState.PAINT:
                        StopNavMeshAgents(true);
                        IsGamePaused = true;
                        _playerInput.SwitchCurrentActionMap("Painting");
                        if (_debugMode) Debug.Log("GameManager.cs: Switch to GamePlay.PAINT");
                        break;
                }
                break;

            case GameState.INVENTORY:
                StopNavMeshAgents(true);
                IsGamePaused = true;
                _playerInput.SwitchCurrentActionMap("Inventory");
                InventoryCanvas.SetActive(true);
                InventoryManager.Instance.ListItems();
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                break;

            case GameState.PAUSE:
                StopNavMeshAgents(true);
                IsGamePaused = true;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                break;

            case GameState.GAMEOVER:
                IsGamePaused = true;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                break;
        }
    }
    #endregion




    #region Message Management

    public void DisplayMessage(string message, float duration = 3f)
    {
        _messageText.text = message;
        if (!_messageCanvas.activeSelf) _messageCanvas.SetActive(true);
        StartCoroutine(HideMessage(duration, message));
    }

    IEnumerator HideMessage(float duration, string message)
    {
        yield return new WaitForSeconds(duration);
        if (string.Compare(message, _messageText.text) == 0)
        {
            _messageCanvas.SetActive(false);
        }
    }

    #endregion




    #region Save and Load

    public void LoadNewGame()
    {
        IsLoading = true;
        LoadNewScene("Asylum", true);
        if (_debugMode) Debug.Log("GameManager.cs: New Game Loaded!");
    }

    public void SaveData(string filename = "Asylum-autosave")
    {
        SaveSystem.SaveGameFile(filename, Player, InventoryManager.Instance.Weapons.transform, EnemyPool, InteractableObjectPool, _interactStaticObjectPool, _doorObjectPool, _textObjectPool, _summonObjectPool, _autoSavePool);
        if (_debugMode) Debug.Log("GameManager.cs: Player data saved!");
    }

    public void LoadData(string filename = "Asylum-autosave")
    {
        SaveData data = SaveSystem.LoadGameFile(filename);
        if (data != null)
        {
            LoadPlayerStats(data);
            LoadInventory(data);
            LoadWeapons(data);
            LoadAmmo(data);
            LoadEnemies(data);
            LoadInteractableObjects(data);
            LoadInteractableStaticObjects(data);
            LoadDoorObjects(data);
            LoadTextObjects(data);
            LoadSummonObjects(data);
            LoadEventData(data);
            LoadVariousData(data);
            LoadAutoSaveData(data);

            if (_debugMode) Debug.Log("GameManager.cs: Player data loaded!");
        }
        else Debug.LogError("GameManager.cs: No save data found!");
    }

    public void LoadNewScene(string filename, bool newGame = false)
    {
        IsLoading = true;

        string sceneNameToLoad = filename.Split('-')[0].Trim();

        StartCoroutine(LoadSceneAsync(sceneNameToLoad, filename, newGame));
    }

    IEnumerator LoadSceneAsync(string sceneName, string filename, bool newGame = false)
    {
        Blackscreen.SetActive(true);

        AsyncOperation asyncLoadLoadingScreen = SceneManager.LoadSceneAsync("LoadingScreen");

        while (!asyncLoadLoadingScreen.isDone)
        {
            yield return null;
        }

        _progressBar = GameObject.Find("ProgressBar").GetComponent<Image>();

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        while (!asyncLoad.isDone)
        {
            float targetFillAmount = Mathf.Lerp(_progressBar.fillAmount, asyncLoad.progress, Time.deltaTime * 5f);
            _progressBar.fillAmount = targetFillAmount;
            yield return null;
        }

        yield return new WaitForSeconds(2f);

        asyncLoad.allowSceneActivation = true;
        
        yield return new WaitForSeconds(1f);

        UpdateReferences();

        yield return new WaitForSeconds(1f);

        if (!newGame)
        {
            LoadData(filename);
        }
        else
        {
            if (_debugMode) Debug.Log("GameManager - Start - Gameplay: " + SceneManager.GetActiveScene().name);
            StartGame();
            StartCoroutine(StartWithUpdatedReferences());
        }

        yield return new WaitForSeconds(1f);
        
        if (!newGame)
        {
            if (_debugMode) Debug.Log("GameManager - Start - LoadGame: " + SceneManager.GetActiveScene().name);
            StartCoroutine(LoadGameWithBlackScreen());
        }
        else if (newGame)
        {
            foreach (Enemy enemy in EnemyPool.GetComponentsInChildren<Enemy>())
            {
                enemy.gameObject.SetActive(false);
            }
        }

        if (_debugMode) Debug.Log("Scene " + sceneName + " is fully loaded!");
        IsLoading = false;
    }

    private void LoadPlayerStats(SaveData data)
    {
        Player.Health = data.Health;
        Vector3 position = new Vector3(data.Position[0], data.Position[1], data.Position[2]);
        Player.transform.position = position;
    }

    private void LoadInventory(SaveData data)
    {
        InventoryManager.Instance.Items.Clear();

        foreach (ItemData itemData in data.Items)
        {
            Item item = CreateItemFromData(itemData);
            InventoryManager.Instance.AddItem(item);
        }
    }

    private void LoadWeapons(SaveData data)
    {
        foreach (WeaponData weaponData in data.Weapons)
        {
            Weapon weapon = InventoryManager.Instance.Weapons.transform.GetChild(weaponData.Index).GetComponent<Weapon>();
            weapon.CurrentAmmoInClip = weaponData.MagazineCount;
            weapon.IsAvailable = weaponData.IsAvailable;
            if (weaponData.IsEquipped) weapon.gameObject.SetActive(true);
            else weapon.gameObject.SetActive(false);
        }
    }

    private void LoadAmmo(SaveData data)
    {
        Ammo ammoSlot = Player.GetComponent<Ammo>();
        ammoSlot.ResetAmmo();

        foreach (Item item in InventoryManager.Instance.Items)
        {
            if (item.Type == ItemType.Ammo)
            {
                ammoSlot.IncreaseCurrentAmmo(item.AmmoType, item.Quantity);
            }
        }

        foreach (Weapon weapon in InventoryManager.Instance.Weapons.transform.GetComponentsInChildren<Weapon>(true))
        {
            if (weapon.IsAvailable)
            {
                ammoSlot.IncreaseCurrentAmmo(weapon.AmmoType, weapon.CurrentAmmoInClip);
            }

            if (weapon.IsEquipped)
            {
                int inventoryAmmo = InventoryManager.Instance.GetInventoryAmmo(weapon.AmmoType);
                Player.SetBulletsUI(weapon.CurrentAmmoInClip, inventoryAmmo);
            }
        }
    }

    private void LoadEnemies(SaveData data)
    {
        Enemy[] enemiesPool = EnemyPool.GetComponentsInChildren<Enemy>(true);
        Mannequin[] mannequinsPool = EnemyPool.GetComponentsInChildren<Mannequin>(true);

        foreach (EnemyData enemyData in data.Enemies)
        {
            foreach (Enemy enemy in enemiesPool)
            {
                if (enemy.GetComponent<UniqueIDComponent>().UniqueID == enemyData.UniqueID)
                {
                    enemy.HealthPoints = enemyData.Health;
                    enemy.IsDead = enemyData.IsDead;
                    enemy.transform.position = new Vector3(enemyData.Position[0], enemyData.Position[1], enemyData.Position[2]);
                    enemy.transform.rotation = Quaternion.Euler(enemyData.Rotation[0], enemyData.Rotation[1], enemyData.Rotation[2]);
                    if (enemy.IsDead)
                    {
                        enemy.gameObject.SetActive(false);
                    }
                    else
                    {
                        enemy.gameObject.SetActive(true);
                    }
                    if (!enemyData.IsActive) enemy.gameObject.SetActive(false);
                }
            }

            foreach (Mannequin mannequin in mannequinsPool)
            {
                if (mannequin.GetComponent<UniqueIDComponent>().UniqueID == enemyData.UniqueID)
                {
                    mannequin.IsDead = enemyData.IsDead;
                    mannequin.transform.position = new Vector3(enemyData.Position[0], enemyData.Position[1], enemyData.Position[2]);
                    mannequin.transform.rotation = Quaternion.Euler(enemyData.Rotation[0], enemyData.Rotation[1], enemyData.Rotation[2]);
                    if (mannequin.IsDead)
                    {
                        mannequin.gameObject.SetActive(false);
                    }
                    else if (enemyData.IsActive)
                    {
                        mannequin.gameObject.SetActive(true);
                    }
                    else mannequin.gameObject.SetActive(false);
                }
            }
        }
    }

    
    private void LoadInteractableObjects(SaveData data)
    {
        InteractableObject[] intObjectsPool = InteractableObjectPool.GetComponentsInChildren<InteractableObject>();
        Duplicate[] duplicatesPool = InteractableObjectPool.GetComponentsInChildren<Duplicate>();

        foreach (InteractableObjectData intObjectsData in data.InteractableObjects)
        {
            foreach (InteractableObject pickupObject in intObjectsPool)
            {
                if (pickupObject.GetComponent<UniqueIDComponent>().UniqueID == intObjectsData.UniqueID)
                {
                    pickupObject.IsActive = intObjectsData.IsActive;
                    if (!pickupObject.IsActive)
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
                    if (!horrorDoll.IsActive)
                    {
                        count++;
                    }
                }
            }

            InventoryManager.Instance.HorrorDollCount.text = count.ToString();

            foreach (Duplicate duplicate in duplicatesPool)
            {
                if (duplicate.DuplicateObject.GetComponent<UniqueIDComponent>().UniqueID == intObjectsData.UniqueID)
                {
                    duplicate.DuplicateObject.GetComponent<InteractableObject>().IsActive = intObjectsData.IsActive;
                    if (!duplicate.DuplicateObject.GetComponent<InteractableObject>().IsActive)
                    {
                        // Move the object to a position far away
                        duplicate.DuplicateObject.transform.position = new Vector3(0, -1000f, 0);
                    }
                }
            }
        }
    }

    private void LoadInteractableStaticObjects(SaveData data)
    {
        Drawer[] intObjPool;

        foreach (Transform staticObject in _interactStaticObjectPool.GetComponentsInChildren<Transform>())
        {
            intObjPool = staticObject.GetComponentsInChildren<Drawer>();

            foreach (InteractStaticObjectData intObjData in data.InteractStaticObjects)
            {
                foreach (Drawer drawer in intObjPool)
                {
                    if (drawer.GetComponent<UniqueIDComponent>().UniqueID == intObjData.UniqueID)
                    {
                        drawer.IsOpen = intObjData.IsOpen;
                        if (drawer.IsDrawer && intObjData.IsOpen) drawer.transform.position = new Vector3(intObjData.Position[0], intObjData.Position[1], intObjData.Position[2]);
                        else if (intObjData.IsOpen) drawer.transform.rotation = Quaternion.Euler(intObjData.Rotation[0], intObjData.Rotation[1], intObjData.Rotation[2]);
                    }
                }
            }
        }
    }

    private void LoadDoorObjects(SaveData data)
    {
        foreach (DoorObjectData doorData in data.Doors)
        {
            foreach (Duplicate door in _doorObjectPool.GetComponentsInChildren<Duplicate>())
            {
                if (door.DuplicateObject.GetComponent<UniqueIDComponent>().UniqueID == doorData.UniqueID)
                {
                    Door doorScript = door.DuplicateObject.GetComponent<Door>();
                    doorScript.IsLocked = doorData.IsLocked;
                    if (_debugMode) Debug.Log("GameManager.cs: Door " + door.DuplicateObject.name + " is " + (doorData.IsLocked ? "locked" : "unlocked") + " and " + (doorData.IsOpen ? "open" : "closed"));
                    if (doorData.IsOpen) doorScript.OpenDoor(false);
                }
            }
        }
    }

    private void LoadTextObjects(SaveData data)
    {
        foreach (TextObjectData textData in data.TextObjects)
        {
            foreach (TextCode textCode in _textObjectPool.GetComponentsInChildren<TextCode>())
            {
                if (textCode.GetComponent<UniqueIDComponent>().UniqueID == textData.UniqueID)
                {
                    textCode.IsAudioActive = textData.IsActive;
                }
            }
        }

        foreach (Duplicate duplicate in _textObjectPool.GetComponentsInChildren<Duplicate>())
        {
            foreach (TextObjectData textData in data.TextObjects)
            {
                if (duplicate.DuplicateObject.GetComponent<UniqueIDComponent>().UniqueID == textData.UniqueID)
                {
                    TextCode textCode = duplicate.DuplicateObject.GetComponent<TextCode>();
                    textCode.IsAudioActive = textData.IsActive;
                }
            }
        }
    }

    private void LoadSummonObjects(SaveData data)
    {
        foreach (SummonObjectData summonData in data.SummonObjects)
        {
            foreach (SummonObject summonObject in _summonObjectPool.GetComponentsInChildren<SummonObject>())
            {
                if (summonObject.GetComponent<UniqueIDComponent>().UniqueID == summonData.UniqueID)
                {
                    summonObject.IsObjectPlaced = summonData.IsObjectPlaced;
                    if (summonData.IsObjectPlaced) summonObject.LoadPlacedObject();
                }
            }
        }
    }

    private void LoadEventData(SaveData data)
    {
        foreach (SavedEventData savedEventData in data.Events)
        {
            foreach (EventDataEntry eventDataEntry in EventData.eventDataEntries)
            {
                if (eventDataEntry.EventName == savedEventData.EventName)
                {
                    if (savedEventData.Active)
                    {
                        eventDataEntry.Active = true;
                        EventData.TriggerEvent(eventDataEntry.EventName);
                    }
                }
            }
        }
    }

    private void LoadVariousData(SaveData data)
    {
        AudioManager.Instance.SetAudioClip(AudioManager.Instance.Environment, data.VariousData.EnvironmentMusicClip);
        AudioManager.Instance.PlayAudio(AudioManager.Instance.Environment, 0.1f, 1f, true);
        Player.IsLightAvailable = data.VariousData.IsFlashlightEnabled;
        if (Player.IsLightAvailable) Player.LightSwitch();
    }

    private void LoadAutoSaveData(SaveData data)
    {
        foreach (AutoSaveData autoSaveData in data.AutoSaveData)
        {
            foreach (AutoSave autoSave in _autoSavePool.GetComponentsInChildren<AutoSave>())
            {
                if (autoSave.GetComponent<UniqueIDComponent>().UniqueID == autoSaveData.UniqueID)
                {
                    if (!autoSaveData.IsActive)
                    {
                        autoSave.IsActive = false;
                        autoSave.GetComponent<Collider>().enabled = false;
                    }
                }
            }
        }
    }

    #endregion




    #region Helper Methods

    public void RemoveEnemyCustomUpdatables(bool exit = true)
    {
        foreach (AISensor enemy in EnemyPool.GetComponentsInChildren<AISensor>(exit))
        {
            if (exit) CustomUpdateManager.RemoveCustomUpdatable(enemy);
            enemy.PausePlayerInSight();
        }

        foreach (Mannequin mannequin in EnemyPool.GetComponentsInChildren<Mannequin>(exit))
        {
            CustomUpdateManager.RemoveCustomUpdatable(mannequin);
        }
    }

    private void AddEnemyCustomUpdatables(bool start = true)
    {
        foreach (AISensor enemy in EnemyPool.GetComponentsInChildren<AISensor>(start))
        {
            if (start) CustomUpdateManager.AddCustomUpdatable(enemy);
            enemy.ResumePlayerInSight();
        }

        foreach (Mannequin mannequin in EnemyPool.GetComponentsInChildren<Mannequin>(start))
        {
            CustomUpdateManager.AddCustomUpdatable(mannequin);
        }
    }

    private void StopNavMeshAgents(bool toggle)
    {
        foreach (NavMeshAgent agent in EnemyPool.GetComponentsInChildren<NavMeshAgent>())
        {
            if (agent.gameObject.activeSelf)
            {
                agent.isStopped = toggle;
                if (_debugMode) Debug.Log("GameManager.cs: " + agent.name + " is " + (toggle ? "stopped" : "moving"));
            }
        }
    }

    private Item CreateItemFromData(ItemData itemData)
    {
        Item newItem = ScriptableObject.CreateInstance<Item>();
        newItem.DisplayName = itemData.DisplayName;
        newItem.Description = itemData.Description;
        newItem.IsUnique = itemData.IsUnique;
        newItem.Quantity = itemData.Quantity;
        newItem.Type = (ItemType)System.Enum.Parse(typeof(ItemType), itemData.Type);
        newItem.AmmoType = (Ammo.AmmoType)System.Enum.Parse(typeof(Ammo.AmmoType), itemData.AmmoType);
        newItem.PotionType = (Potion.PotionType)System.Enum.Parse(typeof(Potion.PotionType), itemData.PotionType);
        newItem.IconPath = itemData.IconPath;
        newItem.Icon = Resources.Load<Sprite>(itemData.IconPath);
        newItem.PrefabPath = itemData.PrefabPath;
        newItem.Prefab = Resources.Load<GameObject>(itemData.PrefabPath);

        return newItem;
    }

    #endregion
}