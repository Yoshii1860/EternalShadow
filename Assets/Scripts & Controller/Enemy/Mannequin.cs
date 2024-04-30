using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(UniqueIDComponent))]
public class Mannequin : MonoBehaviour, ICustomUpdatable
{
    #region Private Fields

    [Header("Mannequin Settings")]
    [Tooltip("Reference to the head of the mannequin")]
    [SerializeField] private Transform _headTransform;
    [Tooltip("Speed at which the head rotates to face the player")]
    [SerializeField] private float _headRotationSpeed = 1f;
    [Tooltip("Array of body parts of the mannequin for death animation - First element should be the head")]
    [SerializeField] private Transform[] _bodyParts;
    [SerializeField] private Transform[] _deathParts;
    [Tooltip("Maximum distance at which the mannequin can follow the player")]
    [SerializeField] private float _maxFollowDistance = 12f;

    // Reference to the player
    private Transform _player;
    // Reference to the Player script
    private Player _playerScript;
    // Reference to the NavMeshAgent component of the mannequin
    private NavMeshAgent _navMeshAgent;
    // Reference to the Animator component of the mannequin
    private Animator _animator;
    // Timer to prevent the mannequin from attacking too frequently
    private float _attackTimer = 0f;

    // Flag to check if the mannequin is stopped
    private bool _isStopped = true;
    // Flag to check if the mannequin's audio is playing for the first time
    private bool _hasAudioStarted = false;
    // Flag to check if the stuck check coroutine is active
    private bool _isStuckCheckActive = false;

    #endregion




    #region Public Fields

    [Tooltip("Flag to check if the mannequin is dead")]
    public bool IsDead = false;
    [Tooltip("Flag to check if the mannequin is moving")]
    public bool IsMoving = false;
    [Tooltip("Flag to check if the mannequin has HasEventStarted moving")]
    public bool HasEventStarted = false;

    #endregion




    #region MonoBehaviour Callbacks

    void Start()
    {
        // Initialize references
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        _playerScript = _player.GetComponent<Player>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
    }

    public void CustomUpdate(float deltaTime)
    {
        if (GameManager.Instance.IsGamePaused) 
        {
            StopAnimation();
            return;
        }

        if (IsDead) return;

        // Update mannequin behaviors
        if (!HasEventStarted) return;

        UpdateMannequinBehaviors();

        UpdateAttackTimer(deltaTime);
    }
    #endregion




    #region Public Methods

    // Called when the mannequin is hit by the player
    public void GetHit(GameObject hitObject)
    {
        if (string.Compare(hitObject.name, "head") == 0)
        {
            IsDead = true;
            hitObject.GetComponent<Rigidbody>().AddForce(-transform.forward * 5f, ForceMode.Impulse);
            AudioManager.Instance.SetAudioClip(gameObject.GetInstanceID(), "mannequin death", 1f, 1f, false);

            StartCoroutine(Death());
        }
        else
        {
            Debug.Log("Mannequin hit by player: " + hitObject.name);
        }
    }

    #endregion




    #region Behavior Methods

    private void UpdateMannequinBehaviors()
    {
        TurnHead(); // Rotate the mannequin's head to face the player
        if (!IsMoving) return;
        MoveMannequin(); // Move the mannequin based on player's visibility
        CheckDoor(); // Check if the mannequin is close to a door to open it 
    }

    private void TurnHead()
    {
        // Calculate direction to the player
        Vector3 directionToPlayer = (_player.position - _headTransform.position).normalized;

        // Ignore vertical component (Y-axis)
        directionToPlayer.y = 0f;

        // Rotate the head towards the player
        if (directionToPlayer != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            _headTransform.rotation = Quaternion.Slerp(_headTransform.rotation, targetRotation, _headRotationSpeed * Time.deltaTime);
        }
    }

    private void MoveMannequin()
    {
        // Calculate the angle between the player and the mannequin
        float angle = Vector3.Angle(_player.position - _headTransform.position, _player.forward);

        // Calculate the position of the player's head
        float yOffset = 1.5f;
        Vector3 playerHead = _player.position + new Vector3(0f, yOffset, 0f);

        // Check if the player is visible by performing a linecast from the mannequin's head to the player's head
        // Ignore the "Characters" and "Enemies" layers
        int layerMask = ~((1 << LayerMask.NameToLayer("Characters")) | (1 << LayerMask.NameToLayer("Enemies")));
        bool isPlayerVisible = Physics.Linecast(_headTransform.position, playerHead, layerMask);
        float distance = Vector3.Distance(_player.position, _headTransform.position);

        // If the mannequin is outside the player's field of view or the player is not visible to the mannequin, 
        // the mannequin starts to move
        if (angle < 90f && distance < _maxFollowDistance || isPlayerVisible && distance < _maxFollowDistance)
        {
            ResumeAnimation(); // Resume the mannequin's movement
            if (_attackTimer == 0) AttackRange(); // Attack the player if close enough
            Debug.DrawLine(_headTransform.position, playerHead, Color.green); // Draw a debug line to visualize player visibility
            if (!_isStuckCheckActive) StartCoroutine(StuckCheck());
        }
        else
        {
            StopAnimation(); // Stop the mannequin's movement
            Debug.DrawLine(_headTransform.position, playerHead, Color.red); // Draw a debug line to visualize player visibility
            if (_isStuckCheckActive) 
            {
                StopCoroutine(StuckCheck());
                _isStuckCheckActive = false;
            }
        }
    }

