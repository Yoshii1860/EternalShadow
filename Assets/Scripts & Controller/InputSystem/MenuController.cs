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
    [SerializeField] Transform saveFileContainer;
    [SerializeField] Transform loadFileContainer;
    [SerializeField] Button returnFromLoadButton;
    [SerializeField] Button returnFromSaveButton;
    [SerializeField] int maxFiles = 8;

    Dictionary<Transform, Vector3> initialButtonPositions = new Dictionary<Transform, Vector3>();
    Transform[] menus;
    Button[] buttons;
    Button selectedButton;
    int buttonNumber;
    public bool canCloseMenu = true;
    public float customTimer = 0f;

    [Header("Colors")]
    [SerializeField] Color selectedColor;
    [SerializeField] Color unselectedColor;

    #endregion

    #region Menu Controller Variables

    [Header("Menu Controller Settings")]
    public float moveDebounceTime = 0.3f;

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

        menus = new Transform[] { mainMenu, loadMenu, saveMenu };
        initialButtonPositions[newGameButton.transform] = newGameButton.transform.position;
        initialButtonPositions[loadGameButton.transform] = loadGameButton.transform.position;
        initialButtonPositions[exitButton.transform] = exitButton.transform.position;
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
            Move();
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
        AudioManager.Instance.SetAudioClip(AudioManager.Instance.playerSpeaker, "woosh");
        AudioManager.Instance.PlayAudio(AudioManager.Instance.playerSpeaker, 0.8f, 1f, false);
        if (selectedButton.gameObject.CompareTag("NewGameButton"))              StartCoroutine(StartNewGame());
        else if (selectedButton.gameObject.CompareTag("LoadGameButton"))        ActivateMenu(loadMenu);
        else if (selectedButton.gameObject.CompareTag("LoadReturnButton"))      Return(returnFromLoadButton);
        else if (selectedButton.gameObject.CompareTag("SaveReturnButton"))      Return(returnFromSaveButton);
        else if (selectedButton.gameObject.CompareTag("LoadFileButton"))        LoadFile();
        else if (selectedButton.gameObject.CompareTag("SaveFileButton"))        SaveFile();
        else if (selectedButton.gameObject.CompareTag("ExitButton"))            ExitGame();
    }

    void Move()
    {
        if (selectedButton == null) 
        {
            selectedButton = buttons[0];
            buttonNumber = 0;
            ChangeSelectedButtonColor(selectedButton);
            return;
        }

        if (move.y > 0.5f)
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
        else if (move.y < -0.5f)
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
        else if (move.x > 0.5f)
        {
            if (selectedButton != returnFromLoadButton || selectedButton != returnFromSaveButton)
            {
                if (saveMenu.gameObject.activeSelf) selectedButton = returnFromSaveButton;
                else if (loadMenu.gameObject.activeSelf) selectedButton = returnFromLoadButton;
                buttonNumber = 0;
                ChangeSelectedButtonColor(selectedButton);
            }
        }
        else if (move.x < -0.5f)
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

        if (mainMenu.gameObject.activeSelf && canCloseMenu)         
        {
            ActivateMenu(mainMenu, false);
            GameManager.Instance.ResumeGame();
            menuCanvas.SetActive(false);
        }
        else if (loadMenu.gameObject.activeSelf)    ActivateMenu(mainMenu, mainMenuActive);
        else if (saveMenu.gameObject.activeSelf)    ActivateMenu(mainMenu, mainMenuActive);
    }

    #endregion

    #region Menu Activation and Configuration

    void ActivateMenu(Transform menu, bool mainMenuState = false)
    {
        newGameButton.gameObject.SetActive(true);
        foreach (Transform m in menus)
        {
            if (m == menu)  
            {
                m.gameObject.SetActive(true);
                buttons = m.GetComponentsInChildren<Button>(); // Initialize buttons array here
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
        foreach (Button b in buttons)
        {
            b.GetComponentInChildren<Image>().color = unselectedColor;
        }
        returnFromLoadButton.GetComponentInChildren<Image>().color = unselectedColor;
        returnFromSaveButton.GetComponentInChildren<Image>().color = unselectedColor;
        button.GetComponentInChildren<Image>().color = selectedColor;
        AudioManager.Instance.PlaySoundOneShot(AudioManager.Instance.playerSpeaker, "click", 0.8f);
    }

    IEnumerator StartNewGame()
    {
        yield return new WaitUntil(() => !AudioManager.Instance.IsPlaying(AudioManager.Instance.playerSpeaker));
        AudioManager.Instance.SetAudioClip(AudioManager.Instance.playerSpeaker, "menu die");
        AudioManager.Instance.PlayAudio(AudioManager.Instance.playerSpeaker, 0.8f, 1f, false);
        yield return new WaitUntil(() => !AudioManager.Instance.IsPlaying(AudioManager.Instance.playerSpeaker));
        GameManager.Instance.LoadNewGame();
        ActivateMenu(mainMenu, false);
        menuCanvas.SetActive(false);
    }

    #endregion

    #region Save and Load Methods

    public void SaveGame()
    {
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

    void LoadFile()
    {
        string filename = selectedButton.GetComponentInChildren<TextMeshProUGUI>().text;
        foreach (Transform file in loadFileContainer)
        {
            Destroy(file.gameObject);
        }
        ActivateMenu(mainMenu, false);
        menuCanvas.SetActive(false);
        GameManager.Instance.LoadNewScene(filename);
    }

    void SaveFile()
    {
        // Get the file name from the button text
        string filename = selectedButton.GetComponentInChildren<TextMeshProUGUI>().text;
        if (filename != " ")
        {
            // Delete File
            File.Delete(Path.Combine(Application.persistentDataPath, filename + ".shadow"));
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
        ActivateMenu(mainMenu, false);
        menuCanvas.SetActive(false);
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
            GameManager.Instance.ResumeGame();
            menuCanvas.SetActive(false);
        }
    }

    void ExitGame()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            Application.Quit();
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
            ActivateMenu(mainMenu, true);
        }
    }

    #endregion
}