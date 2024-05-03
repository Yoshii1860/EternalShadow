using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(UniqueIDComponent))]
public class Door : MonoBehaviour
{
    #region Variables

    public bool IsLocked;
    public bool IsOpen;
    public float DoorOpenAngle = 90.0f;
    public string DisplayMessage = "";
    [SerializeField] private bool _openOnStart;
    [SerializeField] private string _keyDisplayName;
    [SerializeField] private float _smooth = 2.0f;

    private Vector3 defaultRot;
    private Vector3 openRot;

    #endregion
    



    #region Unity Methods

    private void Start()
    {
        defaultRot = transform.eulerAngles;
        openRot = new Vector3(defaultRot.x, defaultRot.y + DoorOpenAngle, defaultRot.z);
        if (_openOnStart) 
        {
            transform.eulerAngles = openRot;
            IsOpen = true;
        }
        else IsOpen = false;
    }

    #endregion




    #region Public Methods

    public void CloseDoor(bool withAudio = true)
    {
        if (IsOpen)
        {
            StartCoroutine(AnimateDoor(IsOpen, withAudio));
            IsOpen = !IsOpen;
        }
    }

    public void OpenDoor(bool withAudio = true)
    {
        if (!IsOpen)
        {
            StartCoroutine(AnimateDoor(IsOpen, withAudio));
            IsOpen = !IsOpen;
        }
    }

    public void Interact()
    {
        Item key = InventoryManager.Instance.FindItem(_keyDisplayName);
        if (IsLocked && key == null)
        {
            // play locked sound
            Debug.Log("Door is locked");
            if (DisplayMessage == "") DisplayMessage = "The door is locked.";
            GameManager.Instance.DisplayMessage(DisplayMessage, 2f);
            if (AudioManager.Instance.IsPlaying(transform.gameObject.GetInstanceID()))
            {
                return;
            }
            else
            {
                AudioManager.Instance.PlayClipOneShot(transform.gameObject.GetInstanceID(), "locked door", .5f);
            } 
        }
        else if (IsLocked && key != null)
        {
            // play unlock sound
            Debug.Log("Door is unlocked");
            if (AudioManager.Instance.IsPlaying(transform.gameObject.GetInstanceID()))
            {
                return;
            }
            else
            {
                AudioManager.Instance.PlayClipOneShot(transform.gameObject.GetInstanceID(), "unlock door", 1f, 1.3f);
                InventoryManager.Instance.RemoveItem(key);
                IsLocked = false;
            }
        }
        else
        {
            // play open sound
            Debug.Log("Door.Interact");
            if (AudioManager.Instance.IsPlaying(transform.gameObject.GetInstanceID()))
            {
                return;
            }
            else
            {
                StartCoroutine(AnimateDoor(IsOpen));
                IsOpen = !IsOpen;   
            }
        }
    }

    public bool DoorState()
    {
        return IsOpen;
    }

    #endregion




    #region Coroutines

    private IEnumerator AnimateDoor(bool isOpen, bool withAudio = true)
    {
        if (isOpen)
        {
            if (withAudio) AudioManager.Instance.PlayClipOneShot(transform.gameObject.GetInstanceID(), "close door", .5f);
        }
        else
        {
            if (withAudio) AudioManager.Instance.PlayClipOneShot(transform.gameObject.GetInstanceID(), "open door", .5f);
            GetComponent<Collider>().enabled = false;
        }

        Vector3 targetRot = isOpen ? defaultRot : openRot;
        Vector3 currentRot = transform.eulerAngles;
        float timeElapsed = 0f;

        while (timeElapsed < _smooth)
        {
            transform.eulerAngles = Vector3.Lerp(currentRot, targetRot, timeElapsed / _smooth);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        GetComponent<Collider>().enabled = true;
        Debug.Log("Door is open");

        transform.eulerAngles = targetRot;
    }

    #endregion
}
