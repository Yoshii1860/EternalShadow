using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummonEvent : MonoBehaviour
{
    #region References

    [Tooltip("The objects that need to be active for the summoning to happen")]
    [SerializeField] private GameObject[] _summoningObjects;
    [Tooltip("The particles that will play when the summoning happens")]
    [SerializeField] private ParticleSystem _summoningParticles;
    [Tooltip("The slender object to be disabled when the summoning happens")]
    [SerializeField] private GameObject _slender;
    [Tooltip("The girl object to be disabled when the summoning happens")]
    [SerializeField] private GameObject _girl;
    [Tooltip("The priest object to be enabled when the summoning happens")]
    [SerializeField] private GameObject _priest;
    [Tooltip("The loot object to be enabled when the summoning happens")]
    [SerializeField] private GameObject _loot;
    [Tooltip("The speaker object to play the summoning sound")]
    [SerializeField] private GameObject _speaker;
    [Tooltip("The doors to be closed when the summoning happens")]
    [SerializeField] private Door[] _doors;
    [Tooltip("The lockers to be closed when the summoning happens")]
    [SerializeField] private GameObject[] _lockers;

    #endregion




    #region Public Methods

    // Check if all the items are active - used by the summoning objects
    public void CheckItems()
    {
        foreach (GameObject summoningObject in _summoningObjects)
        {
            if (summoningObject.activeSelf == false)
            {
                return;
            }
        }

        // If all the items are active, start the summoning
        AudioManager.Instance.PlayAudio(_speaker.GetInstanceID(), 0.8f, 1f, false);
        AudioManager.Instance.FadeOutAudio(AudioManager.Instance.Environment, 3f);
        _summoningParticles.Play();

        // Close all doors and lockers
        CloseDoors();

        // Deactivate the enemies and remove them from the custom update manager
        _girl.SetActive(false);
        _slender.SetActive(false);
        GameManager.Instance.CustomUpdateManager.RemoveCustomUpdatable(_girl.GetComponent<AISensor>());
        GameManager.Instance.CustomUpdateManager.RemoveCustomUpdatable(_slender.GetComponent<AISensor>());

        // Activate the loot
        _loot.SetActive(true);

        // Start the summoning animation
        StartCoroutine(PlaySummoningAnimation());
    }

    #endregion




    #region Private Methods

    // Close all doors and lockers
    private void CloseDoors()
    {
        foreach (Door door in _doors)
        {
            door.CloseDoor();
            door.IsLocked = true;
            door.DisplayMessage = "This door cannot be opened!";
        }

        foreach (GameObject locker in _lockers)
        {
            locker.GetComponent<LockerCode>().CloseLocker();
        }
    }

    #endregion




    #region Coroutines

    // Start the summoning event
    private IEnumerator PlaySummoningAnimation()
    {
        yield return new WaitForSeconds(1f);

        // Play the summoning sound
        AudioManager.Instance.PlayClipOneShot(_priest.GetInstanceID(), "summoning", 0.7f, 1f);

        yield return new WaitForSeconds(3f);

        // Set the new environment music and play it
        AudioManager.Instance.SetAudioClip(AudioManager.Instance.Environment, "horror chase music 3");
        AudioManager.Instance.PlayAudio(AudioManager.Instance.Environment, 0.4f, 1f, true);
        
        // Play the summoning animation
        _priest.GetComponent<Animator>().SetTrigger("summon");

        yield return new WaitForSeconds(2f);

        // play a speaker sound of the player
        AudioManager.Instance.PlayClipOneShot(AudioManager.Instance.PlayerSpeaker2, "speaker summon", 1f);

        yield return new WaitForSeconds(5f);

        // Enable the priest and start the boss fight
        AudioManager.Instance.FadeInAudio(_priest.GetInstanceID(), 3f);
        _priest.GetComponent<Boss>().FollowPlayer();
        _priest.GetComponent<CapsuleCollider>().enabled = true;
        _priest.GetComponent<BoxCollider>().enabled = true; 
    }

    #endregion
}

