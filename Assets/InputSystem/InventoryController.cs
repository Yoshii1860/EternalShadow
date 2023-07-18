using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using Cinemachine;
using UnityEngine.UI;

public class InventoryController : MonoBehaviour
{
    public float moveDebounceTime = 0.3f;

    bool interact, exit;
    Vector2 move;

    void Update()
    {
        if (GameManager.Instance.CurrentGameState == GameManager.GameState.Inventory)
        {
            if (interact) Interact();
            if (exit) Exit();
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        interact = context.ReadValueAsButton();
    }

    public void OnExit(InputAction.CallbackContext context)
    {
        exit = context.ReadValueAsButton();
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

    void Interact()
    {
        interact = false;
        Debug.Log("Interact");

        InventoryManager.Instance.ShowItemDisplay();
    }

    void Exit()
    {
        exit = false;
        Debug.Log("Exit");

        GameManager.Instance.ResumeGame();
    }

    void Move()
    {
        Debug.Log(InventoryManager.Instance.selectedItemNumber);
        if (move.y > 0.5f)
        {
            Debug.Log("Up");
            if (InventoryManager.Instance.selectedItemNumber - 3 >= 0)
            {
                InventoryManager.Instance.selectedItemNumber -= 3;
                InventoryManager.Instance.ChangeSelectedItem();
            }
        }
        else if (move.y < -0.5f)
        {
            Debug.Log("Down");
            if(InventoryManager.Instance.selectedItemNumber + 3 < InventoryManager.Instance.Items.Count)
            {
                InventoryManager.Instance.selectedItemNumber += 3;
                InventoryManager.Instance.ChangeSelectedItem();
            }
        }
        else if (move.x > 0.5f)
        {
            Debug.Log("Right");
            if (InventoryManager.Instance.selectedItemNumber + 1 < InventoryManager.Instance.Items.Count)
            {
                InventoryManager.Instance.selectedItemNumber += 1;
                InventoryManager.Instance.ChangeSelectedItem();
            }
        }
        else if (move.x < -0.5f)
        {
            Debug.Log("Left");
            if (InventoryManager.Instance.selectedItemNumber - 1 >= 0)
            {
                InventoryManager.Instance.selectedItemNumber -= 1;
                InventoryManager.Instance.ChangeSelectedItem();
            }
        }
        Debug.Log(InventoryManager.Instance.selectedItemNumber);
    }
}