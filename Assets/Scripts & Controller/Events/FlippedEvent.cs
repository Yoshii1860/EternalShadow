using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlippedEvent : MonoBehaviour
{
    #region Fields

    [Header("Room Objects")]
    [Tooltip("The cell that is flipped")]
    [SerializeField] private GameObject _flippedCell;
    [Tooltip("The cell that is not flipped")]
    [SerializeField] private GameObject _unflippedCell;
    [Space(10)]

    [Header("Objects to activate/deactivate")]
    [Tooltip("The _mannequins that will be activated")]
    [SerializeField] private GameObject[] _mannequins;
    [Tooltip("The object that the player needs to look at to trigger the event")]
    [SerializeField] private Transform _lookAtObject;
    [Tooltip("The horror doll that will only be there when it`s flipped")]
    [SerializeField] private GameObject _horrorDoll;
    [Tooltip("The ground fog that will only be there when it`s flipped")]
    [SerializeField] private GameObject _groundFog;

    private bool _playOnce = false;

    #endregion




    #region Unity Methods

    void Start()
    {
        foreach (GameObject mannequin in _mannequins)
        {
            mannequin.SetActive(false);
        }
    }

    #endregion




    #region Collider Trigger

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            // if the player is looking at the _lookAtObject or at least less then 30 degrees away from it
            if (Vector3.Angle(other.transform.forward, _lookAtObject.position - other.transform.position) < 30)
            {
                // Set the flipped cell active and the unflipped cell inactive
                _unflippedCell.SetActive(true);
                _flippedCell.SetActive(false);
                _horrorDoll.SetActive(false);
                _groundFog.SetActive(false);
                GetComponent<BoxCollider>().enabled = false;

                // Set the event data
                GameManager.Instance.EventData.SetEvent("Flipped");

                // Activate the _mannequins and start the event
                for (int i = 0; i < _mannequins.Length; i++)
                {
                    Debug.Log("Mannequin " + i + " activated");
                    _mannequins[i].SetActive(true);
                }

                StartCoroutine(StartMannequins());
            }
            else
            {
                // Play the speaker audio just once
                if (!_playOnce) 
                {
                    AudioManager.Instance.PlayClipOneShotWithDelay(AudioManager.Instance.PlayerSpeaker2, "speaker flipped", 0.5f);
                    _playOnce = true;
                }
            }
        }
    }

    #endregion




    #region Coroutines

    // Start the mannequins with a delay
    IEnumerator StartMannequins()
    {
        yield return new WaitForSeconds(2f);

        for (int i = 0; i < _mannequins.Length; i++)
        {
            Mannequin mannequinCode = _mannequins[i].GetComponent<Mannequin>();
            mannequinCode.HasEventStarted = true;
            mannequinCode.IsMoving = true;
        }

        yield return new WaitForSeconds(1f);

        AudioManager.Instance.PlayClipOneShot(AudioManager.Instance.PlayerSpeaker2, "speaker mannequins 2");

        FinishEvent();
    }

    #endregion




    #region Private Methods

    // Finish the event by deactivating the game object
    private void FinishEvent()
    {
        gameObject.SetActive(false);
    }

    #endregion




    #region Public Methods

    // This method is called when the game is loaded
    public void EventLoad()
    {
        _unflippedCell.SetActive(true);
        _flippedCell.SetActive(false);
        _groundFog.SetActive(false);
        _playOnce = true;
        GetComponent<BoxCollider>().enabled = false;

        for (int i = 0; i < _mannequins.Length; i++)
        {
            Mannequin mannequinCode = _mannequins[i].GetComponent<Mannequin>();
            if (!mannequinCode.IsDead)
            {
                _mannequins[i].SetActive(true);
                mannequinCode.HasEventStarted = true;
                mannequinCode.IsMoving = true;
            }
        }
    }

    #endregion
}
