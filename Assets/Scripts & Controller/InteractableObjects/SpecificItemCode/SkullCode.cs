using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkullCode : InteractableObject
{
    #region Variables

    [Space(10)]
    [Header("Components")]
    [SerializeField] private GameObject _bonesaw;
    [SerializeField] private GameObject _skullcap;

    [Space(10)]
    [Header("Cut Sequence")]
    [SerializeField] private Vector3 _skullcapPosition = new Vector3(0f, 0.022f, -0.007f);
    [SerializeField] private Vector3 _skullcapRotation = new Vector3(-15.45f, 16.68f, 102.13f);
    [SerializeField] private float _cutTimer = 4.5f;
    [SerializeField] private string _displayMessage;
    [SerializeField] private Transform _vCamFollowTarget;

    #endregion




    #region Unity Methods  

    protected override void Start()
    {
        _bonesaw.SetActive(false);
    }

    #endregion




    #region Base Methods

    // Override the base class method for specific implementation
    protected override void RunItemCode()
    {
        if (GameManager.Instance.EventData.CheckEvent("Skull")) return;

        // Find the bonesaw in the inventory
        Item item = InventoryManager.Instance.FindItem("Bonesaw");

        if (item != null)
        {
            // Set the event to true
            GameManager.Instance.EventData.SetEvent("Skull");
            // Remove the item from the inventory
            InventoryManager.Instance.RemoveItem(item);
            // Display the bonesaw
            _bonesaw.SetActive(true);
            // Trigger the gameplay event
            GameManager.Instance.GameplayEvent();
            GameManager.Instance.PlayerController.ToggleArms(false);
            GameManager.Instance.PlayerController.SetFollowTarget(_vCamFollowTarget);
            _bonesaw.GetComponent<Animator>().SetTrigger("Cut");
            StartCoroutine(CutSkull());
        }
        // If the player doesn't have the bonesaw, display a message
        else
        {
            GameManager.Instance.DisplayMessage(_displayMessage, 2f);
        }
    }

    #endregion




    #region Coroutines

    // Cut the skull
    IEnumerator CutSkull()
    {
        // Play the audio and align it with the animation
        AudioManager.Instance.PlayClipOneShotWithDelay(AudioManager.Instance.PlayerSpeaker2, "speaker cut head", 1f);
        yield return new WaitForSeconds(0.4f);
        AudioManager.Instance.PlayClipOneShot(gameObject.GetInstanceID(), "bonesaw cut", 0.6f, 1f);
        yield return new WaitForSeconds(_cutTimer);

        // translate skullcap to desired position and rotation over time
        float t = 0;
        Vector3 startPosition = _skullcap.transform.localPosition;
        Quaternion startRotation = _skullcap.transform.localRotation;

        while (t < 1)
        {
            t += Time.deltaTime;
            _skullcap.transform.localPosition = Vector3.Lerp(startPosition, _skullcapPosition, t);
            _skullcap.transform.localRotation = Quaternion.Lerp(startRotation, Quaternion.Euler(_skullcapRotation), t);
            yield return null;
        }

        yield return new WaitForSeconds(1f);

        // Set the bonesaw inactive and disable the collider of the skullcap
        _bonesaw.SetActive(false);
        _skullcap.transform.parent.GetComponent<Collider>().enabled = false;

        // Resume the game
        GameManager.Instance.PlayerController.ToggleArms(true);
        GameManager.Instance.PlayerController.SetFollowTarget();
        GameManager.Instance.ResumeGame();
    }

    #endregion




    #region Public Methods

    // Event load method to reset the skull state
    public void EventLoad()
    {
        _bonesaw.SetActive(false);
        _skullcap.transform.localPosition = _skullcapPosition;
        _skullcap.transform.localEulerAngles = _skullcapRotation;
        _skullcap.transform.parent.GetComponent<Collider>().enabled = false;
    }

    #endregion
}
