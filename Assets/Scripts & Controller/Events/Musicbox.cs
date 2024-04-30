using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Musicbox : MonoBehaviour, ICustomUpdatable
{
    #region Variables and References

    [Header("Musicbox Settings")]
    [Tooltip("The maximum distance at which the canvas becomes fully black.")]
    [SerializeField] private float _maxDistanceToBlackOut = 10f;
    [Space(10)]

    [Header("References")]
    [Tooltip("The black screen image that fades in when the player gets closer.")]
    [SerializeField] private Image _blackScreen;
    [Tooltip("The door that locks when the player enters the event.")]
    [SerializeField] private Door _door;
    [Tooltip("The text that fades in and out on the start of the event.")]
    [SerializeField] private TextMeshProUGUI _startEventText;
    [Tooltip("The game object with the audio source of the monster.")]
    [SerializeField] private GameObject _monsterAudioSourceObject;
    [Tooltip("The music boxes light object.")]
    [SerializeField] private GameObject _itemLight;
    [Tooltip("The music box item.")]
    [SerializeField] ItemController musicBoxObject;
    [Space(10)]

    [Header("Waypoints")]
    [Tooltip("The waypoints the music box moves through.")]
    [SerializeField] private Transform[] _waypoints;
    
    // The current and previous waypoint the music box is moving to
    private Transform _currentWaypoint;
    private Transform _previousWaypoint;
    // The index of the current waypoint
    private int _waypointIndex = 0;
    // The warning counter for the player
    private int _exitWarningCounter = 0;

    private int _monsterAudioID;

    private bool _isInside = false;
    private bool _hasStarted = false;
    private bool _hasExitedWaypoint = false;
    private bool _hasEnded = false;
    private bool _playOnce = false;

    #endregion




    #region Unity Functions

    private void Start()
    {
        _monsterAudioID = _monsterAudioSourceObject.GetInstanceID();
    }

    #endregion




    #region Custom Update

    public void CustomUpdate(float deltaTime)
    {
        // If the event has ended, stop the music box and remove the custom updatable
        if (GameManager.Instance.EventData.CheckEvent("Musicbox") && !_hasEnded) 
        {
            Debug.Log("Musicbox event ended due to event data");
            AudioManager.Instance.StopAudio(gameObject.GetInstanceID());
            GameManager.Instance.CustomUpdateManager.RemoveCustomUpdatable(this);
            return;
        }
        
        // If the event hasn´t started, adjust the volume of the music box and the environment
        if (!_hasStarted) 
        {
            AdjustBlackscreen();
            AdjustEnvironmentVolume();
        }

        // if there are still waypoints to go through, move the music box to the next waypoint
        if (_waypoints[_waypointIndex].childCount > 0) _hasExitedWaypoint = _waypoints[_waypointIndex].GetComponentInChildren<MusicboxWay>().Exited;

        // adjust the volume of the music box according to the player´s position
        MusicBoxVolume();

        // if the event has started, pause environment music and play a speaker sound
        if (_hasStarted && !_playOnce) 
        {
            AudioManager.Instance.PauseAudio(AudioManager.Instance.Environment);
            AudioManager.Instance.PlayClipOneShot(AudioManager.Instance.PlayerSpeaker2, "speaker musicbox");
            _playOnce = true;
        }
    }

    #endregion




    #region Colliders

    private void OnTriggerEnter(Collider other) 
    {
        if (other.CompareTag("Player"))
        {
            // if the player reaches the last waypoint, stop the event and enable the item light
            if (Vector3.Distance(transform.position, _waypoints[_waypoints.Length-1].position) < 0.1f)
            {
                if (_hasEnded) return;

                // Stop the music box and fade out the black screen
                StopAllCoroutines();
                StartCoroutine(FadeOutAudio());

                // Enable the item light and enable the flashlight
                _itemLight.SetActive(true);
                GameManager.Instance.Player.IsLightAvailable = true;

                // Set the event as ended and set the event data
                _hasEnded = true;
                GameManager.Instance.EventData.SetEvent("Musicbox");

                return;
            }

            if (_isInside) return;
            else _isInside = true;

            // if the event has not started, start it with the first waypoint and black screen
            if (!_hasStarted)
            {
                _hasStarted = true;

                // lock the door
                _door.CloseDoor();
                _door.IsLocked = true;

                // Fade to black and start the event
                StartCoroutine(FadeToBlack());
                StartCoroutine(LeavingWay());
            }
        }
    }

    #endregion




    #region Private Methods

        #region Event Scene Methods

        // Adjust the black screen according to the player´s position
        private void AdjustBlackscreen()
        {
            // If the music box is not playing, play it
            if (!AudioManager.Instance.IsPlaying(gameObject.GetInstanceID())) AudioManager.Instance.PlayAudio(gameObject.GetInstanceID(), 1f, 1f, true);

            // Calculate direction to the player
            float distance = Vector3.Distance(transform.position, GameManager.Instance.Player.transform.position);
            // If the player gets closer, fade to black
            float isFadingIn = Mathf.SmoothStep(0.99f, 0f, distance / _maxDistanceToBlackOut);
            // Clamp the isFadingIn value to the range [0, 1]
            isFadingIn = Mathf.Clamp(isFadingIn, 0f, 1f);
            // Set the isFadingIn value of the image color
            _blackScreen.color = new Color(0, 0, 0, isFadingIn);
        }

        // Adjust the volume of the environment music according to the player´s position
        private void AdjustEnvironmentVolume()
        {
            // Calculate distance between player and object
            float distance = Vector3.Distance(transform.position, GameManager.Instance.Player.transform.position);

            // Customize distance range and volume curve to your preferences
            float targetVolume = Mathf.Lerp(0.02f, 0.1f, Mathf.Clamp01(distance / 5f)); // Adjust values as needed

            // Set environment music volume
            AudioManager.Instance.SetVolumeOverTime(AudioManager.Instance.Environment, targetVolume);
        }

        #endregion




        #region Musicbox Movement Methods

        // Play the push sounds
        private void PushSounds()
        {
            // Play a random push sound with random volume
            int random = Random.Range(1, 4);
            string sound = "push " + random;
            float volume = GetComponent<AudioSource>().volume;
            AudioManager.Instance.PlayClipOneShot(gameObject.GetInstanceID(), sound, volume);

            // If the event scene is active, play the push sounds after 2 seconds
            if (GameManager.Instance.CurrentSubGameState == GameManager.SubGameState.EVENT) Invoke("PushSounds", 2f);
        }

        // Adjust the volume of the music box according to the player´s position
        private void MusicBoxVolume()
        {
            // Calculate the angle between player and music box
            Vector3 direction = transform.position - GameManager.Instance.Player.transform.position;
            float angle = Vector3.Angle(GameManager.Instance.Player.transform.forward, direction);

            float volume = 0f;

            if (angle < 10f) volume = 1f;
            else
            {
                // Inverse relationship: lower angle (facing) -> higher volume, higher angle (looking away) -> lower volume
                volume = Mathf.SmoothStep(0.7f, 0.2f, angle / 180f); // Reverse order of arguments

                // Clamp volume to your desired range (0.5f minimum, 1f maximum)
                volume = Mathf.Clamp(volume, 0.2f, 0.7f);
            }

            // Set music box volume
            AudioManager.Instance.SetVolume(gameObject.GetInstanceID(), volume);
        }

        #endregion

    #endregion




    #region Coroutines

        #region Event Scene Methods

        // Black screen fades in and player/monster sounds are played
        private IEnumerator FadeToBlack()
        {
            // Fade in the black screen
            for (float i = _blackScreen.color.a; i < 1; i += 0.01f)
            {
                _blackScreen.color = new Color(0, 0, 0, i);
                yield return new WaitForSeconds(0.01f);
            }
            _blackScreen.color = new Color(0, 0, 0, 1);

            yield return new WaitForSeconds(2f);

            // Start a gameplay event and turn off the flashlight
            GameManager.Instance.GameplayEvent();
            if (GameManager.Instance.Player.Flashlight.enabled) GameManager.Instance.Player.LightSwitch();
            GameManager.Instance.Player.IsLightAvailable = false;

            yield return new WaitForSeconds(0.5f);

            // Play the monster sound
            AudioManager.Instance.PlayClipOneShot(_monsterAudioID, "MB grunt", 0.8f, 1f);

            yield return new WaitForSeconds(0.5f);

            // Play the player sound
            AudioManager.Instance.PlayClipOneShotWithDelay(AudioManager.Instance.PlayerSpeaker2, "speaker musicbox 2", 3f);

            // Fade out the text and move the music box
            StartCoroutine(TextFade());
            StartCoroutine(MoveMusicbox());
        }

        // Black screen fades out and the door is unlocked
        IEnumerator FadeOutAudio()
        {
            // Deactivate the waypoints and the text
            foreach (Transform waypoint in _waypoints)
            {
                waypoint.gameObject.SetActive(false);
            }
            _startEventText.gameObject.SetActive(false);

            // Disable the music box object
            musicBoxObject.enabled = false;

            // Fade out the black screen
            for (float i = _blackScreen.color.a; i > 0; i -= 0.01f)
            {
                _blackScreen.color = new Color(0, 0, 0, i);
                yield return new WaitForSeconds(0.01f);
            }

            // Unlock the door and open it
            _door.IsLocked = false;
            _door.OpenDoor();

            // Stop the music box and fade out the environment music
            AudioManager.Instance.FadeOutAudio(gameObject.GetInstanceID(), 5f);

            yield return new WaitUntil(() => !AudioManager.Instance.IsPlaying(gameObject.GetInstanceID()));

            // enable the musicbox object
            musicBoxObject.enabled = true;

            // Resume the environment music
            AudioManager.Instance.UnpauseAudio(AudioManager.Instance.Environment);
            AudioManager.Instance.SetVolumeOverTime(AudioManager.Instance.Environment, 0.1f);

            // Disable this script
            this.enabled = false;
        }

        // Text fades in and out
        private IEnumerator TextFade(bool isFadingIn = true)
        {
            float scale = 0.002f;
            WaitForSeconds wait = new WaitForSeconds(0.01f);

            // slowly fade in to 1 and scale to 1.2
            if (isFadingIn)
            {
                for (float i = _startEventText.color.a; i < 1; i += 0.01f)
                {
                    _startEventText.color = new Color(_startEventText.color.r, _startEventText.color.g, _startEventText.color.b, i);
                    _startEventText.transform.localScale = new Vector3(_startEventText.transform.localScale.x + scale, _startEventText.transform.localScale.y + scale, _startEventText.transform.localScale.z + scale);
                    yield return wait;
                }
                StartCoroutine(TextFade(false));
            }
            // slowly fade out to 0 and scale to 1
            else
            {
                for (float i = _startEventText.color.r; i >= 0; i -= 0.01f)
                {
                    _startEventText.color = new Color(i, _startEventText.color.g, _startEventText.color.b, _startEventText.color.a);
                    _startEventText.transform.localScale = new Vector3(_startEventText.transform.localScale.x + scale, _startEventText.transform.localScale.y + scale, _startEventText.transform.localScale.z + scale);
                    yield return wait;
                }
            }
        }

        #endregion




    #region Musicbox Movement Methods

        // Move the music box to the next waypoint
        private IEnumerator MoveMusicbox()
        {
            Debug.Log("MoveMusicbox started");  
            WaitForSeconds wait = new WaitForSeconds(0.05f);

            // when starting the event, previousWaypoint is null, so set it to the first waypoint
            if (_previousWaypoint == null) _previousWaypoint = _waypoints[_waypointIndex]; 
            else 
            {
                _previousWaypoint.gameObject.SetActive(false);
                _previousWaypoint = _currentWaypoint;
            }

            // if waypointIndex+1 is out of bounds of waypoints length, set currentWaypoint to the last waypoint
            if (_waypointIndex == _waypoints.Length - 1) yield break;
            else _currentWaypoint = _waypoints[_waypointIndex+1];

            // activates the next waypoint
            _currentWaypoint.gameObject.SetActive(true);

            // waits until the player reaches the waypoint
            yield return new WaitUntil(() => _isInside);

            // if the player has entered the waypoint, start the gameplay event
            GameManager.Instance.GameplayEvent();

            // plays the push sounds and moves the music box to the next waypoint
            PushSounds();
            while (Vector3.Distance(transform.position, _currentWaypoint.position) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, _currentWaypoint.position, 0.1f);
                yield return wait;
            }

            // if musicbox has reached the next waypoint, resume the event scene
            GameManager.Instance.ResumeFromEventScene();
            _isInside = false;

            // preventing first waypoint to be deactivated prematurely
            if (!_waypoints[_waypointIndex+1].gameObject.activeSelf) _previousWaypoint.gameObject.SetActive(false);

            // activates the players path of the next waypoint
            _currentWaypoint.GetChild(0).gameObject.SetActive(true);
            _waypointIndex++;

            // Start the next MoveMusicbox coroutine
            StartCoroutine(MoveMusicbox());

            // if the warning counter reaches 3, play the monster sound and kill the player
            if (_exitWarningCounter == 3)
            {
                AudioManager.Instance.PlayClipOneShot(_monsterAudioID, "MB attack", 1f, 1f);
                yield return new WaitForSeconds(0.5f);
                GameManager.Instance.Player.TakeDamage(101);
            }
        }

        // If player exits the waypoint, set exited to true and play a random monster sound
        private IEnumerator LeavingWay()
        {
            yield return new WaitUntil(() => _hasExitedWaypoint);

            // Play a random monster sound
            int random = Random.Range(1, 6);
            string sound = "MB alert " + random;
            AudioManager.Instance.PlayClipOneShot(_monsterAudioID, sound, 0.8f, 1f);
            
            // raise the warning counter and start the TimeToLeave coroutine
            _exitWarningCounter++;
            StartCoroutine(TimeToLeave());

            yield return new WaitUntil(() => !_hasExitedWaypoint);

            // if the player has exited the waypoint, reset the warning counter
            StartCoroutine(LeavingWay());
        }

        // If the player has exited the waypoint, player has 15 seconds to get back to the waypoint
        private IEnumerator TimeToLeave()
        {
            int counter = _exitWarningCounter;

            yield return new WaitForSeconds(15f);

            // if the player has not returned to the waypoint, play the monster sound and kill the player
            if (_hasExitedWaypoint && counter == _exitWarningCounter)
            {
                AudioManager.Instance.PlayClipOneShot(_monsterAudioID, "MB attack", 1f, 1f);
                yield return new WaitForSeconds(0.5f);
                GameManager.Instance.Player.TakeDamage(101);
            }
        }

        #endregion

    #endregion




    #region Public Methods

    // Event load when the game starts with a save file
    public void EventLoad()
    {
        Debug.Log("Musicbox event loaded");
    }

    #endregion
}
