using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrowbarCode : MonoBehaviour
{
    #region Fields

    [SerializeField] private Animator _firstRailing;
    [SerializeField] private Animator _secondRailing;
    [SerializeField] private MirrorCode _mirrorCode;

    #endregion




    #region Animator Triggers

    // Break the first railing
    public void BreakFirst()
    {
        _firstRailing.SetTrigger("Fall");
        AudioManager.Instance.PlayClipOneShot(gameObject.GetInstanceID(), "rails removal 2", 0.6f, 1f);
    }

    // Break the second railing
    public void BreakSecond()
    {
        _secondRailing.SetTrigger("Fall");
        AudioManager.Instance.PlayClipOneShot(gameObject.GetInstanceID(), "rails removal 2", 0.6f, 1f);
    }

    // Play audio when removing the railings
    public void RemovalAudio()
    {
        AudioManager.Instance.PlayClipOneShot(gameObject.GetInstanceID(), "rails removal 1", 0.6f, 1f);
    }

    // Deactivate the crowbar
    public void DeactivateCrowbar()
    {
        // Set the event to active
        GameManager.Instance.EventData.SetEvent("Rails");

        // Disable the colliders of the railings
        _firstRailing.transform.GetComponentInChildren<Collider>().enabled = false;
        _secondRailing.transform.GetComponentInChildren<Collider>().enabled = false;

        // Resume the game
        GameManager.Instance.PlayerController.ToggleArms(true);
        GameManager.Instance.PlayerController.SetFollowTarget();
        GameManager.Instance.ResumeGame();

        // Enable the mirror event and add the mirror event to the custom update manager
        _mirrorCode.enabled = true;
        GameManager.Instance.CustomUpdateManager.AddCustomUpdatable(_mirrorCode);

        // Deactivate the crowbar
        gameObject.SetActive(false);
    }

    #endregion




    #region Public Methods

    // Function to be called to load the event from the save file
    public void EventLoad()
    {
        _firstRailing.gameObject.SetActive(false);
        _secondRailing.gameObject.SetActive(false);
    }

    #endregion
}
