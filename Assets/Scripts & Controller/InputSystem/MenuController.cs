using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.IO;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour, ICustomUpdatable, IPointerEnterHandler, IPointerExitHandler
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
    [SerializeField] private Transform _optionsMenu;
    [SerializeField] private Transform _deviceMenu;
    [SerializeField] private Transform _graphicsMenu;
    [Space(10)]

    [Header("Menu Buttons")]
    [SerializeField] private Transform _newGameButton;
    [SerializeField] private Transform _returnGameButton;
    //[SerializeField] private Transform _loadGameButton;
    [SerializeField] private Transform _exitButton;
    [SerializeField] private Button _returnFromLoadButton;
    [SerializeField] private Button _returnFromSaveButton;
    [Space(10)]

    [Header("Message Display Reference")]
    [SerializeField] private Transform _messageDisplay;
    [SerializeField] private GameObject _sensitivityDisplay;
    [SerializeField] private Slider _sensitivitySlider;
    [Space(10)]

    [Header("Other Menu Settings")]
    public bool CanCloseMenu = true;
    public float CloseMenuTimer = 0f;
    public float SensitivityValue = 0f;
    [SerializeField] private TextMeshProUGUI _sensitivityText;
    [SerializeField] private GameObject _mouseCheckbox;
    [SerializeField] private GameObject _controllerCheckbox;
    [Space(10)]

    [Header("Colors")]
    [SerializeField] private Color _selectedColor;
    [SerializeField] private Color _unselectedColor;
    [Space(10)]

    // Button and Menu Variables
    private Transform[] _menus;
    private Button[] _buttons;
    private Button[] _decisionButtons;
    private Button _selectedButton;
    private int _currentButtonNumber;
    private bool _isDead = false;
    private bool _isAdjustingSensitivity = false;

    // Audio Source and Clips
    private AudioSource _audioSource;
    private AudioClip _enterClip;
    private AudioClip _clickClip;
    private AudioClip _menuDieClip;
    private AudioClip _menuMusic;
    private AudioClip _bootPC;
    private AudioClip _shutdownPC;
    private AudioClip _wooshClip;

    private float _clickTimer = 0f;

    #endregion




    #region Menu Controller Variables

    [Header("Menu Controller Settings")]
    public float MoveDebounceTime = 0.3f;

    private bool _canMove = false;
    private bool _isDeciding = false;
    private bool _isAccepting = false;
    private bool _isLoading = false;

    // Input Handling Variables
    private bool _isInteracting, _isGoingBack;
    private Vector2 _movePos;
    private float _moveSensitivity;

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

        _menus = new Transform[] { _mainMenu, _loadMenu, _saveMenu, _deathMenu, _optionsMenu, _deviceMenu, _graphicsMenu};
        _audioSource = GetComponent<AudioSource>();
        _enterClip = Resources.Load<AudioClip>("SFX/clicker");
        _clickClip = Resources.Load<AudioClip>("SFX/click");
        _menuDieClip = Resources.Load<AudioClip>("SFX/menu die");
        _menuMusic = Resources.Load<AudioClip>("SFX/menu music");
        _bootPC = Resources.Load<AudioClip>("SFX/boot pc");
        _shutdownPC = Resources.Load<AudioClip>("SFX/shutdown pc");
        _wooshClip = Resources.Load<AudioClip>("SFX/woosh");

        InitializeInputDevice();
        InitializeGraphicsQuality();
    }

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




    #region Initialization Methods

    private void InitializeInputDevice()
    {
        int deviceChoice = PlayerPrefs.GetInt("InputDevice", 0);
        if (Gamepad.all.Count == 0)
        {
            PlayerPrefs.SetInt("InputDevice", 0);
            OnMouseSelected();
        }
        else if (deviceChoice == 0) 
        {
            OnMouseSelected();
        }
        else if (deviceChoice == 1) 
        {
            OnControllerSelected();
        }

        float sensitivityValue = PlayerPrefs.GetFloat("Sensitivity", 0);
        if (sensitivityValue != 0)
        {
            SensitivityValue = sensitivityValue;
            _sensitivitySlider.value = SensitivityValue;
            _sensitivityText.text = SensitivityValue.ToString();
        }
    }

    private void InitializeGraphicsQuality()
    {
        int graphicsChoice = PlayerPrefs.GetInt("GraphicsQuality", 1);
        if (graphicsChoice == 0)
        {
            OnLowQualitySelected();
        }
        else if (graphicsChoice == 1)
        {
            OnMediumQualitySelected();
        }
        else if (graphicsChoice == 2)
        {
            OnHighQualitySelected();
        }
    }

    #endregion




    #region Custom Update

    // Custom Update method to handle menu interactions
    public void CustomUpdate(float deltaTime)
    {
        if (CloseMenuTimer < .5f)   CloseMenuTimer += deltaTime;
        else                        CanCloseMenu = true;

        _clickTimer += deltaTime;

        if (GameManager.Instance.CurrentGameState == GameManager.GameState.MENU)
        {
            if (_isInteracting) Interact();
            if (_isGoingBack) Back();
        }

        if (Gamepad.all.Count == 0)
        {
            PlayerPrefs.SetInt("InputDevice", 0);
            OnMouseSelected();
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
            if (!_canMove && !_isLoading) Move();
        }
    }

    public void OnScroll(InputAction.CallbackContext context)
    {
        if (_isAdjustingSensitivity)
        {
            _moveSensitivity = context.ReadValue<float>();
            MoveOnSensitivity();
        }
    }

    public void OnBack(InputAction.CallbackContext context)
    {
        _isGoingBack = context.ReadValueAsButton();
    }

    #endregion




    #region Menu Keyboard Interaction Methods

    // Method to handle menu interactions
    private void Interact()
    {
        _isInteracting = false;

        if (_isAdjustingSensitivity)
        {
            StartCoroutine(StopAdjustingSensitivity());
            return;
        }
        
        if (_selectedButton == null || _isLoading == false) 
        {
            return;
        }

        if (_isDeciding) 
        {
            DecisionInput();
        }

        // Play the woosh sound effect
        if (string.Compare(_selectedButton.gameObject.name, "NewGame") == 0)
        {
            _audioSource.PlayOneShot(_wooshClip, 0.5f);
            StartCoroutine(StartNewGame());
            return;
        }
        else _audioSource.PlayOneShot(_enterClip, 0.5f);

        // Check the selected button and perform the appropriate action
        if (string.Compare(_selectedButton.gameObject.name, "LoadGame") == 0)                    ActivateMenu(_loadMenu);
        else if (string.Compare(_selectedButton.gameObject.name, "Return") == 0)                 Return();
        else if (_graphicsMenu.gameObject.activeSelf)                                            Graphics();
        else if (_deviceMenu.gameObject.activeSelf)                                              Devices();
        else if (_selectedButton.gameObject.CompareTag("LoadFileButton"))                        StartCoroutine(LoadFile());
        else if (_selectedButton.gameObject.CompareTag("SaveFileButton"))                        StartCoroutine(SaveFile());
        else if (_selectedButton.gameObject.CompareTag("OptionsButton"))                         ActivateMenu(_optionsMenu);
        else if (_selectedButton.gameObject.CompareTag("InputButton"))                           ActivateMenu(_deviceMenu);
        else if (_selectedButton.gameObject.CompareTag("GraphicsButton"))                        ActivateMenu(_graphicsMenu);
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




    #region Button Navigation Methods

    public void StartNewGameButton()
    {
        if (_isDeciding) return;
        _audioSource.PlayOneShot(_wooshClip, 0.5f);
        StartCoroutine(StartNewGame());
    }

    public void LoadGameButton()
    {
        if (_isDeciding) return;
        _audioSource.PlayOneShot(_enterClip, 0.5f);
        ActivateMenu(_loadMenu);
    }

    public void OptionsButton()
    {
        if (_isDeciding) return;
        _audioSource.PlayOneShot(_enterClip, 0.5f);
        ActivateMenu(_optionsMenu);
    }

    public void GraphicsButton()
    {
        _audioSource.PlayOneShot(_enterClip, 0.5f);
        ActivateMenu(_graphicsMenu);
    }

    public void LowQualityButton()
    {
        _audioSource.PlayOneShot(_enterClip, 0.5f);
        OnLowQualitySelected();
        ShowChosenGraphics();
    }

    public void MediumQualityButton()
    {
        _audioSource.PlayOneShot(_enterClip, 0.5f);
        OnMediumQualitySelected();
        ShowChosenGraphics();
    }

    public void HighQualityButton()
    {
        _audioSource.PlayOneShot(_enterClip, 0.5f);
        OnHighQualitySelected();
        ShowChosenGraphics();
    }

    public void InputButton()
    {
        _audioSource.PlayOneShot(_enterClip, 0.5f);
        ActivateMenu(_deviceMenu);
    }

    public void MouseButton()
    {
        _audioSource.PlayOneShot(_enterClip, 0.5f);
        OnMouseSelected();
        ShowActiveDevice();
    }

    public void ControllerButton()
    {
        _audioSource.PlayOneShot(_enterClip, 0.5f);
        OnControllerSelected();
        ShowActiveDevice();
    }

    public void ExitGameButton()
    {
        if (_isDeciding) return;
        _audioSource.PlayOneShot(_enterClip, 0.5f);
        StartCoroutine(ExitGame());
    }

    public void ReturnButton()
    {
        if (_isDeciding) return;
        _audioSource.PlayOneShot(_enterClip, 0.5f);
        Return();
    }

    public void RetryButton()
    {
        if (_isDeciding) return;
        _audioSource.PlayOneShot(_wooshClip, 0.5f);
        LoadAuotsave();
    }

    public void YesButton()
    {
        _audioSource.PlayOneShot(_enterClip, 0.5f);
        _isAccepting = true;
        _isDeciding = false;
    }

    public void NoButton()
    {
        _audioSource.PlayOneShot(_enterClip, 0.5f);
        _isAccepting = false;
        _isDeciding = false;
    }

    public void SensitivityButton()
    {
        StartCoroutine(AdjustSensitivity());
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Pointer Enter: " + eventData.hovered[0].name + ", " + eventData.hovered[1].name);
        if (_isLoading) return;

        if (_isDeciding)
        {
            foreach (GameObject obj in eventData.hovered)
            {
                Debug.Log(obj.name);
                if (obj.CompareTag("Yes") || obj.CompareTag("No"))
                {
                    _selectedButton = obj.GetComponent<Button>();
                    ChangeHoverButtonColor(true);
                }
            }
        }
        else
        {
            foreach (GameObject obj in eventData.hovered)
            {
                if (obj.GetComponent<Button>() != null)
                {
                    _selectedButton = obj.GetComponent<Button>();
                    ChangeHoverButtonColor(true);
                    return;
                }
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_isLoading) return;
        ChangeHoverButtonColor(false);
    }

    public void OnChangingSensitivity()
    {
        // round the sensitivity value to the nearest 0.1
        SensitivityValue = Mathf.Round(_sensitivitySlider.value * 10) / 10;
        _sensitivitySlider.value = SensitivityValue;
    }

    public void LoadFileButton()
    {
        _audioSource.PlayOneShot(_enterClip, 0.5f);
        StartCoroutine(LoadFile());
    }

    public void SaveFileButton()
    {
        StartCoroutine(SaveFile());
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
        else if (_isAdjustingSensitivity) 
        {
            MoveOnSensitivity();
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

    // Method to move on sensitivity buttons
    private void MoveOnSensitivity()
    {
        if (_moveSensitivity > 0.5f)
        {
            if (SensitivityValue < 1f) 
            {
                _sensitivitySlider.value += 0.1f;
            }
        }
        else if (_moveSensitivity < -0.5f)
        {
            if (SensitivityValue > -1f) 
            {
                _sensitivitySlider.value -= 0.1f;
            }
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
        // Set the new game button and return button active based on the main menu state
        if (_mainMenu.gameObject.activeSelf)
        {
            _newGameButton.gameObject.SetActive(mainMenuState);
            _returnGameButton.gameObject.SetActive(!mainMenuState);
        }

        // Reset the buttons array and color
        ResetButtonColor();
        List <Button> buttonList = new List<Button>(_buttons);
        buttonList.Remove(_returnFromLoadButton);
        buttonList.Remove(_returnFromSaveButton);

        // Remove the new game button from the button list if the main menu is not active and add the return button
        if (!mainMenuState)     
        {
            buttonList.Remove(_newGameButton.GetComponent<Button>());
            if (_mainMenu.gameObject.activeSelf)
            {
                buttonList.Insert(0, _returnGameButton.GetComponent<Button>());
            }
        }
        else 
        {
            buttonList.Remove(_returnGameButton.GetComponent<Button>());
        }

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

            _newGameButton.gameObject.SetActive(true);
            _returnGameButton.gameObject.SetActive(false);
        }

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

        if (menu == _deviceMenu) ShowActiveDevice();
        else if (menu == _graphicsMenu) ShowChosenGraphics();

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
        if (button != null) button.GetComponentInChildren<Image>().color = _selectedColor;

        // Play the click sound effect and prevent OnPointerExit from playing the sound
        if (_clickTimer > 0.2f && button != null) 
        {
            _audioSource.PlayOneShot(_clickClip, 0.5f);
            _clickTimer = 0f;
        }
    }

    private void ChangeHoverButtonColor(bool isPointing)
    {
        if (isPointing)
        {
            if (_clickTimer > 0.2f)
            {
                _audioSource.PlayOneShot(_clickClip, 0.5f);
                _clickTimer = 0f;
            }
            _selectedButton.GetComponentInChildren<Image>().color = _selectedColor;
        }
        else
        {
            if (_selectedButton != null) 
            {
                foreach (Button b in _buttons)
                {
                    b.GetComponentInChildren<Image>().color = _unselectedColor;
                }
                _selectedButton = null;
            }
        }
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




    #region Options

    private void Graphics()
    {
        switch (_selectedButton.gameObject.tag)
        {
            case "LowQ":
                OnLowQualitySelected();
                break;

            case "MediumQ":
                OnMediumQualitySelected();
                break;

            case "HighQ":
                OnHighQualitySelected();
                break;

            default:
                break;
        }

        ShowChosenGraphics();
    }

    private void Devices()
    {
        switch (_selectedButton.gameObject.tag)
        {
            case "Controller":
                OnControllerSelected();
                if (GameManager.Instance.PlayerController != null) GameManager.Instance.PlayerController.SetInputDevice(1);
                break;

            case "Mouse":
                OnMouseSelected();
                if (GameManager.Instance.PlayerController != null) GameManager.Instance.PlayerController.SetInputDevice(0);
                break;

            case "Sensitivity":
                Debug.Log("Sensitivity");
                StartCoroutine(AdjustSensitivity());
                break;

            default:
                Debug.Log("Default");
                break;
        }

        ShowActiveDevice();
    }

    private void ShowActiveDevice()
    {
        int deviceChoice = PlayerPrefs.GetInt("InputDevice", 0);

        // Find the devices and change their checkmarks
        _mouseCheckbox.SetActive(deviceChoice == 0 ? true : false);
        _controllerCheckbox.SetActive(deviceChoice == 1 ? true : false);
    }

    private void ShowChosenGraphics()
    {
        int graphicsChoice = PlayerPrefs.GetInt("GraphicsQuality", 1);

        // Find the graphics buttons and change their checkmarks
        GameObject.FindWithTag("LowQ").transform.GetChild(1).GetChild(0).gameObject.SetActive(graphicsChoice == 0 ? true : false);
        GameObject.FindWithTag("MediumQ").transform.GetChild(1).GetChild(0).gameObject.SetActive(graphicsChoice == 1 ? true : false);
        GameObject.FindWithTag("HighQ").transform.GetChild(1).GetChild(0).gameObject.SetActive(graphicsChoice == 2 ? true : false);  
    }

    private void OnControllerSelected()
    {
        PlayerPrefs.SetInt("InputDevice", 1);
        InputSystem.EnableDevice(Gamepad.current);
        InputSystem.DisableDevice(Mouse.current);
    }

    private void OnMouseSelected()
    {
        PlayerPrefs.SetInt("InputDevice", 0);
        InputSystem.EnableDevice(Mouse.current);
        InputSystem.DisableDevice(Gamepad.current);
    }

    private void OnLowQualitySelected()
    {
        PlayerPrefs.SetInt("GraphicsQuality", 0);
        QualitySettings.SetQualityLevel(0);
    }

    private void OnMediumQualitySelected()
    {
        PlayerPrefs.SetInt("GraphicsQuality", 1);
        QualitySettings.SetQualityLevel(1);
    }

    private void OnHighQualitySelected()
    {
        PlayerPrefs.SetInt("GraphicsQuality", 2);
        QualitySettings.SetQualityLevel(2);
    }

    private IEnumerator AdjustSensitivity()
    {
        _isAdjustingSensitivity = true;
        int menuID = HideMenu();
        _sensitivityDisplay.SetActive(true);
        yield return new WaitUntil(() => !_isAdjustingSensitivity);
        _menus[menuID].gameObject.SetActive(true);
        _sensitivityDisplay.SetActive(false);
    }

    private IEnumerator StopAdjustingSensitivity()
    {
        _sensitivityText.text = SensitivityValue.ToString();
        PlayerPrefs.SetFloat("Sensitivity", SensitivityValue);
        if (GameManager.Instance.PlayerController != null) GameManager.Instance.PlayerController.SetInputDevice(PlayerPrefs.GetInt("InputDevice", 0));
        yield return new WaitForSeconds(0.2f);
        _isAdjustingSensitivity = false;
        yield return null;
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
            int menuID = HideMenu();
            MessageDisplay("Unsaved progress will be lost.\n\nContinue?");
            yield return new WaitUntil(() => !_isDeciding);
            _menus[menuID].gameObject.SetActive(true);
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
            int menuID = HideMenu();
            MessageDisplay("Overwrite file?");
            yield return new WaitUntil(() => !_isDeciding);
            _menus[menuID].gameObject.SetActive(true);
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
        _messageDisplay.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = message;
        _decisionButtons = _messageDisplay.GetComponentsInChildren<Button>(true);
        _messageDisplay.gameObject.SetActive(true);
    }

    #endregion




    #region Start, Return and Exit Method

    // Method to return to the main menu
    void Return()
    {
        // Check if the player is in the main menu
        bool isMainMenu = SceneManager.GetActiveScene().name == "MainMenu";

        switch (_selectedButton.gameObject.tag)
        {
            case "LoadReturnButton":
                foreach (Transform file in _loadFileContainer)
                {
                    Destroy(file.gameObject);
                }
                ActivateMenu(_mainMenu, isMainMenu);
                break;

            case "SaveReturnButton":
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
                break;

            case "ReturnGame":
                Back();
                break;

            case "ReturnButton":
                if (_optionsMenu.gameObject.activeSelf)
                {
                    _optionsMenu.gameObject.SetActive(false);
                    ActivateMenu(_mainMenu, isMainMenu);
                }
                else if (_deviceMenu.gameObject.activeSelf)
                {
                    _deviceMenu.gameObject.SetActive(false);
                    ActivateMenu(_optionsMenu, false);
                }
                else if (_graphicsMenu.gameObject.activeSelf)
                {
                    _graphicsMenu.gameObject.SetActive(false);
                    ActivateMenu(_optionsMenu, false);
                }
                break;

            default:
                break;
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
            int menuID = HideMenu();
            MessageDisplay("Exit to Main Menu?");
            yield return new WaitUntil(() => !_isDeciding);
            _menus[menuID].gameObject.SetActive(true);
            _messageDisplay.gameObject.SetActive(false);

            // If the player accepts, exit to the main menu
            if (_isAccepting)
            {
                _isAccepting = false;
                GameManager.Instance.RemoveAllCustomUpdatables();
                GameManager.Instance.CustomUpdateManager.AddCustomUpdatable(this);
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

    private int HideMenu()
    {
        int counter = 0;
        foreach (Transform menu in _menus)
        {
            if (menu.gameObject.activeSelf)
            {
                menu.gameObject.SetActive(false);
                break;
            }
            menu.gameObject.SetActive(false);
            counter++;
        }
        return counter; 
    }

    // Coroutine to start a new game
    private IEnumerator StartNewGame()
    {
        _isLoading = true;
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
        _isLoading = false;

        yield return new WaitForSeconds(1f);

        // Deactivate the menu audio
        StartCoroutine(DeactiveMenuAudio());
    }

    #endregion
}