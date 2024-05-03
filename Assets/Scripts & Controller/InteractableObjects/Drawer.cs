using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Drawer : InteractableObject
{
    #region Variables

    [Space(10)]
    [Header("OPEN DOOR/DRAWER")]
    [Tooltip("Is it a drawer (true) or a door (false)?")]
    public bool IsDrawer;
    [Tooltip("Is the drawer/door open?")]
    public bool IsOpen = false;

    [Tooltip("The position the drawer moves to when opened")]
    [SerializeField] private Vector3 _openPosition;
    [Tooltip("The closed position of the drawer")]
    [SerializeField] private Vector3 _closedPosition;
    [Tooltip("The rotation the door rotates to when opened (for doors only)")]
    [SerializeField] private Vector3 _openRotation;
    [Tooltip("The closed rotation of the door (for doors only)")]
    [SerializeField] private Vector3 _closedRotation;

    private bool _isMoving = false;

    #endregion




    #region Unity Methods

    // Start is called before the first frame update
    protected override void Start()
    {
        if (IsDrawer)   _closedPosition             = transform.localPosition;
        else            _closedRotation             = transform.localEulerAngles;
    }

    #endregion




    #region Base Class Methods

    protected override void RunItemCode()
    {
        if (_isMoving) return;
        StartCoroutine(LerpToOpenOrClose());
    }

    #endregion




    #region Coroutines

    // Lerp the drawer/door to the open position
    IEnumerator LerpToOpenOrClose()
    {
        _isMoving = true;

        // Play the sound of the drawer/door opening
        if (IsDrawer) AudioManager.Instance.PlayClipOneShot(gameObject.GetInstanceID(), "drawer", 1f, 1f);
        else AudioManager.Instance.PlayClipOneShot(gameObject.GetInstanceID(), "closet door", 1f, 1f);

        Vector3 targetPosition, targetRotation;
        float timeToMove = 1f;

        targetPosition = IsOpen ? _closedPosition : _openPosition;
        targetRotation = IsOpen ? _closedRotation : _openRotation;

        yield return new WaitForSeconds(0.3f);

        IsOpen = !IsOpen;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / timeToMove;

            if (IsDrawer)
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, t);
                yield return null;
            }
            else
            {
                transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(targetRotation), t);
                yield return null;
            }

            yield return null;
        }
        
        _isMoving = false;
    }

    #endregion
}