    // Check if the mannequin is close to a door to open it
    private void CheckDoor()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 2f, LayerMask.GetMask("Interact"));
        
        if (hitColliders.Length > 0)
        {
            for (int i = 0; i < hitColliders.Length; i++)
            {
                Door door = hitColliders[i].GetComponent<Door>();
                if (door != null)
                {
                    if (!door.DoorState() && !door.IsLocked)
                    {
                        door.OpenDoor();
                    }
                }
            }
        }
    }

    #endregion




    #region Private Methods

    // Stop the mannequin's movement and audio
    private void StopAnimation()
    {
        if (_isStopped) return;
        AudioManager.Instance.PauseAudio(gameObject.GetInstanceID());
        _navMeshAgent.SetDestination(transform.position);
        _animator.speed = 0;
        _isStopped = true;
    }

    // Resume the mannequin's movement and audio
    private void ResumeAnimation()
    {
        // Resume the mannequin's movement
        if (!_isStopped) return;
        _animator.SetTrigger("Walk");

        // Resume audio if it's paused, else play audio
        if (!_hasAudioStarted) 
        {
            AudioManager.Instance.PlayAudio(gameObject.GetInstanceID(), 1f, 1f, true);
            _hasAudioStarted = true;
        }
        else AudioManager.Instance.UnpauseAudio(gameObject.GetInstanceID());

        _navMeshAgent.SetDestination(_player.position);
        _animator.speed = 1;
        _isStopped = false;
    }

    // Attack the player if close enough
    private void AttackRange()
    {
        // Check if mannequin is close enough to attack
        float distance = Vector3.Distance(_player.position, _headTransform.position);

        if (distance < 2f)
        {
            _attackTimer = 2f;
            StartCoroutine(AttackHit());
        }
    }

    // Update the attack timer to prevent the mannequin from attacking too frequently
    private void UpdateAttackTimer(float deltaTime)
    {
        if (_attackTimer > 0f)
        {
            _attackTimer -= deltaTime;
            if (_attackTimer <= 0f)
            {
                _attackTimer = 0f;
            }
        }
    }

    // Play pain sound with random volume, pitch, and sound
    private void PlayHitSoundOnPlayer()
    {
        int randomNum = Random.Range(1, 5);
        float randomVolume = Random.Range(0.6f, 1.0f);
        string painSound = "pain" + randomNum.ToString();

        AudioManager.Instance.PlayClipOneShot(AudioManager.Instance.PlayerSpeaker2, painSound, randomVolume);
    }

    // Switch body part with death part
    private void SwitchBodyPart(Transform bodyPart, Transform deathPart)
    {
        bodyPart.position = deathPart.position;
        bodyPart.rotation = deathPart.rotation;
        deathPart.gameObject.SetActive(true);
        bodyPart.gameObject.SetActive(false);
        // add force to head to make it fall
        if (string.Compare(deathPart.name, "head") == 0)
        {
            deathPart.GetComponent<Rigidbody>().AddForce(-transform.forward * 5f, ForceMode.Impulse);
        }
    }

    #endregion




    #region Coroutines

        // Check if the mannequin is stuck over a period of time
    IEnumerator StuckCheck()
    {
        _isStuckCheckActive = true;

        // if magnitude of velocity is less than 0.1, mannequin is stuck
        if (_navMeshAgent.velocity.magnitude < 0.1f)
        {
            float timer = 3f;
            while (timer > 0)
            {
                timer -= Time.deltaTime;

                if (_navMeshAgent.velocity.magnitude > 0.1f)
                {
                    _isStuckCheckActive = false;
                    yield break;
                }

                yield return null;
            }
            Debug.Log("Mannequin is stuck!");

            // Stop the mannequin's movement to reset the path
            StopAnimation();
        }

        _isStuckCheckActive = false;
    }

    // Start the mannequin's movement
    IEnumerator AttackHit()
    {
        AudioManager.Instance.PlayClipOneShot(AudioManager.Instance.PlayerSpeaker2, "mannequin hit", 1f); // Play attack sound

        // Cause damage to the player
        float randomizer = Mathf.FloorToInt(Random.Range(15, 33f));
        _playerScript.TakeDamage(randomizer);

        // Cause bleeding to the player with a 15% chance
        randomizer = Random.Range(0,100);
        if (randomizer <= 15)
        {
            if (!_playerScript.IsBleeding) _playerScript.Bleeding();
        }

        yield return new WaitForSeconds(0.5f);

        PlayHitSoundOnPlayer();
    }

    // Death animation for the mannequin
    IEnumerator Death()
    {
        AudioManager.Instance.PlayAudio(gameObject.GetInstanceID());

        // match body parts with death parts
        for (int i = 0; i < _bodyParts.Length; i++)
        {
            for (int j = 0; j < _deathParts.Length; j++)
            {
                if (string.Compare(_bodyParts[i].name, _deathParts[j].name) == 0)
                {
                    Debug.Log("Switching body part: " + _bodyParts[i].name);
                    // switch body part with death part
                    SwitchBodyPart(_bodyParts[i], _deathParts[j]);
                    // wait 2 seconds for head to fall before switching other body parts
                    if (string.Compare(_deathParts[j].name, "head") == 0) yield return new WaitForSeconds(2f);
                    break;
                }
            }
        }

        yield return new WaitUntil (() => !AudioManager.Instance.IsPlaying(gameObject.GetInstanceID()));

        // stop audio, remove audio source, and remove custom updatable
        AudioManager.Instance.StopAudio(gameObject.GetInstanceID());
        GameManager.Instance.CustomUpdateManager.RemoveCustomUpdatable(this);
        AudioManager.Instance.RemoveAudioSource(gameObject.GetInstanceID());

        yield return new WaitForSeconds(30f);
        
        // destroy mannequin
        gameObject.SetActive(false);
    }

    #endregion
}