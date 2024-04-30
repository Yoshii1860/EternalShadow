using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MirrorCode : MonoBehaviour, ICustomUpdatable
{
    #region Layers and Transforms

    private LayerMask _playerLayer; // Layer to identify the player object
    private LayerMask _mirrorLayer; // Layer to identify the mirror object
    private Transform _spotlightTransform; // Transform of the spotlight on the mirror
    [Tooltip("The transform of the camRoot on the player.")]
    [SerializeField] private Transform _camRootTransform;
    [Space(10)]

    #endregion




    #region References

    [Tooltip("The second spotlight on the door mirror.")]
    [SerializeField] private GameObject _secondSpotlight;
    [Tooltip("The new follow target for the player.")]
    [SerializeField] private Transform _newFollowTarget;
    [Tooltip("The particles on the cross.")]
    [SerializeField] private GameObject _particles;
    [Tooltip("The animator on the cross.")]
    [SerializeField] private Animator _animator;
    [Space(10)]

    #endregion




    #region Other References

    [Tooltip("The _door object.")]
    [SerializeField] private Door _door;
    [Tooltip("The room lights.")]
    [SerializeField] private Light _roomLights;

    #endregion




    #region Variables

    private bool _playOnce = false; // Flag to track if the event has been triggered

    private const float MAX_Y_POSITION = 0.06f; // The maximum Y position to hit of the spotlight on the mirror
    private const float MIN_Y_POSITION = -0.06f; // The minimum Y position to hit of the spotlight on the mirror
    private const float MAX_ANGLE = 14.6f; // The maximum angle to hit of the spotlight on the mirror
    private const float MIN_ANGLE = 14.2f; // The minimum angle to hit of the spotlight on the mirror

    #endregion




    #region Unity Methods

    void Start()
    {
        _playerLayer = 1 << LayerMask.NameToLayer("Characters");
        _spotlightTransform = transform.GetChild(0);
        _mirrorLayer = 1 << gameObject.layer;

        _spotlightTransform.gameObject.SetActive(false); // Disable the spotlight initially
        _secondSpotlight.SetActive(false); // Disable the second spotlight initially
        this.enabled = false; // Disable the script initially
    }

    #endregion




    #region Custom Update
    
    public void CustomUpdate(float deltaTime)
    {
        // Check if the event has been triggered - if so (by save file data), move the cross to the correct position
        if (GameManager.Instance.EventData.CheckEvent("Mirror") && !_playOnce)
        {
            _playOnce = true;
            _animator.transform.position = new Vector3(6.61549997f, 0.888799667f, 2.56540012f);
            _animator.transform.eulerAngles = new Vector3(0, 0, 277.630005f);
            GameManager.Instance.CustomUpdateManager.RemoveCustomUpdatable(this);
        }

        if (GameManager.Instance.Player.Flashlight.enabled) 
        {
            RaycastOnMirror();
        }
    }

    #endregion




    #region Private Methods

    private void RaycastOnMirror()
    {
        // Raycast from the flashlight towards the direction it's facing
        Ray ray = new Ray(_camRootTransform.position, _camRootTransform.forward);
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * 20f, Color.red); // Draw the ray in the scene view

        // Check for collision with the mirror object
        if (Physics.Raycast(ray, out hit, 20f, _mirrorLayer))
        {
            Debug.Log("Hit the mirror!");

            // Calculate the angle between the normal of the mirror and the forward direction of the camera
            float dotProduct = Vector3.Dot(_camRootTransform.forward.normalized, hit.normal.normalized);
            float angleInRadians = Mathf.Acos(dotProduct);
            float angleInDegrees = Mathf.Rad2Deg * angleInRadians;
            float angleDegreesNormal = angleInDegrees - 90f;

            _spotlightTransform.position = hit.point; // Set the position of the spotlight to the hit point

            // Enable the spotlight on the mirror
            _spotlightTransform.gameObject.SetActive(true);

            // Get the local Y-axis of the spotlight
            Vector3 angle = new Vector3(0, angleDegreesNormal, 0);

            // Smoothly rotate the spotlight towards the target rotation
            _spotlightTransform.localEulerAngles = angle;

            if (!GameManager.Instance.EventData.CheckEvent("Mirror")) CheckMirrorHit(angleDegreesNormal);
        }
        else
        {
            Debug.Log("No mirror hit!");
            
            // Disable the spotlight if not pointing at the mirror
            _spotlightTransform.gameObject.SetActive(false);
        }
    }

    private void CheckMirrorHit(float angle)
    {
        // Check if the door is open
        if (!_door.DoorState())
        {
            // Check if the spotlight is within the correct position and angle
            if (_spotlightTransform.localPosition.y  > MIN_Y_POSITION
            && _spotlightTransform.localPosition.y   < MAX_Y_POSITION
            && angle                                > MIN_ANGLE
            && angle                                < MAX_ANGLE)
            {
                // Check if the flashlight is pointing (aim) at the mirror and the room lights are off
                if (GameManager.Instance.Player.Flashlight.spotAngle < 35f && !_roomLights.enabled && !_playOnce)
                {
                    // Start the event
                    _playOnce = true;
                    Vector3 playerPosition = GameManager.Instance.Player.transform.position;
                    Vector3 playerEulerAngles = GameManager.Instance.Player.transform.eulerAngles;
                    StartCoroutine(BurnCross(playerPosition, playerEulerAngles));
                }
                else Debug.Log("MirrorEvent: spotAngle, roomLights or playOnce false!");
            }
            else
            {
                _secondSpotlight.SetActive(false);
            }
        }
        else Debug.Log("MirrorEvent: The door is closed!");
    }

    #endregion




    #region Coroutines

    // 
    private IEnumerator BurnCross(Vector3 position, Vector3 eulerAngles)
    {
        // Freeze the player and move the player to the correct position and rotation
        float time = 1.5f;
        while (time > 0)
        {
            GameManager.Instance.Player.transform.eulerAngles = eulerAngles;
            GameManager.Instance.Player.transform.position = position;
            time -= Time.deltaTime;
            yield return null;
        }

        // Disable the collider of the mirror
        transform.GetComponent<BoxCollider>().enabled = false;

        // Start a gameplay event
        GameManager.Instance.GameplayEvent();
        GameManager.Instance.PlayerController.SetFollowTarget(_newFollowTarget);

        // Permanently activate the second spotlight
        _secondSpotlight.SetActive(true);
        
        yield return new WaitForSeconds(1.5f);

        // Play the cross burn audio and enable the particles
        _particles.SetActive(true);
        AudioManager.Instance.SetAudioClip(_particles.transform.parent.gameObject.GetInstanceID(), "cross burn", 1f, 1f, false);
        AudioManager.Instance.PlayAudio(_particles.transform.parent.gameObject.GetInstanceID());

        yield return new WaitForSeconds(1f);

        // Play the cross fall animation
        _animator.SetTrigger("Fall");

        yield return new WaitForSeconds(9f);

        // Disable the spotlight
        _secondSpotlight.SetActive(false);

        // Resume the game and set the event to active
        GameManager.Instance.PlayerController.SetFollowTarget();
        GameManager.Instance.ResumeGame();
        GameManager.Instance.EventData.SetEvent("Mirror");

        // Disable the script and remove it from the custom update manager
        this.enabled = false;
        GameManager.Instance.CustomUpdateManager.RemoveCustomUpdatable(this);
    }

    #endregion




    #region Public Methods

    // Load the event
    public void EventLoad()
    {
        Debug.Log("Mirror event loaded");
    }

    #endregion
}
