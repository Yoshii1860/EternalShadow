using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    // Singleton pattern to ensure only one instance exists
    public static GameManager Instance { get; private set; }

    // Game state
    public enum GameState
    {
        MainMenu,
        Gameplay,
        Inventory,
        Paused,
        GameOver
    }

    public GameState CurrentGameState { get; private set; }

    [SerializeField] GameObject inventoryObject;
    [SerializeField] PlayerInput playerInput;

    public bool isPaused = false;

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

    private void SetGameState(GameState newState)
    {
        CurrentGameState = newState;

        // Handle state-specific actions
        switch (CurrentGameState)
        {
            case GameState.MainMenu:
                // Add code for main menu behavior
                break;

            case GameState.Gameplay:
                // Add code for gameplay behavior
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