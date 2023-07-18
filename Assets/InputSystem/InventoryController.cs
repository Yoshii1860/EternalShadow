using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using Cinemachine;
using UnityEngine.UI;

public class InventoryController : MonoBehaviour
{
    public float moveDebounceTime = 0.3f;
    public ItemActions itemActions;

    bool interact, exit, back;
    Vector2 move;

    void Update()
    {
        if (GameManager.Instance.CurrentGameState == GameManager.GameState.Inventory)
        {
            if (interact) Interact();
            if (exit) Exit();
            if (back) Back();
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

    public void OnBack(InputAction.CallbackContext context)
    {
        back = context.ReadValueAsButton();
    }

    void Interact()
    {
        interact = false;
        Debug.Log("InventoryController: Interact");

        if (!InventoryManager.Instance.itemActionsOpen)
        {
            InventoryManager.Instance.ShowItemDisplay();
        }
        else
        {
            if (InventoryManager.Instance.itemActionNumber ==       0)  itemActions.Use(InventoryManager.Instance.selectedItem);
            else if (InventoryManager.Instance.itemActionNumber ==  1)  itemActions.Inspect(InventoryManager.Instance.selectedItem);
            else if (InventoryManager.Instance.itemActionNumber ==   2)  itemActions.Combine(InventoryManager.Instance.selectedItem);
            else if (InventoryManager.Instance.itemActionNumber ==  3)  itemActions.ThrowAway(InventoryManager.Instance.selectedItem);

            InventoryManager.Instance.BackToSelection();
        }
    }

    void Exit()
    {
        exit = false;
        Debug.Log("InventoryController: Exit");

        GameManager.Instance.ResumeGame();
    }

    void Move()
    {
        if(!InventoryManager.Instance.itemActionsOpen)
        {
            MoveSelection();
        }
        else
        {
            MoveActions();
        }
    }

    void Back()
    {
        InventoryManager.Instance.BackToSelection();
    }

    void MoveSelection()
    {
        if (move.y > 0.5f)
        {
            Debug.Log("InventoryController: Up");
            if (InventoryManager.Instance.highlightNumber - 3 >= 0)
            {
                InventoryManager.Instance.ChangeSelectedItemColor(true, false);
                InventoryManager.Instance.highlightNumber -= 3;
                InventoryManager.Instance.ChangeSelectedItemColor(true, true);
            }
        }
        else if (move.y < -0.5f)
        {
            Debug.Log("InventoryController: Down");
            if(InventoryManager.Instance.highlightNumber + 3 < InventoryManager.Instance.Items.Count)
            {
                InventoryManager.Instance.ChangeSelectedItemColor(true, false);
                InventoryManager.Instance.highlightNumber += 3;
                InventoryManager.Instance.ChangeSelectedItemColor(true, true);
            }
        }
        else if (move.x > 0.5f)
        {
            Debug.Log("InventoryController: Right");
            if (InventoryManager.Instance.highlightNumber + 1 < InventoryManager.Instance.Items.Count)
            {
                InventoryManager.Instance.ChangeSelectedItemColor(true, false);
                InventoryManager.Instance.highlightNumber += 1;
                InventoryManager.Instance.ChangeSelectedItemColor(true, true);
            }
        }
        else if (move.x < -0.5f)
        {
            Debug.Log("InventoryController: Left");
            if (InventoryManager.Instance.highlightNumber - 1 >= 0)
            {
                InventoryManager.Instance.ChangeSelectedItemColor(true, false);
                InventoryManager.Instance.highlightNumber -= 1;
                InventoryManager.Instance.ChangeSelectedItemColor(true, true);
            }
        }
    }

    void MoveActions()
    {
        if (move.y > 0.5f)
        {
            Debug.Log("InventoryController: Actions Up");
            if (InventoryManager.Instance.itemActionNumber - 1 >= 0)
            {
                InventoryManager.Instance.ChangeSelectedActionColor(false);
                InventoryManager.Instance.itemActionNumber -= 1;
                InventoryManager.Instance.ChangeSelectedActionColor(true);
            }
        }
        else if (move.y < -0.5f)
        {
            Debug.Log("InventoryController: Actions Down");
            if(InventoryManager.Instance.itemActionNumber + 1 < InventoryManager.Instance.actionsChildCount)
            {
                InventoryManager.Instance.ChangeSelectedActionColor(false);
                InventoryManager.Instance.itemActionNumber += 1;
                InventoryManager.Instance.ChangeSelectedActionColor(true);
            }
        }
    }
}