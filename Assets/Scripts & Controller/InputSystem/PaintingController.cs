using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class PaintingController : MonoBehaviour, ICustomUpdatable
{
    #region Fields

    [Header("Menu Controller Settings")]
    public float moveDebounceTime = 0.3f;

    // Input flags
    bool interact, back;
    Vector2 move;

    PaintingEvent paintingEvent;

    #endregion

    #region Unity Lifecycle Methods

    void Start()
    {
        // Get reference to PaintingEvent component
        paintingEvent = GetComponent<PaintingEvent>();
    }

    #endregion

    #region Input Handling

    // Callback for interact input
    public void OnInteract(InputAction.CallbackContext context)
    {
        interact = context.ReadValueAsButton();
    }

    // Callback for move input
    public void OnMove(InputAction.CallbackContext context)
    {
        move = context.ReadValue<Vector2>();
        
        // Debounce the move input
        float deltaTime = Time.deltaTime;
        if (Time.time - moveDebounceTime > deltaTime && paintingEvent.pause == false)
        {
            moveDebounceTime = Time.time;
            Move();
        }
    }

    // Callback for back input
    public void OnBack(InputAction.CallbackContext context)
    {
        back = context.ReadValueAsButton();
    }

    #endregion

    #region Custom Update

    public void CustomUpdate(float deltaTime)
    {
        // Process inputs when in Painting sub-game state
        if (GameManager.Instance.CurrentSubGameState == GameManager.SubGameState.Painting && paintingEvent.pause == false)
        {
            if (interact) Interact();
            if (back) Back();
        }
    }

    #endregion

    #region Menu Interaction Methods

    // Interact with the selected painting
    void Interact()
    {
        interact = false;
        Debug.Log("Interact with painting");
        paintingEvent.MarkPainting();
    }

    // Move between paintings based on input
    void Move()
    {
        if (move.x > 0.5f)
        {
            if (paintingEvent.paintingSelector < 4)
            {
                AudioManager.Instance.PlaySoundOneShot(gameObject.GetInstanceID(), "pen click", 1f, 1f);
                paintingEvent.paintingSelector++;
            }
            else return;
        }
        else if (move.x < -0.5f)
        {
            if (paintingEvent.paintingSelector > 0)
            {
                AudioManager.Instance.PlaySoundOneShot(gameObject.GetInstanceID(), "pen click", 1f, 1f);
                paintingEvent.paintingSelector--;
            }
            else return;
        }
    }

    // Return to the main game state
    void Back()
    {
        back = false;

        GameManager.Instance.ResumeGame();
    }

    #endregion
}