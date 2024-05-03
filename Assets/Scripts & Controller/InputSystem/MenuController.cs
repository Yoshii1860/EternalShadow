using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.IO;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour, ICustomUpdatable
{
    #region Singleton

    public static MenuController Instance { get; private set; }

    #endregion




    #region Custom File Comparer

    // Custom comparer for sorting save files by date
    public class SaveFileComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            string format = "dd-MM-yyyy HH-mm-ss";
            System.DateTime xDate, yDate;

            // Try parsing the date and time from filenames
            if (System.DateTime.TryParseExact(x, format, System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out xDate) &&
                System.DateTime.TryParseExact(y, format, System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out yDate))
            {
                // Compare the parsed dates to determine the order
                return yDate.CompareTo(xDate); // Sorting in descending order (newest first)
            }
            else
            {
                // If parsing fails, just compare the original filenames
                return y.CompareTo(x);
            }
        }
    }

    #endregion




    #region Menu Canvas Variables

    [Header("Menu Canvas Reference")]
    public GameObject MenuCanvas;
    [Space(10)]

    [Header("Save/Load References and Settings")]
    [SerializeField] private GameObject _loadFilePrefab;
    [SerializeField] private GameObject _saveFilePrefab;
    [SerializeField] private Transform _saveFileContainer;
    [SerializeField] private Transform _loadFileContainer;
    [SerializeField] private int maxFiles = 8;
    [Space(10)]

    [Header("Menu References")]
    [SerializeField] private Transform _mainMenu;
    [SerializeField] private Transform _loadMenu;
    [SerializeField] private Transform _saveMenu;
    [SerializeField] private Transform _deathMenu;
    [Space(10)]

    [Header("Menu Buttons")]
    [SerializeField] private Transform _newGameButton;
    [SerializeField] private Transform _loadGameButton;
    [SerializeField] private Transform _exitButton;
    [SerializeField] private Button _returnFromLoadButton;
    [SerializeField] private Button _returnFromSaveButton;
    [Space(10)]

    [Header("Message Display Reference")]
    [SerializeField] private Transform _messageDisplay;
    [Space(10)]

    [Header("Menu Settings")]
    public bool CanCloseMenu = true;
    public float CloseMenuTimer = 0f;
    [Space(10)]

    [Header("Colors")]
    [SerializeField] private Color _selectedColor;
    [SerializeField] private Color _unselectedColor;
    [Space(10)]

    // Button and Menu Variables
    private Dictionary<Transform, Vector3> _initialButtonPositions = new Dictionary<Transform, Vector3>();
    private Transform[] _menus;
    private Button[] _buttons;
    private Button[] _decisionButtons;
    private Button _selectedButton;
    private int _currentButtonNumber;
    private bool _isDead = false;

    // Audio Source and Clips
    private AudioSource _audioSource;
    private AudioClip _enterClip;
    private AudioClip _clickClip;
    private AudioClip _menuDieClip;
    private AudioClip _menuMusic;
    private AudioClip _bootPC;
    private AudioClip _shutdownPC;
    private AudioClip _wooshClip;

    #endregion




    #region Menu Controller Variables

    [Header("Menu Controller Settings")]
    public float MoveDebounceTime = 0.3f;

    private bool _canMove = false;
    private bool _isDeciding = false;
    private bool _isAccepting = false;

    // Input Handling Variables
    private bool _isInteracting, _isGoingBack;
    private Vector2 _movePos;

    #endregion




    #region Unity Callbacks

    // Awake is called when the script instance is being loaded
    void Awake()
    {
        #region Singleton

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

        #endregion



        // Initialize the menu variables
        _menus = new Transform[] { _mainMenu, _loadMenu, _saveMenu, _deathMenu };
        _initialButtonPositions[_newGameButton.transform] = _newGameButton.transform.position;
        _initialButtonPositions[_loadGameButton.transform] = _loadGameButton.transform.position;
        _initialButtonPositions[_exitButton.transform] = _exitButton.transform.position;
        _audioSource = GetComponent<AudioSource>();
        _enterClip = Resources.Load<AudioClip>("SFX/clicker");
        _clickClip = Resources.Load<AudioClip>("SFX/click");
        _menuDieClip = Resources.Load<AudioClip>("SFX/menu die");
        _menuMusic = Resources.Load<AudioClip>("SFX/menu music");
        _bootPC = Resources.Load<AudioClip>("SFX/boot pc");
        _shutdownPC = Resources.Load<AudioClip>("SFX/shutdown pc");
        _wooshClip = Resources.Load<AudioClip>("SFX/woosh");
    }

    // Start is called before the first frame update
    void Start()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            ActivateMenu(_mainMenu, true);
        }
        else
        {
            ActivateMenu(_mainMenu, false);
            MenuCanvas.SetActive(false);
        }
    }

    #endregion




    #region Custom Update

    // Custom Update method to handle menu interactions
    public void CustomUpdate(float deltaTime)
    {
        if (CloseMenuTimer < .5f)   CloseMenuTimer += deltaTime;
        else                        CanCloseMenu = true;

        if (GameManager.Instance.CurrentGameState == GameManager.GameState.MENU)
        {
            if (_isInteracting) Interact();
            if (_isGoingBack) Back();
        }
    }

    #endregion




    #region Public Methods

    // Method to display the death screen - called from GameManager
    public void DeathScreen()
    {
        _isDead = true;
        ActivateMenu(_deathMenu, false);
    }

    // Method to display the main menu - called from GameManager
    public void MainMenu()
    {
        ActivateMenu(_mainMenu, true);
    }

    #endregion

    


    #region Input Handling

    public void OnInteract(InputAction.CallbackContext context)
    {
        _isInteracting = context.ReadValueAsButton();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _movePos = context.ReadValue<Vector2>();

        // Debounce the move input
        float deltaTime = Time.deltaTime;
        if (Time.time - MoveDebounceTime > deltaTime)
        {
            MoveDebounceTime = Time.time;
            if (!_canMove) Move();
        }
    }

    public void OnBack(InputAction.CallbackContext context)
    {
        _isGoingBack = context.ReadValueAsButton();
    }

    #endregion




    #region Menu Interaction Methods

    // Method to handle menu interactions
    private void Interact()
    {
        _isInteracting = false;
        
        if (_selectedButton == null) return;

        if (_isDeciding) 
        {
            DecisionInput();
        }

        // Play the woosh sound effect
        if (string.Compare(_selectedButton.gameObject.name, "NewGame") == 0)
            _audioSource.PlayOneShot(_wooshClip, 0.5f);
        else
            _audioSource.PlayOneShot(_enterClip, 0.5f);

        // Check the selected button and perform the appropriate action
        if (string.Compare(_selectedButton.gameObject.name, "NewGame") == 0)                     StartCoroutine(StartNewGame());
        else if (string.Compare(_selectedButton.gameObject.name, "LoadGame") == 0)               ActivateMenu(_loadMenu);
        else if (string.Compare(_selectedButton.gameObject.name, "Return") == 0 
                && _loadMenu.gameObject.activeSelf)                                              Return(_returnFromLoadButton);
        else if (string.Compare(_selectedButton.gameObject.name, "Return") == 0 
                && _saveMenu.gameObject.activeSelf)                                              Return(_returnFromSaveButton);
        else if (_selectedButton.gameObject.CompareTag("LoadFileButton"))                        StartCoroutine(LoadFile());
        else if (_selectedButton.gameObject.CompareTag("SaveFileButton"))                        StartCoroutine(SaveFile());
        else if (string.Compare(_selectedButton.gameObject.name, "ExitGame") == 0)               StartCoroutine(ExitGame());
        else if (string.Compare(_selectedButton.gameObject.name, "Retry") == 0)                  LoadAuotsave();
    }

    // Method to handle decision input
    private void DecisionInput()
    {
        if (_selectedButton.gameObject.CompareTag("Yes"))
        {
            _isAccepting = true;
            _isDeciding = false;
            return;
        }
        else if (_selectedButton.gameObject.CompareTag("No"))
        {
            _isAccepting = false;
            _isDeciding = false;
            return;
        }
    }

    #endregion




    #region Menu Navigation Methods

    // Method to handle menu navigation
    private void Move()
    {
        // If the player is deciding, move on the decision
        if (_isDeciding)
        {
            MoveOnDecision();
            return;
        }

        // If no button is selected, select the first button
        if (_selectedButton == null) 
        {
            if (_buttons.Length == 0)
            {
                if (_selectedButton != _returnFromLoadButton || _selectedButton != _returnFromSaveButton)
                {
                    _selectedButton = (_saveMenu.gameObject.activeSelf) ? _returnFromSaveButton : _returnFromLoadButton;
                    _currentButtonNumber = 0;
                    ChangeSelectedButtonColor(_selectedButton);
                    return;
                }
            }
            _selectedButton = _buttons[0];
            _currentButtonNumber = 0;
            ChangeSelectedButtonColor(_selectedButton);
            return;
        }

        // Move the selected button based on the input
        if (_movePos.y > 0.5f && _buttons.Length > 0)
        {
            MoveUp(true);
        }
        else if (_movePos.y < -0.5f && _buttons.Length > 0)
        {
            MoveUp(false);
        }
        else if (_movePos.x > 0.5f && _buttons.Length > 0)
        {
            MoveRight(true);
        }
        else if (_movePos.x < -0.5f && _buttons.Length > 0)
        {
            MoveRight(false);
        }
        else return;
    }

    // Method to move on decision buttons
    private void MoveOnDecision()
    {
        if (_movePos.x > 0.5f)
        {
            if (_selectedButton == _decisionButtons[1]) return;
            _selectedButton = _decisionButtons[1];
            ChangeSelectedButtonColor(_selectedButton);
        }
        else if (_movePos.x < -0.5f)
        {
            if (_selectedButton == _decisionButtons[0]) return;
            _selectedButton = _decisionButtons[0];
            ChangeSelectedButtonColor(_selectedButton);
        }
    }

    // Method to move the selected button up or down
    private void MoveUp(bool moveUp)
    {
        if (!moveUp && _currentButtonNumber + 1 >= _buttons.Length || moveUp && _currentButtonNumber - 1 < 0) return;

        _currentButtonNumber = moveUp ? _currentButtonNumber - 1 : _currentButtonNumber + 1;
        _selectedButton = _buttons[_currentButtonNumber];
        ChangeSelectedButtonColor(_selectedButton);
    }

    // Method to move the selected button right or left
    private void MoveRight(bool moveRight)
    {
        if (!_saveMenu.gameObject.activeSelf && !_loadMenu.gameObject.activeSelf) return;

        if (moveRight)
        {
            if (_selectedButton == _returnFromLoadButton || _selectedButton == _returnFromSaveButton) return;
            
            _selectedButton = _saveMenu.gameObject.activeSelf ? _returnFromSaveButton : _returnFromLoadButton;
            _currentButtonNumber = 0;
            ChangeSelectedButtonColor(_selectedButton);
        }
        else
        {
            if (_selectedButton == _returnFromLoadButton || _selectedButton == _returnFromSaveButton)
            {
                _selectedButton = _buttons[0];
                _currentButtonNumber = 0;
                ChangeSelectedButtonColor(_selectedButton);
            }
        }
    }

    // Method to handle going back in the menu
    private void Back()
    {
        _isGoingBack = false;

        if (!CanCloseMenu) return;

        // Check if the main menu is active
        bool mainMenuActive = SceneManager.GetActiveScene().name == "MainMenu";

        if (mainMenuActive && _mainMenu.gameObject.activeSelf) return;

        // if the main menu scene is not active
        // and the main menu is open, resume the game
        if (_mainMenu.gameObject.activeSelf && CanCloseMenu && !_isDead)         
        {
            ActivateMenu(_mainMenu, false);
            GameManager.Instance.ResumeGame();
            MenuCanvas.SetActive(false);
            _audioSource.PlayOneShot(_enterClip, 0.5f);
            return;
        }

        // if the main menu scene is not active
        // and the load/save menu is open, return to the main menu
        if (_loadMenu.gameObject.activeSelf || _saveMenu.gameObject.activeSelf)
        {
            ActivateMenu(_isDead ? _deathMenu : _mainMenu, mainMenuActive);
        }
    }

    #endregion




    #region Menu Activation and Configuration

    // Method to activate the menu and configure the buttons
    public void OnMainMenu(bool mainMenuState)
    {
        // Set the new game button active based on the main menu state
        _newGameButton.gameObject.SetActive(mainMenuState);
        // Set the button positions based on the main menu state
        _loadGameButton.transform.position = _initialButtonPositions[mainMenuState ? _loadGameButton.transform : _newGameButton.transform];
        _exitButton.transform.position = _initialButtonPositions[mainMenuState ? _exitButton.transform : _loadGameButton.transform];

        // Reset the buttons array and color
        ResetButtonColor();
        List <Button> buttonList = new List<Button>(_buttons);
        buttonList.Remove(_returnFromLoadButton);
        buttonList.Remove(_returnFromSaveButton);

        // Remove the new game button from the button list if the main menu is not active
        if (!mainMenuState)   buttonList.Remove(_newGameButton.GetComponent<Button>());

        // Set the buttons array and selected button
        _buttons = buttonList.ToArray();
        _selectedButton = null;
        _currentButtonNumber = 0;
    }

    // Method to activate the menu and configure the buttons
    private void ActivateMenu(Transform menu, bool mainMenuState = false)
    {
        // If the main menu is active, play the menu music
        if (mainMenuState) 
        {
            if (!_audioSource.isPlaying)
            {
                StartCoroutine(ActivateMenuAudio());
            }
        }

        _newGameButton.gameObject.SetActive(true);

        // Activate the selected menu and deactivate the others
        foreach (Transform m in _menus)
        {
            if (m == menu)  
            {
                m.gameObject.SetActive(true);
                _buttons = m.GetComponentsInChildren<Button>();
            }
            else     
            {       
                m.gameObject.SetActive(false);
            }
        }

        // If the menu is not the load or save menu, return to the main menu
        if (menu != _loadMenu && menu != _saveMenu) 
        {
            OnMainMenu(mainMenuState);
        }
        // If the menu is the load or save menu, populate the menu with saved files
        else
        {
            PopulateMenu(   menu == _loadMenu ? _loadFileContainer : _saveFileContainer, 
                            menu == _loadMenu ? _loadFilePrefab : _saveFilePrefab);
        }
    }

    // Method to reset the button colors
    private void ResetButtonColor()
    {
        foreach (Button b in _buttons)
        {
            b.GetComponentInChildren<Image>().color = _unselectedColor;
        }
        _returnFromLoadButton.GetComponentInChildren<Image>().color = _unselectedColor;
        _returnFromSaveButton.GetComponentInChildren<Image>().color = _unselectedColor;
    }

    // Method to change the selected button color
    private void ChangeSelectedButtonColor(Button button)
    {
        // If the player is deciding, change the decision button color
        if (_isDeciding)
        {
            foreach (Button b in _decisionButtons)
            {
                b.GetComponentInChildren<Image>().color = _unselectedColor;
            }
            button.GetComponentInChildren<Image>().color = _selectedColor;
            _audioSource.PlayOneShot(_clickClip, 0.5f);
            return;
        }

        // Unslect all buttons and select the new button
        foreach (Button b in _buttons)
        {
            b.GetComponentInChildren<Image>().color = _unselectedColor;
        }

        _returnFromLoadButton.GetComponentInChildren<Image>().color = _unselectedColor;
        _returnFromSaveButton.GetComponentInChildren<Image>().color = _unselectedColor;
        button.GetComponentInChildren<Image>().color = _selectedColor;

        // Play the click sound effect
        _audioSource.PlayOneShot(_clickClip, 0.5f);
    }

    #endregion




    #region Audio Methods

    // Method to activate the menu audio over time
    private IEnumerator ActivateMenuAudio()
    {
        _audioSource.volume = 0f;
        _audioSource.clip = _menuMusic;
        _audioSource.loop = true;
        _audioSource.Play();
        while (_audioSource.volume < 1f)
        {
            _audioSource.volume += Time.deltaTime;
            yield return null;
        }
        _audioSource.volume = 1f;
    }

    // Method to deactivate the menu audio over time
    private IEnumerator DeactiveMenuAudio()
    {
        while (_audioSource.volume > 0)
        {
            _audioSource.volume -= Time.deltaTime;
            yield return null;
        }
        _audioSource.Stop();
        _audioSource.volume = 1f;
    }

    #endregion




    #region Save and Load Methods

    // Method to display the load menu - Called from a PC in the game
    public void SaveGame()
    {
        // Play the boot PC sound effect
        _audioSource.PlayOneShot(_bootPC, 0.5f);

        ActivateMenu(_saveMenu);
    }

    // Method to populate the menu with saved files
    private void PopulateMenu(Transform container, GameObject prefab)
    {
        // Get the list of saved files
        string[] savedFiles = GetSavedFiles();

        // Sort the savedFiles array based on the date and time in filenames
        System.Array.Sort(savedFiles, new SaveFileComparer());

        // Create a button for each saved file
        int filecount = 0;
        foreach (string file in savedFiles)
        {
            GameObject fileObject = Instantiate(prefab, container);
            fileObject.GetComponentInChildren<TextMeshProUGUI>().text = file;
            filecount++;
        }

        // Create empty buttons for the remaining slots
        if (prefab.CompareTag("SaveFileButton"))
        {
            for (int i = filecount; i < maxFiles; i++)
            {
                GameObject fileObject = Instantiate(prefab, container);
                fileObject.GetComponentInChildren<TextMeshProUGUI>().text = "";
            }
        }

        // Reset the button colors and set the buttons List
        ResetButtonColor();
        _buttons = container.GetComponentsInChildren<Button>();
        List <Button> buttonList = new List<Button>(_buttons);

        // Remove the return buttons from the button list
        buttonList.Remove(_returnFromLoadButton);
        buttonList.Remove(_returnFromSaveButton);

        // Set the buttons array and selected button
        _buttons = buttonList.ToArray();
        _selectedButton = null;
        _currentButtonNumber = 0;
    }

    // Function to get the list of saved file names (excluding file extensions)
    private string[] GetSavedFiles()
    {
        // Get all files in the persistent data path with the ".shadow" extension
        string[] savedFiles = Directory.GetFiles(Application.persistentDataPath, "*.shadow");
        for (int i = 0; i < savedFiles.Length; i++)
        {
            // Remove the directory path and ".shadow" extension, leaving just the file names
            savedFiles[i] = Path.GetFileNameWithoutExtension(savedFiles[i]);
        }
        return savedFiles;
    }

    // Coroutine to load a saved file
    IEnumerator LoadFile()
    {
        // Check if the player is in the main menu and let decide if they want to load a new game
        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            Button saveButton = _selectedButton;

            ////////////////////////////
            // Decision
            ////////////////////////////
            _isDeciding = true;
            MessageDisplay("Unsaved progress will be lost. Continue?");
            yield return new WaitUntil(() => !_isDeciding);
            _messageDisplay.gameObject.SetActive(false);
            _selectedButton = saveButton;
            if (!_isAccepting)
            {
                yield break;
            }
            else
            {
                _isAccepting = false;
            }
            ////////////////////////////
            // Decision End
            ////////////////////////////
        }

        // Get the file name from the button text
        string filename = _selectedButton.GetComponentInChildren<TextMeshProUGUI>().text;

        // Delete all children of the parent canvas to prevent duplicates
        foreach (Transform file in _loadFileContainer)
        {
            Destroy(file.gameObject);
        }

        // Deactivate the menu audio
        if (_audioSource.isPlaying) StartCoroutine(DeactiveMenuAudio());

        // Deactivate the menu and load the new scene
        ActivateMenu(_mainMenu, false);
        MenuCanvas.SetActive(false);
        GameManager.Instance.LoadNewScene(filename);

        yield return null;
    }

    // Coroutine to save the game data
    IEnumerator SaveFile()
    {
        // Get the file name from the button text
        string filename = _selectedButton.GetComponentInChildren<TextMeshProUGUI>().text;

        // Check if the player is overwriting a file and let them decide to do so
        if (filename != "" && filename != "Asylum-autosave")
        {
            ////////////////////////////
            // Decision
            ////////////////////////////
            _isDeciding = true;
            MessageDisplay("Overwrite file?");
            yield return new WaitUntil(() => !_isDeciding);
            _messageDisplay.gameObject.SetActive(false);
            if (_isAccepting)
            {
                File.Delete(Path.Combine(Application.persistentDataPath, filename + ".shadow"));
                _isAccepting = false;
            }
            else
            {
                yield break;
            }
            ////////////////////////////
            // Decision End
            ////////////////////////////
        }
        // Check if the player is trying to overwrite the autosave file
        else if (filename == "Asylum-autosave")
        {
            GameManager.Instance.DisplayMessage("Can`t overwrite autosave", 2f);
            yield break;
        }

        // Get Scene Name and Date/Time
        Scene scene = SceneManager.GetActiveScene();
        filename = scene.name + " - " + System.DateTime.Now.ToString("dd-MM-yyy HH-mm-ss");

        // Delete all children of the parent canvas to prevent duplicates
        foreach (Transform file in _saveFileContainer)
        {
            Destroy(file.gameObject);
        }

        // Save the game data
        GameManager.Instance.SaveData(filename);
        _isInteracting = false;
        _canMove = true;
        _selectedButton = null;

        yield return new WaitForSeconds(1f);

        // Play the shutdown PC sound effect
        _audioSource.PlayOneShot(_shutdownPC, 0.5f);

        yield return new WaitForSeconds(1f);

        // Deactivate the menu 
        _canMove = false;
        ActivateMenu(_mainMenu, false);
        MenuCanvas.SetActive(false);
    }

    // Method to load the autosave file
    public void LoadAuotsave()
    {
        // Get the list of saved files
        string[] savedFiles = GetSavedFiles();
        string autosave = "Asylum-autosave";

        // Load the autosave file from the saved files array
        foreach (string file in savedFiles)
        {
            if (file.Contains(autosave))
            {
                GameManager.Instance.LoadNewScene(file, false);
            }
        }

        // Deactivate the menu
        _deathMenu.gameObject.SetActive(false);
        ActivateMenu(_mainMenu, false);
        MenuCanvas.SetActive(false);
    }

    // Method to display a message on the screen for the player to decide
    private void MessageDisplay(string message)
    {
        _messageDisplay.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = message;
        _decisionButtons = _messageDisplay.GetComponentsInChildren<Button>(true);
        _messageDisplay.gameObject.SetActive(true);
    }

    #endregion




    #region Start, Return and Exit Method

    // Method to return to the main menu
    void Return(Button returnButton)
    {
        // Check if the player is in the main menu
        bool isMainMenu = SceneManager.GetActiveScene().name == "MainMenu";
        
        // If the return button is a load, return to the main menu
        if (returnButton.gameObject.CompareTag("LoadReturnButton"))
        {
            foreach (Transform file in _loadFileContainer)
            {
                Destroy(file.gameObject);
            }
            ActivateMenu(_mainMenu, isMainMenu);
        }
        // If the return button is a save, resume the game
        else if (returnButton.gameObject.CompareTag("SaveReturnButton"))
        {
            foreach (Transform file in _saveFileContainer)
            {
                Destroy(file.gameObject);
            }

            // Deactivate the menu audio
            ActivateMenu(_mainMenu, false);
            MenuCanvas.SetActive(false);

            // Play the click sound effect
            _audioSource.PlayOneShot(_enterClip, 0.5f);

            // Resume the game
            GameManager.Instance.ResumeGame();
        }
    }

    // Method to exit the game
    private IEnumerator ExitGame()
    {
        // Check if the player is in the main menu scene
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            Application.Quit();
        }
        // If the player is not in the main menu scene, let them decide to exit to the main menu
        else
        {
            ////////////////////////////
            // Decision
            ////////////////////////////
            _isDeciding = true;
            MessageDisplay("Exit to Main Menu?");
            yield return new WaitUntil(() => !_isDeciding);
            _messageDisplay.gameObject.SetActive(false);

            // If the player accepts, exit to the main menu
            if (_isAccepting)
            {
                _isAccepting = false;
                GameManager.Instance.RemoveEnemyCustomUpdatables();
                SceneManager.LoadScene("MainMenu");
                ActivateMenu(_mainMenu, true);
                _audioSource.Play();
            }
            else
            {
                yield break;
            }
            ////////////////////////////
            // Decision End
            ////////////////////////////
        }
    }

    // Coroutine to start a new game
    private IEnumerator StartNewGame()
    {
        _isInteracting = false;
        _selectedButton = null;
        _canMove = true;

        yield return new WaitForSeconds(2f);

        // Play the menu sound effect
        _audioSource.PlayOneShot(_menuDieClip, 0.8f);

        _canMove = false;

        // Load the new game and deactivate the menu
        GameManager.Instance.LoadNewGame();
        ActivateMenu(_mainMenu, false);
        MenuCanvas.SetActive(false);

        yield return new WaitForSeconds(1f);

        // Deactivate the menu audio
        StartCoroutine(DeactiveMenuAudio());
    }

    #endregion
}