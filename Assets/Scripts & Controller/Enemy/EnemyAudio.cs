using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAudio : MonoBehaviour
{
    #region Fields
    
    private int _stepID;
    private float _enemyDamage;

    private bool _isFirstStep = false;
    private bool _isShouting = false;
    private bool _isSpeaking = false;
    private bool _isChasing = false;

    private float _volume = 1f;
    private string _runSoundClip;
    private string _walkSoundClip;
    private string _speakSoundClip;
    private string _shoutSoundClip;

    Transform _playerTransform;

    #endregion




    #region Unity Methods

    private void Start()
    {
        _stepID = transform.GetChild(0).gameObject.GetInstanceID();
        _enemyDamage = GetComponent<Enemy>().EnemyDamage;

        _playerTransform = GameObject.FindWithTag("Player").transform;

        // Set the audio clips based on the enemy type
        switch (GetComponent<Enemy>().EnemyType)
        {
            case EnemyType.SLENDER:
                _runSoundClip = "slender run";
                _walkSoundClip = "slender walk ";
                _speakSoundClip = "slender hello";
                _shoutSoundClip = "slender scream";
                break;
            case EnemyType.GIRL:
                _runSoundClip = "woman run";
                _walkSoundClip = "woman step ";
                _speakSoundClip = "weeping ghost woman";
                _shoutSoundClip = "woman scream";
                break;
        }   
    }

    #endregion




    #region Sound Methods

    // Play the sound based on the enemy's state
    private void SpeakSound(int isChasing)
    {
        // Check if the enemy is chasing the player [1 = true, 0 = false]
        if (isChasing == 1) _isChasing = true;
        else _isChasing = false;

        // Check if the enemy is already speaking or shouting
        if (!_isChasing && _isSpeaking) return;
        else if (_isChasing && _isShouting) return;

        StartCoroutine(SpeakerSoundChange());
    }

    // Play the step sound based on the enemy's state
    private void StepSound()
    {
        // Check if the enemy is chasing the player (set by the SpeakSound method)
        if (_isChasing)
        {
            // Play the run sound
            AudioManager.Instance.PlayClipOneShot(_stepID, _runSoundClip, _volume);
        }
        else
        {
            // Play the walk sound with alternating steps
            string clipName = _walkSoundClip + (_isFirstStep ? "1" : "2");
            AudioManager.Instance.PlayClipOneShot(_stepID, clipName, _volume);
            _isFirstStep = !_isFirstStep;
        }
    }

    // Play the sound when the enemy dies
    private void DieSound()
    {
        _isShouting = false;
        _isSpeaking = false;
        AudioManager.Instance.StopAudio(gameObject.GetInstanceID());
    }

    // Play the sound when the enemy hits the player and inflict damage
    private void HitSoundWithDamage() 
    {
        AudioManager.Instance.PlayClipOneShot(transform.gameObject.GetInstanceID(), "slender punch", 0.6f);
        GameManager.Instance.Player.TakeDamage(_enemyDamage);
    }

    #endregion




    #region Coroutines

    private IEnumerator SpeakerSoundChange()
    {
        // Stop the audio with a delay
        AudioManager.Instance.FadeOutAudio(gameObject.GetInstanceID(), 0.5f);

        yield return new WaitForSeconds(0.5f);

        // Set the audio clip based on the enemy's state
        AudioManager.Instance.SetAudioClip(gameObject.GetInstanceID(), _isChasing ? _shoutSoundClip : _speakSoundClip, _volume, 1f, true);
        AudioManager.Instance.FadeInAudio(gameObject.GetInstanceID(), 0.5f, _volume, 0f);

        // Set the enemy's state
        _isShouting = _isChasing ? true : false;
        _isSpeaking = _isChasing ? false : true;
    }

    #endregion




    #region Public Methods

    // Changing Volume based on the floor level
    public void VolumeFloorChanger()
    {
        float volume;

        volume = 1.0f - Mathf.Abs(_playerTransform.position.y - transform.position.y) / 4.0f;
        volume = Mathf.Clamp(volume, 0.25f, 1.0f);
        _volume = volume;

        AudioManager.Instance.SetVolume(transform.gameObject.GetInstanceID(), _volume);
    }

    #endregion
}
