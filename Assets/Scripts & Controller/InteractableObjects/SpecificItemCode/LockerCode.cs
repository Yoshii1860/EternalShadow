using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockerCode : InteractableObject
{
    #region Variables

    [Space(10)]
    [Header("Components")]
    [SerializeField] private Transform _doorTransform;

    private MeshRenderer _doorMesh;
    private bool _isInsideCollider = false;
    private bool _isOpen = false;
    private bool _isLocked = false;

    #endregion




    #region Unity Methods

    protected override void Start()
    {
        _doorMesh = _doorTransform.GetComponent<MeshRenderer>();
    }

    #endregion




    #region Base Methods

    // Override the base class method for specific implementation
    protected override void RunItemCode()
    {
        if (!_isLocked) 
        {
            Debug.Log("Locker INTERACTED");
            if (_isOpen)    StartCoroutine(Close());
            else            StartCoroutine(Open());
        }
    }

    #endregion




    #region Colliders

    // When the player enters the collider and closes the door, the player is hidden
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _isInsideCollider = true;
            HighlightLocker(true);
        }
    }

    // When the player exits the collider, the player is visible again
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _isInsideCollider = false;
            ToggleEnemyVisibility(false);
            HighlightLocker(false);
        }
    }

    #endregion




    #region Private Methods

    // Close the lockers and lock them
    public void CloseLocker()
    {
        if (_isOpen)    StartCoroutine(ShutDown());
        else            _isLocked = true;
    }

    #endregion




    #region Helper Methods

    // Highlight the locker door
    private void HighlightLocker(bool highlight)
    {
        if (highlight)
        {
            _doorMesh.material.EnableKeyword("_EMISSION");
            _doorMesh.material.SetColor("_EmissionColor", new Color(0.1f, 0.1f, 0.1f));
        }
        else
        {
            _doorMesh.material.DisableKeyword("_EMISSION");
        }
    }

    // Toggle the visibility of the enemies
    void ToggleEnemyVisibility(bool hidden)
    {
        foreach (AISensor sensor in GameManager.Instance.EnemyPool.GetComponentsInChildren<AISensor>())
        {
            sensor.IsPlayerHidden = hidden;
        }
    }

    #endregion




    #region Coroutines

    // Coroutine to open the locker
    IEnumerator Open()
    {
        AudioManager.Instance.PlayClipOneShot(gameObject.GetInstanceID(), "open locker", 0.8f);
        _isLocked = true;

        // slowly _isOpen door of the locker
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime;
            _doorTransform.localEulerAngles = Vector3.Lerp(Vector3.zero, new Vector3(0, -100, 0), t);
            yield return null;
        }

        if (_isInsideCollider)
        {
            ToggleEnemyVisibility(false);
        }
        Debug.Log("Locker OPENED");
        _isLocked = false;
        _isOpen = true;
    }

    // Coroutine to close the locker
    IEnumerator Close()
    {
        AudioManager.Instance.PlayClipOneShot(gameObject.GetInstanceID(), "close locker", 0.8f);

        if (_isInsideCollider)
        {
            ToggleEnemyVisibility(true);
        }
        _isLocked = true;
        
        // close the door of the locker
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime;
            _doorTransform.localRotation = Quaternion.Lerp(_doorTransform.localRotation, Quaternion.Euler(0, 0, 0), t);
            yield return null;
        }
        Debug.Log("Locker CLOSED");
        _isLocked = false;
        _isOpen = false;
    }

    // Coroutine to close the locker and lock it
    IEnumerator ShutDown()
    {
        _isLocked = true;
        
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime;
            _doorTransform.localRotation = Quaternion.Lerp(_doorTransform.localRotation, Quaternion.Euler(0, 0, 0), t);
            yield return null;
        }
    }

    #endregion
}