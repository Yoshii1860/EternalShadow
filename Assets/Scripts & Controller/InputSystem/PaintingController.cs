using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class PaintingController : MonoBehaviour, ICustomUpdatable
{
    #region Fields

    [Header("Menu Controller Settings")]
    [SerializeField] private float _moveDebounceTime = 0.3f;

    // Input flags
    private bool _isInteracting, _isGoingBack;
    private Vector2 _movePos;

    // Reference to PaintingEvent component
    PaintingEvent _paintingEvent;

    #endregion




    #region Unity Lifecycle Methods

    void Start()
    {
        // Get reference to PaintingEvent component
        _paintingEvent = GetComponent<PaintingEvent>();
    }

    #endregion




    #region Custom Update

    public void CustomUpdate(float deltaTime)
    {
        // Process inputs when in Painting sub-game state
        if (GameManager.Instance.CurrentSubGameState == GameManager.SubGameState.PAINT && _paintingEvent.IsPaused == false)
        {
            if (_isInteracting) Interact();
            if (_isGoingBack) Back();
        }
    }

    #endregion




    #region Input Handling

    // Callback for interact input
    public void OnInteract(InputAction.CallbackContext context)
    {
        Debug.Log("Painting: OnInteract");
        _isInteracting = context.ReadValueAsButton();
    }

    // Callback for move input
    public void OnMove(InputAction.CallbackContext context)
    {
        _movePos = context.ReadValue<Vector2>();
        
        // Debounce the move input
        float deltaTime = Time.deltaTime;
        if (Time.time - _moveDebounceTime > deltaTime && _paintingEvent.IsPaused == false)
        {
            _moveDebounceTime = Time.time;
            Move();
        }
    }

    // Callback for back input
    public void OnBack(InputAction.CallbackContext context)
    {
        _isGoingBack = context.ReadValueAsButton();
    }

    #endregion




    #region Menu Interaction Methods

    // Interact with the selected painting
    void Interact()
    {
        Debug.Log("Painting: Interact Method");
        _isInteracting = false;
        _paintingEvent.MarkPainting();
    }

    // Move between paintings based on input
    void Move()
    {
        if (_movePos.x > 0.5f)
        {
            if (_paintingEvent.PaintingSelector < 4)
            {
                AudioManager.Instance.PlayClipOneShot(gameObject.GetInstanceID(), "pen click", 1f, 1f);
                _paintingEvent.PaintingSelector++;
            }
            else return;
        }
        else if (_movePos.x < -0.5f)
        {
            if (_paintingEvent.PaintingSelector > 0)
            {
                AudioManager.Instance.PlayClipOneShot(gameObject.GetInstanceID(), "pen click", 1f, 1f);
                _paintingEvent.PaintingSelector--;
            }
            else return;
        }
    }

    // Return to the main game state
    void Back()
    {
        _isGoingBack = false;

        _paintingEvent.OnExit();
        GameManager.Instance.ResumeGame();
    }

    #endregion
}