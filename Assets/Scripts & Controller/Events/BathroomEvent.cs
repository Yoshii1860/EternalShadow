using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BathroomEvent : MonoBehaviour
{
    #region Fields

    [SerializeField] private Animator _girl;
    private bool _playOnce = false;

    #endregion




    #region Collider Trigger

    // When the player enters the trigger, start the event
    void OnTriggerEnter(Collider other) 
    {
        if (other.CompareTag("Player"))
        {
            GetComponent<Collider>().enabled = false;
            StartCoroutine(StartEvent());
        }
    }

    #endregion




    #region Coroutines

    IEnumerator StartEvent()
    {
        if (!_girl.gameObject.activeSelf) _girl.gameObject.SetActive(true);

        // Fade out the environment audio and remove the AISensor from the custom update manager
        AudioManager.Instance.FadeOutAudio(AudioManager.Instance.Environment, 2f);
        GameManager.Instance.CustomUpdateManager.RemoveCustomUpdatable(_girl.GetComponent<AISensor>());

        // Trigger animation and play audio
        _girl.SetTrigger("GetOut");

        if (!_playOnce)
        {
            AudioManager.Instance.PlayClipOneShotWithDelay(AudioManager.Instance.PlayerSpeaker2, "speaker girl", 1.5f);
            _playOnce = true;
        }

        // Check event
        GameManager.Instance.EventData.CheckEvent("Bathroom");

        yield return new WaitForSeconds(3f);

        // Play chase music
        AudioManager.Instance.SetAudioClip(AudioManager.Instance.Environment, "horror chase music 2");
        AudioManager.Instance.PlayAudio(AudioManager.Instance.Environment, 0.30f, 1f, true);

        yield return new WaitForSeconds(4f);

        // Activate the girl
        StartGirl();
    }

    #endregion




    #region Private Methods

    // Start the girl, add the AISensor to the custom update manager and enable the EnemyBT script
    private void StartGirl()
    {
        GameManager.Instance.CustomUpdateManager.AddCustomUpdatable(_girl.GetComponent<AISensor>());
        _girl.GetComponent<EnemyBT>().enabled = true;
        _girl.GetComponentInChildren<Collider>().enabled = true;
    }

    #endregion




    #region Public Methods

    // Function to be called to load the event from the save file
    public void EventLoad()
    {
        _girl.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        _girl.GetComponent<EnemyBT>().enabled = true;
        _girl.GetComponentInChildren<Collider>().enabled = true;
        GetComponent<Collider>().enabled = false;
        _playOnce = true;
    }

    #endregion
}

