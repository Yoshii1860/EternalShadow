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

    [Header("Menu Canvas Settings")]
    [SerializeField] GameObject loadFilePrefab;
    [SerializeField] GameObject saveFilePrefab;
    public GameObject menuCanvas;
    [SerializeField] Transform mainMenu;
    [SerializeField] Transform newGameButton;
    [SerializeField] Transform loadGameButton;
    [SerializeField] Transform exitButton;
    [SerializeField] Transform loadMenu;
    [SerializeField] Transform saveMenu;
    [SerializeField] Transform deathScreen;
    [SerializeField] Transform messageDisplay;
    [SerializeField] Transform saveFileContainer;
    [SerializeField] Transform loadFileContainer;
    [SerializeField] Button returnFromLoadButton;
    [SerializeField] Button returnFromSaveButton;
    [SerializeField] int maxFiles = 8;

    Dictionary<Transform, Vector3> initialButtonPositions = new Dictionary<Transform, Vector3>();
    Transform[] menus;
    Button[] buttons;
    Button[] decisionButtons;
    Button selectedButton;
    int buttonNumber;
    bool isDead = false;
    public bool canCloseMenu = true;
    public float customTimer = 0f;

    [Header("Colors")]
    [SerializeField] Color selectedColor;
    [SerializeField] Color unselectedColor;

    // Audio Source and Clips
    AudioSource audioSource;
    AudioClip wooshClip;
    AudioClip clickClip;
    AudioClip menuDieClip;
    AudioClip menuMusic;
    AudioClip bootPC;
    AudioClip shutdownPC;

    #endregion

    #region Menu Controller Variables

    [Header("Menu Controller Settings")]
    public float moveDebounceTime = 0.3f;
    bool cantMove = false;
    bool decision = false;
    bool yes = false;

    bool interact, back;
    Vector2 move;

    #endregion

    #region Unity Callbacks

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

        menus = new Transform[] { mainMenu, loadMenu, saveMenu, deathScreen };
        initialButtonPositions[newGameButton.transform] = newGameButton.transform.position;
        initialButtonPositions[loadGameButton.transform] = loadGameButton.transform.position;
        initialButtonPositions[exitButton.transform] = exitButton.transform.position;
        audioSource = GetComponent<AudioSource>();
        wooshClip = Resources.Load<AudioClip>("SFX/woosh");
        clickClip = Resources.Load<AudioClip>("SFX/click");
        menuDieClip = Resources.Load<AudioClip>("SFX/menu die");
        menuMusic = Resources.Load<AudioClip>("SFX/menu music");
        bootPC = Resources.Load<AudioClip>("SFX/boot pc");
        shutdownPC = Resources.Load<AudioClip>("SFX/shutdown pc");
    }

    void Start()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            ActivateMenu(mainMenu, true);
        }
        else
        {
            ActivateMenu(mainMenu, false);
            menuCanvas.SetActive(false);
        }
    }

    public void CustomUpdate(float deltaTime)
    {
        if (customTimer < .5f)
        {
            customTimer += deltaTime;
        }
        else
        {
            canCloseMenu = true;
        }

        if (GameManager.Instance.CurrentGameState == GameManager.GameState.MainMenu)
        {
            if (interact) Interact();
            if (back) Back();
        }
    }

    #endregion



    #region Public Methods

    public void DeathScreen()
    {
        Debug.Log("Death Screen!!!!!");
        isDead = true;
        ActivateMenu(deathScreen, false);
    }

    public void MainMenu()
    {
        ActivateMenu(mainMenu, true);
    }

    #endregion

    

    #region Input Handling

    public void OnInteract(InputAction.CallbackContext context)
    {
        interact = context.ReadValueAsButton();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        move = context.ReadValue<Vector2>();

        // Debounce the move input
        float deltaTime = Time.deltaTime;
        if (Time.time - moveDebounceTime > deltaTime)
        {
            moveDebounceTime = Time.time;
            if (!cantMove) Move();
        }
    }

    public void OnBack(InputAction.CallbackContext context)
    {
        back = context.ReadValueAsButton();
    }

    #endregion

    #region Menu Interaction Methods

    void Interact()
    {
        interact = false;
        
        if (selectedButton == null) return;
        if (decision) 
        {
            if (selectedButton.gameObject.CompareTag("Yes"))
            {
                yes = true;
                decision = false;
                audioSource.PlayOneShot(wooshClip);
                return;
            }
            else if (selectedButton.gameObject.CompareTag("No"))
            {
                yes = false;
                decision = false;
                audioSource.PlayOneShot(clickClip);
                return;
            }
        }
        audioSource.PlayOneShot(wooshClip);
        if (string.Compare(selectedButton.gameObject.name, "NewGame") == 0)                     StartCoroutine(StartNewGame());
        else if (string.Compare(selectedButton.gameObject.name, "LoadGame") == 0)               ActivateMenu(loadMenu);
        else if (string.Compare(selectedButton.gameObject.name, "Return") == 0 
                && loadMenu.gameObject.activeSelf)                                              Return(returnFromLoadButton);
        else if (string.Compare(selectedButton.gameObject.name, "Return") == 0 
                && saveMenu.gameObject.activeSelf)                                              Return(returnFromSaveButton);
        else if (selectedButton.gameObject.CompareTag("LoadFileButton"))                        StartCoroutine(LoadFile());
        else if (selectedButton.gameObject.CompareTag("SaveFileButton"))                        StartCoroutine(SaveFile());
        else if (string.Compare(selectedButton.gameObject.name, "ExitGame") == 0)               StartCoroutine(ExitGame());
        else if (string.Compare(selectedButton.gameObject.name, "Retry") == 0)                  LoadAuotsave();
    }

    void Move()
    {
        if (decision)
        {
            if (move.x > 0.5f)
            {
                selectedButton = decisionButtons[1];
                ChangeSelectedButtonColor(selectedButton);
            }
            else if (move.x < -0.5f)
            {
                selectedButton = decisionButtons[0];
                ChangeSelectedButtonColor(selectedButton);
            }
            return;
        }

        if (selectedButton == null) 
        {
            if (buttons.Length == 0)
            {
                if (selectedButton != returnFromLoadButton || selectedButton != returnFromSaveButton)
                {
                    if (saveMenu.gameObject.activeSelf) selectedButton = returnFromSaveButton;
                    else if (loadMenu.gameObject.activeSelf) selectedButton = returnFromLoadButton;
                    buttonNumber = 0;
                    ChangeSelectedButtonColor(selectedButton);
                    return;
                }
            }
            selectedButton = buttons[0];
            buttonNumber = 0;
            ChangeSelectedButtonColor(selectedButton);
            return;
        }

        if (move.y > 0.5f && buttons.Length > 0)
        {
            if (buttonNumber - 1 >= 0)
            {
                buttonNumber--;
                selectedButton = buttons[buttonNumber];
                ChangeSelectedButtonColor(selectedButton);
            }
            else
            {
                buttonNumber = buttons.Length - 1;
                selectedButton = buttons[buttonNumber];
                ChangeSelectedButtonColor(selectedButton);
            }
        }
        else if (move.y < -0.5f && buttons.Length > 0)
        {
            if (buttonNumber + 1 < buttons.Length)
            {
                buttonNumber++;
                selectedButton = buttons[buttonNumber];
                ChangeSelectedButtonColor(selectedButton);
            }
            else
            {
                buttonNumber = 0;
                selectedButton = buttons[buttonNumber];
                ChangeSelectedButtonColor(selectedButton);
            }
        }
        else if (move.x > 0.5f && buttons.Length > 0)
        {
            if (selectedButton != returnFromLoadButton || selectedButton != returnFromSaveButton)
            {
                if (saveMenu.gameObject.activeSelf) selectedButton = returnFromSaveButton;
                else if (loadMenu.gameObject.activeSelf) selectedButton = returnFromLoadButton;
                buttonNumber = 0;
                ChangeSelectedButtonColor(selectedButton);
            }
        }
        else if (move.x < -0.5f && buttons.Length > 0)
        {
            if (selectedButton == returnFromLoadButton || selectedButton == returnFromSaveButton)
            {
                selectedButton = buttons[0];
                buttonNumber = 0;
                ChangeSelectedButtonColor(selectedButton);
            }
        }
        else return;
    }

    void Back()
    {
        back = false;
        if (!canCloseMenu) return;

        bool mainMenuActive = SceneManager.GetActiveScene().name == "MainMenu";

        if (mainMenuActive && mainMenu.gameObject.activeSelf) return;

        if (mainMenu.gameObject.activeSelf && canCloseMenu && !isDead)         
        {
            ActivateMenu(mainMenu, false);
            GameManager.Instance.ResumeGame();
            menuCanvas.SetActive(false);
            audioSource.PlayOneShot(clickClip, 0.8f);
        }
        else if (loadMenu.gameObject.activeSelf && !isDead)     ActivateMenu(mainMenu, mainMenuActive);
        else if (loadMenu.gameObject.activeSelf && isDead)      ActivateMenu(deathScreen, mainMenuActive);
        else if (saveMenu.gameObject.activeSelf && !isDead)     ActivateMenu(mainMenu, mainMenuActive);
        else if (saveMenu.gameObject.activeSelf && isDead)      ActivateMenu(deathScreen, mainMenuActive);
    }

    #endregion

    #region Menu Activation and Configuration

    void ActivateMenu(Transform menu, bool mainMenuState = false)
    {
        if (mainMenuState) 
        {
            if (!audioSource.isPlaying)
            {
                audioSource.volume = 0f;
                audioSource.clip = menuMusic;
                audioSource.loop = true;
                audioSource.Play();
                while (audioSource.volume < 1f)
                {
                    audioSource.volume += Time.deltaTime;
                }
                audioSource.volume = 1f;
            }
        }

        newGameButton.gameObject.SetActive(true);
        foreach (Transform m in menus)
        {
            if (m == menu)  
            {
                m.gameObject.SetActive(true);
                buttons = m.GetComponentsInChildren<Button>();
            }
            else     
            {       
                m.gameObject.SetActive(false);
            }
        }

        if (menu == loadMenu)       PopulateMenu(loadFileContainer, loadFilePrefab);
        else if (menu == saveMenu)  PopulateMenu(saveFileContainer, saveFilePrefab);
        else
        {
            OnMainMenu(mainMenuState);
        }
    }

    public void OnMainMenu(bool mainMenuState)
    {
        newGameButton.gameObject.SetActive(mainMenuState);

        if (mainMenuState) 
        {
            loadGameButton.transform.position = initialButtonPositions[loadGameButton.transform];
            exitButton.transform.position = initialButtonPositions[exitButton.transform];
        }
        else               
        {
            loadGameButton.transform.position = initialButtonPositions[newGameButton.transform];
            exitButton.transform.position = initialButtonPositions[loadGameButton.transform];
        }

        // Reset the buttons array and color
        ResetButtonColor();
        List <Button> buttonList = new List<Button>(buttons);
        buttonList.Remove(returnFromLoadButton);
        buttonList.Remove(returnFromSaveButton);
        if (!mainMenuState)   buttonList.Remove(newGameButton.GetComponent<Button>());
        buttons = buttonList.ToArray();
        selectedButton = null;
        buttonNumber = 0;
    }

    void ResetButtonColor()
    {
        foreach (Button b in buttons)
        {
            b.GetComponentInChildren<Image>().color = unselectedColor;
        }
        returnFromLoadButton.GetComponentInChildren<Image>().color = unselectedColor;
        returnFromSaveButton.GetComponentInChildren<Image>().color = unselectedColor;
    }

    void ChangeSelectedButtonColor(Button button)
    {
        if (decision)
        {
            foreach (Button b in decisionButtons)
            {
                b.GetComponentInChildren<Image>().color = unselectedColor;
            }
            button.GetComponentInChildren<Image>().color = selectedColor;
            audioSource.PlayOneShot(clickClip, 0.8f);
            return;
        }

        foreach (Button b in buttons)
        {
            b.GetComponentInChildren<Image>().color = unselectedColor;
        }
        returnFromLoadButton.GetComponentInChildren<Image>().color = unselectedColor;
        returnFromSaveButton.GetComponentInChildren<Image>().color = unselectedColor;
        button.GetComponentInChildren<Image>().color = selectedColor;
        audioSource.PlayOneShot(clickClip, 0.8f);
    }

    IEnumerator StartNewGame()
    {
        interact = false;
        selectedButton = null;
        cantMove = true;
        yield return new WaitForSeconds(2f);
        audioSource.PlayOneShot(menuDieClip, 0.8f);
        cantMove = false;
        GameManager.Instance.LoadNewGame();
        ActivateMenu(mainMenu, false);
        menuCanvas.SetActive(false);
        yield return new WaitForSeconds(1f);
        while (audioSource.volume > 0)
        {
            audioSource.volume -= Time.deltaTime;
            yield return null;
        }
        audioSource.Stop();
        audioSource.volume = 1f;
    }

    #endregion

    #region Save and Load Methods

    public void SaveGame()
    {
        audioSource.PlayOneShot(bootPC, 0.6f);
        ActivateMenu(saveMenu);
    }

    void PopulateMenu(Transform container, GameObject prefab)
    {
        string[] savedFiles = GetSavedFiles();

        // Sort the savedFiles array based on the date and time in filenames
        System.Array.Sort(savedFiles, new SaveFileComparer());

        int filecount = 0;
        foreach (string file in savedFiles)
        {
            GameObject fileObject = Instantiate(prefab, container);
            fileObject.GetComponentInChildren<TextMeshProUGUI>().text = file;
            filecount++;
        }

        if (prefab.CompareTag("SaveFileButton"))
        {
            for (int i = filecount; i < maxFiles; i++)
            {
                GameObject fileObject = Instantiate(prefab, container);
                fileObject.GetComponentInChildren<TextMeshProUGUI>().text = "";
            }
        }

        ResetButtonColor();
        buttons = container.GetComponentsInChildren<Button>();
        List <Button> buttonList = new List<Button>(buttons);
        buttonList.Remove(returnFromLoadButton);
        buttonList.Remove(returnFromSaveButton);
        buttons = buttonList.ToArray();
        selectedButton = null;
        buttonNumber = 0;
    }

    // Function to get the list of saved file names (excluding file extensions)
    string[] GetSavedFiles()
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

    IEnumerator LoadFile()
    {
        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            Button saveButton = selectedButton;

            ////////////////////////////
            // Decision
            ////////////////////////////
            decision = true;
            MessageDisplay("Unsaved progress will be lost. Continue?");
            yield return new WaitUntil(() => !decision);
            messageDisplay.gameObject.SetActive(false);
            selectedButton = saveButton;
            if (!yes)
            {
                yield break;
            }
            else
            {
                yes = false;
            }
            ////////////////////////////
            // Decision End
            ////////////////////////////
        }
        string filename = selectedButton.GetComponentInChildren<TextMeshProUGUI>().text;
        foreach (Transform file in loadFileContainer)
        {
            Destroy(file.gameObject);
        }
        StartCoroutine(FadeOut());
        ActivateMenu(mainMenu, false);
        menuCanvas.SetActive(false);
        GameManager.Instance.LoadNewScene(filename);
        yield return null;
    }

    IEnumerator FadeOut()
    {
        if (audioSource.isPlaying)
        {
            while (audioSource.volume > 0)
            {
                audioSource.volume -= Time.deltaTime;
                yield return null;
            }
            audioSource.Stop();
            audioSource.volume = 1f;
        }
    }

    IEnumerator SaveFile()
    {
        // Get the file name from the button text
        string filename = selectedButton.GetComponentInChildren<TextMeshProUGUI>().text;
        if (filename != "" && filename != "Asylum-autosave")
        {
            ////////////////////////////
            // Decision
            ////////////////////////////
            decision = true;
            MessageDisplay("Overwrite file?");
            yield return new WaitUntil(() => !decision);
            messageDisplay.gameObject.SetActive(false);
            if (yes)
            {
                File.Delete(Path.Combine(Application.persistentDataPath, filename + ".shadow"));
                yes = false;
            }
            else
            {
                yield break;
            }
            ////////////////////////////
            // Decision End
            ////////////////////////////
        }
        else if (filename == "Asylum-autosave")
        {
            GameManager.Instance.DisplayMessage("Can`t overwrite autosave", 2f);
            yield break;
        }
        // Get Scene Name and Date/Time
        Scene scene = SceneManager.GetActiveScene();
        filename = scene.name + " - " + System.DateTime.Now.ToString("dd-MM-yyy HH-mm-ss");
        Debug.Log("SaveObject.OnButtonClick: " + filename);
        // Delete all children of the parent canvas to prevent duplicates
        foreach (Transform file in saveFileContainer)
        {
            Destroy(file.gameObject);
        }
        // Save the game data
        GameManager.Instance.SaveData(filename);
        interact = false;
        cantMove = true;
        selectedButton = null;
        yield return new WaitForSeconds(1f);
        audioSource.PlayOneShot(shutdownPC, 0.6f);
        yield return new WaitForSeconds(1f);
        cantMove = false;
        ActivateMenu(mainMenu, false);
        menuCanvas.SetActive(false);
    }

    public void LoadAuotsave()
    {
        string[] savedFiles = GetSavedFiles();
        string autosave = "Asylum-autosave";
        foreach (string file in savedFiles)
        {
            if (file.Contains(autosave))
            {
                GameManager.Instance.LoadNewScene(file, false);
            }
        }
        deathScreen.gameObject.SetActive(false);
        ActivateMenu(mainMenu, false);
        menuCanvas.SetActive(false);
    }

    private void MessageDisplay(string message)
    {
        messageDisplay.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = message;
        decisionButtons = messageDisplay.GetComponentsInChildren<Button>(true);
        messageDisplay.gameObject.SetActive(true);
    }

    #endregion

    #region Return and Exit Methods

    void Return(Button returnButton)
    {
        bool isMainMenu = SceneManager.GetActiveScene().name == "MainMenu";
        
        if (returnButton.gameObject.CompareTag("LoadReturnButton"))
        {
            foreach (Transform file in loadFileContainer)
            {
                Destroy(file.gameObject);
            }
            ActivateMenu(mainMenu, isMainMenu);
        }
        else if (returnButton.gameObject.CompareTag("SaveReturnButton"))
        {
            foreach (Transform file in saveFileContainer)
            {
                Destroy(file.gameObject);
            }
            ActivateMenu(mainMenu, false);
            audioSource.PlayOneShot(clickClip, 0.8f);
            GameManager.Instance.ResumeGame();
            menuCanvas.SetActive(false);
        }
    }

    IEnumerator ExitGame()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            Application.Quit();
        }
        else
        {
            ////////////////////////////
            // Decision
            ////////////////////////////
            decision = true;
            MessageDisplay("Exit to Main Menu?");
            yield return new WaitUntil(() => !decision);
            messageDisplay.gameObject.SetActive(false);

            if (yes)
            {
                yes = false;
                GameManager.Instance.RemoveEnemyCustomUpdatables();
                SceneManager.LoadScene("MainMenu");
                ActivateMenu(mainMenu, true);
                audioSource.Play();
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

    #endregion
}