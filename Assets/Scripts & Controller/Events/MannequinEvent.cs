using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MannequinEvent : MonoBehaviour
{
    #region Fields

    [Tooltip("The first mannequin in the event.")]
    [SerializeField] private Mannequin _firstMannequin;
    [Tooltip("The second mannequin in the event.")]
    [SerializeField] private Mannequin _secondMannequin;
    [Tooltip("The _weapon required to pick up first to trigger the event.")]
    [SerializeField] private Weapon _weapon;

    private bool _hasEntered = false;
    private bool _playOnce = false;

    #endregion




    #region Collider Trigger

    private void OnTriggerExit(Collider other) 
    {
        if (other.tag == "Player")
        {
            if (!_hasEntered)
            {
                // Start mannequins head movement
                _hasEntered = true;
                _firstMannequin.HasEventStarted = true;
                _secondMannequin.HasEventStarted = true;
            }
            else
            {
                if (_weapon.IsAvailable)
                {
                    // Set the event to active
                    GameManager.Instance.EventData.SetEvent("Mannequin");

                    // Start the mannequins movement
                    _firstMannequin.IsMoving = true;
                    _secondMannequin.IsMoving = true;

                    if (!_playOnce) 
                    {
                        AudioManager.Instance.PlayClipOneShotWithDelay(AudioManager.Instance.PlayerSpeaker2, "speaker mannequins", 1.5f);
                        _playOnce = true;
                    }

                    Destroy(gameObject);
                }
                else Debug.Log("You need to pick up the weapon first.");
            }
        }
    }

    #endregion




    #region Public Methods

    // Load the event from the save file
    public void EventLoad()
    {
        if (_firstMannequin.gameObject.activeSelf)
        {
            _firstMannequin.HasEventStarted = true;
            _firstMannequin.IsMoving = true;
        }

        if (_secondMannequin.gameObject.activeSelf)
        {
            _secondMannequin.HasEventStarted = true;
            _secondMannequin.IsMoving = true;
        }

        _playOnce = true;
    }

    #endregion
}
