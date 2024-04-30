using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

#region Boss State Enum

public enum BossState { CHASE, ATTACK, STUN, TELEPORT, HALF_LIFE, DIE, PAUSE };

#endregion

public class Boss : MonoBehaviour, ICustomUpdatable
{
    #region Fields
    
    [SerializeField] BossState _currentState;
    private BossState _previousState;

    [Header("Boss Attributes")]
    [Tooltip("Health of the boss")]
    [SerializeField] int _bossHealth = 100;
    [Tooltip("Damage dealt to the player")]
    [SerializeField] float _bossDamage = 25f;
    [Tooltip("Attack range of the boss")]
    [SerializeField] float _bossAttackRange = 2.5f;
    [Tooltip("Teleport range of the boss")]
    [SerializeField] float _bossTeleportRange = 11f;
    [Space(10)]

    [Header("Light Components")]
    [Tooltip("Sun object in the scene")]
    [SerializeField] GameObject _sunObject;
    [Tooltip("Reflection probes object in the scene")]
    [SerializeField] GameObject _reflectionProbesLight;
    [Tooltip("Reflection probes object in the scene when half health is reached")]
    [SerializeField] GameObject _reflectionProbesDark;
    [Tooltip("Lights container containing all the lights in the scene")]
    [SerializeField] Transform _lightsContainer;
    [Space(10)]

    [Header("Teleport Points")]
    [Tooltip("Teleport points for the boss to move around the scene upstairs and downstairs")]
    [SerializeField] Transform[] _teleportPointsUpstairs;
    [SerializeField] Transform[] _teleportPointsDownstairs;

    [Header("Particles")]
    [Tooltip("Dissolve particle system")]
    [SerializeField] ParticleSystem _dissolveParticle;
    [Tooltip("Explosion particle system")]
    [SerializeField] ParticleSystem _explosionParticle;
    [Tooltip("Steam particle system")]
    [SerializeField] ParticleSystem _steamParticle;
    [Tooltip("Teleport particle system prefab")]
    [SerializeField] GameObject _particlePrefab;
    [Tooltip("Steam particle system prefab")]
    [SerializeField] GameObject _steamPrefab;
    [Tooltip("Fire particle system prefab")]
    [SerializeField] GameObject _firePrefab;
    [Space(10)]

    [Header("Other Components")]
    [Tooltip("GameObject with meshes of the boss")]
    [SerializeField] GameObject _bossMeshContainer;
    [Tooltip("Player object")]
    [SerializeField] Transform _playerTransform;
    [Tooltip("Main key object")]
    [SerializeField] Transform _mainKey;
    [Tooltip("Game end object")]
    [SerializeField] GameEnd _gameEndScript;
    [Space(10)]

    [Header("Audio Components")]
    [Tooltip("Footsteps audio source of the boss")]
    [SerializeField] AudioSource _audioSourceSteps;
    [Tooltip("Speaker audio source of the boss")]
    [SerializeField] AudioSource _audioSourceSpeaker;
    [Tooltip("Breath audio source of the boss")]
    private AudioSource _audioSourceBreath;
    [Space(10)]

    [Header("Debug")]
    [Tooltip("Debug mode")]
    [SerializeField] bool _debugMode = false;

    // Other Boss components
    private Animator _bossAnimator;
    private NavMeshAgent _bossNavMeshAgent;
    private Collider[] _bossColliders;

    // Variables for calculations of teleport points
    private float _currentDistanceToPlayer = 100f;
    private int _currentTeleportNum = 0;

    // Booleans for states
    private bool _isHit = false;
    private bool _isTeleporting = false;
    private bool _hasHalfHealth = false;
    private bool _isDead = false;
    private bool _isHalfEventDone = false;
    private bool _hasHalfEventStarted = false;

    #endregion




    #region Unity Callbacks

    void Start()
    {
        _bossNavMeshAgent = GetComponent<NavMeshAgent>();
        _bossAnimator = GetComponent<Animator>();
        _audioSourceBreath = GetComponent<AudioSource>();
        _bossColliders = GetComponents<Collider>();
    }

    #endregion




    #region Custom Update and State Machine

    public void CustomUpdate(float deltaTime)
    {
        // Set the state to pause if the game is paused
        if (GameManager.Instance.IsGamePaused && _currentState != BossState.PAUSE) 
        {
            _previousState = _currentState;
            _currentState = BossState.PAUSE;
            StateMachine();
        }
        else if (GameManager.Instance.IsGamePaused || _currentState == BossState.PAUSE) return;

        // Call the state machine
        StateMachine();

        // Check the health of the boss for possible state changes
        if (_bossHealth <= 50 && !_hasHalfHealth)
        {
            _hasHalfHealth = true;
            _currentState = BossState.HALF_LIFE;
        }
        else if (_bossHealth <= 0 && !_isDead)
        {
            _currentState = BossState.DIE;
        }
    }

    private void StateMachine()
    {
        switch (_currentState)
        {
            case BossState.CHASE:

                if (_debugMode) Debug.Log("Chase");

                // Check if the player is in attack range
                if (Vector3.Distance(transform.position, _playerTransform.position) < _bossAttackRange)
                {
                    _currentState = BossState.ATTACK;
                }
                // Check if the player is out of chase range for teleport
                else if (Vector3.Distance(transform.position, _playerTransform.position) > _bossTeleportRange)
                {
                    _bossAnimator.SetTrigger("idle");
                    _currentState = BossState.TELEPORT;
                }
                // Continue chasing the player
                else Chase();

                break;

            case BossState.ATTACK:

                if (_debugMode) Debug.Log("Attack");

                // Check if the player is out of attack range
                if (Vector3.Distance(transform.position, _playerTransform.position) > _bossAttackRange && Vector3.Distance(transform.position, _playerTransform.position) < _bossTeleportRange)
                {
                    _bossAnimator.SetTrigger("walk");
                    _currentState = BossState.CHASE;
                }
                // Check if the player is out of chase range for teleport
                else if (Vector3.Distance(transform.position, _playerTransform.position) > _bossTeleportRange)
                {
                    _bossAnimator.SetTrigger("idle");
                    _currentState = BossState.TELEPORT;
                }
                // Attack the player
                else _bossAnimator.SetTrigger("attack");

                break;

            case BossState.STUN:

                if (_debugMode) Debug.Log("Stunned");

                // Stun the boss for a few seconds
                Stunned();

                break;

            case BossState.TELEPORT:

                if (_debugMode) Debug.Log("Teleporting");

                // Teleport the boss to a new location
                if (!_isTeleporting) StartCoroutine(TeleportCoroutine());

                break;

            case BossState.HALF_LIFE:

                if (_debugMode) Debug.Log("Half");

                // Start the half event
                if (!_hasHalfEventStarted) StartCoroutine(HalfEventRoutine());

                break;

            case BossState.DIE:

                if (_debugMode) Debug.Log("Die");

                // Start the death event
                if (!_isDead) StartCoroutine(Die());

                break;

            case BossState.PAUSE:

                if (_debugMode) Debug.Log("Pause");

                // Pause the game and the boss
                StartCoroutine(PauseGame());

                break;
        }
    }

    #endregion




    #region Pause State

    IEnumerator PauseGame()
    {
        // pause the navmesh agent and the audio sources
        _bossNavMeshAgent.isStopped = true;
        _audioSourceBreath.Pause();
        _audioSourceSteps.Pause();

        // wait until the game is unpaused
        yield return new WaitUntil(() => GameManager.Instance.IsGamePaused == false);

        // unpause the navmesh agent and the audio sources
        _audioSourceBreath.UnPause();
        _audioSourceSteps.UnPause();
        _bossNavMeshAgent.isStopped = false;

        // set the state back to the previous state
        _currentState = _previousState;
    }

    #endregion




    #region Chase State

    void Chase()
    {
        // unpause the navmesh agent and set the destination to the player
        _bossNavMeshAgent.isStopped = false;
        _bossNavMeshAgent.SetDestination(_playerTransform.position);
        _bossAnimator.SetTrigger("walk");
    }

    #endregion




    #region Stunned State

    void Stunned()
    {
        // make sure the boss is stunned only once
        if (!_isHit) _isHit = true;
        else return;

        // play sound and animation
        AudioManager.Instance.PlayClipOneShot(gameObject.GetInstanceID(), "priest hit", 1f, 1f);
        _bossAnimator.SetTrigger("hit");

        // pause the boss for a few seconds
        _audioSourceSteps.Pause();
        _bossNavMeshAgent.isStopped = true;

        // wait for a few seconds and reset the movement
        StartCoroutine(ResetMovement());
    }

    IEnumerator ResetMovement()
    {
        yield return new WaitForSeconds(2f);

        // unpause all audio sources
        _audioSourceBreath.Pause();
        _audioSourceSpeaker.Play();
        _audioSourceSteps.UnPause();

        // set the state back to chase
        _currentState = BossState.CHASE;

        // wait until the speaker audio source is done playing
        yield return new WaitUntil (() => _audioSourceSpeaker.isPlaying == false);

        // unpause the breath audio source
        _audioSourceBreath.UnPause();

        yield return new WaitForSeconds(1f);

        // reset the hit boolean
        _isHit = false;
    }

    #endregion




    #region Teleport State

    IEnumerator TeleportCoroutine()
    {
        // set the teleporting boolean to true and disable all colliders to prevent damage
        _isTeleporting = true;
        foreach (Collider col in _bossColliders) col.enabled = false;

        // Start the teleport event by playing the dissolve particle effect
        _dissolveParticle.Play();

        yield return new WaitForSeconds(3f);

        // stop the enemy from moving and stop the step audio source
        _bossAnimator.SetTrigger("idle");
        _audioSourceSteps.Pause();
        _bossNavMeshAgent.isStopped = true;

        // play sound and particle effects to indicate the teleport
        AudioManager.Instance.PlayClipOneShot(gameObject.GetInstanceID(), "explosion", 1f, 1f);
        _explosionParticle.Play();

        // Instantiate fire particle effect and play it together with the steam particle effect
        GameObject firePS = Instantiate(_firePrefab, transform.position, Quaternion.identity);
        _steamParticle.Play();

        yield return new WaitForSeconds(0.3f);

        // disable the boss mesh to make him invisible and play the teleport sound
        _bossMeshContainer.SetActive(false);
        AudioManager.Instance.PlayClipOneShot(gameObject.GetInstanceID(), "teleport forth", 1f, 1f);

        yield return new WaitForSeconds(1f);

        // stop all particle effects
        _dissolveParticle.Stop();
        _explosionParticle.Stop();
        _steamParticle.Stop();

        // get the teleport points
        Transform[] teleporters = ScanPositions();

        yield return new WaitForSeconds(0.25f);

        // instantiate the teleport indication particle effect to give player a hint where the boss will appear
        GameObject obj = Instantiate(_particlePrefab, teleporters[0].position, Quaternion.identity);
        GameObject obj2 = null;

        // if the boss has half health, instantiate two particle effects to make it more difficult
        if (_hasHalfHealth) obj2 = Instantiate(_particlePrefab, teleporters[1].position, Quaternion.identity);

        // start the half event if the boss has half health and it is not done yet
        if (_hasHalfHealth && !_isHalfEventDone)
        {
            StartCoroutine(HalfEventRoutine());
            yield return new WaitUntil(() => _isHalfEventDone == true);
        }

        // play the second teleport particle effect if the boss has half health
        obj.GetComponent<ParticleSystem>().Play();

        // instantiate the steam particle effect at the teleport points
        GameObject steamPS = Instantiate(_steamPrefab, teleporters[0].position, Quaternion.identity);
        GameObject steamPS2 = null;
        // if the boss has half health, instantiate two steam particle effects to make it more difficult
        if (_hasHalfHealth) 
        {
            obj2.GetComponent<ParticleSystem>().Play();
            steamPS2 = Instantiate(_steamPrefab, teleporters[1].position, Quaternion.identity);
        }

        yield return new WaitForSeconds(2f);

        // play the teleport back sound and teleport the boss back to the teleport point
        AudioManager.Instance.PlayClipOneShot(gameObject.GetInstanceID(), "teleport back", 1f, 1f);
        
        // set the teleport point to the random teleport point close to the first one if the boss has half health
        int random = 0;
        if (_hasHalfHealth) random = Random.Range(0, 2);

        // teleport the boss to the teleport point
        transform.position = teleporters[random].position;

        // make the boss visible again and activate the colliders
        _bossMeshContainer.SetActive(true);
        foreach (Collider col in _bossColliders) col.enabled = true;

        // avoid the boss from being stunned after teleporting
        _isHit = true;

        // destroy the fire of the last teleport point
        Destroy(firePS);

        yield return new WaitForSeconds(0.5f);

        // Unpause the audio sources and the navmesh agent and set the state back to chase
        _audioSourceSteps.UnPause();
        _bossNavMeshAgent.isStopped = false;
        _bossAnimator.SetTrigger("walk");
        _currentState = BossState.CHASE;

        // wait for a few seconds and make the boss stunnable again
        yield return new WaitForSeconds(1.5f);

        _isHit = false;

        yield return new WaitForSeconds(0.5f);

        // destroy the teleport particle effects and the steam particle effects
        Destroy(obj);
        Destroy(steamPS);
        if (_hasHalfHealth) 
        {
            Destroy(obj2);
            Destroy(steamPS2);
        }

        // the boss can teleport again
        _isTeleporting = false;

    }

    // Scan the teleport points and return the closest two points
    Transform[] ScanPositions()
    {
        // check if the player is upstairs or downstairs and set the teleport points accordingly
        if (_playerTransform.position.y < -3.76f)
        {
            FindClosestPoint(_teleportPointsDownstairs);
        }
        else
        {
            FindClosestPoint(_teleportPointsUpstairs);
        }

        // return the teleport points
        Transform[] teleportPoints = new Transform[2];
        teleportPoints[0] = _teleportPointsUpstairs[_currentTeleportNum];

        // set the second teleport point to the next or previous point if the boss has half health
        if (_currentTeleportNum + 1 != _teleportPointsUpstairs.Length) 
            teleportPoints[1] = _teleportPointsUpstairs[_currentTeleportNum + 1];
        else if (_currentTeleportNum - 1 != -1)
            teleportPoints[1] = _teleportPointsUpstairs[_currentTeleportNum - 1];

        return teleportPoints;
    }

    // Find the closest teleport point to the player
    private void FindClosestPoint(Transform[] teleportPoints)
    {
        for (int i = 0; i < teleportPoints.Length; i++)
        {
            // calculate the distance to the player
            float newDistance = Vector3.Distance(teleportPoints[i].position, _playerTransform.position);

            // set the new distance if it is smaller than the current distance
            if (newDistance < _currentDistanceToPlayer)
            {
                // set the new distance and the current teleport point
                _currentDistanceToPlayer = newDistance;
                _currentTeleportNum = i;
            }
        }

        // reset the distance to the player
        _currentDistanceToPlayer = 100f;
    }

    #endregion




    #region Half Life Event

    IEnumerator HalfEventRoutine()
    {
        // set the half event started boolean to true and wait until the boss is done teleporting
        _hasHalfEventStarted = true;
        yield return new WaitUntil(() => _isTeleporting == false);

        // turn off the lights and play a sound
        LightsOff(_lightsContainer);
        AudioManager.Instance.PlayClipOneShot(AudioManager.Instance.PlayerSpeaker2, "power down", 0.8f, 1f);

        yield return new WaitForSeconds(0.5f);

        // switch the reflection probes
        _reflectionProbesLight.SetActive(false);
        _reflectionProbesDark.SetActive(true);

        yield return new WaitForSeconds(5f);

        // make the boss faster and set the half event done boolean to true
        _bossNavMeshAgent.speed = 5f;
        _isHalfEventDone = true;
    }

    // Turn off all lights in the scene
    void LightsOff(Transform lights)
    {
        for (int i = 0; i < lights.childCount; i++)
        {
            Transform lightChild = lights.GetChild(i);
            Light light = lightChild.GetComponent<Light>();
            if (light != null)
            {
                light.enabled = false;
            }
            else if (lightChild.childCount > 0)
            {
                LightsOff(lightChild);
            }
        }
        _sunObject.SetActive(false);
    }

    #endregion




    #region Die Event

    IEnumerator Die()
    {
        // set the boss to dead and stop the navmesh agent
        _isDead = true;
        _bossNavMeshAgent.isStopped = true;

        // stop audio sources and Fade out the environment and breathing sound
        _audioSourceSpeaker.Stop();
        _audioSourceSteps.Stop();
        AudioManager.Instance.FadeOutAudio(_audioSourceBreath.gameObject.GetInstanceID(), 1f);
        AudioManager.Instance.FadeOutAudio(AudioManager.Instance.Environment, 5f);

        // stop all particle effects and play the die animation
        _dissolveParticle.Stop();
        _explosionParticle.Stop();
        _steamParticle.Stop();
        _bossAnimator.SetTrigger("die");

        // remove the boss from the custom update manager
        GameManager.Instance.CustomUpdateManager.RemoveCustomUpdatable(this);

        yield return new WaitForSeconds(7f);

        AudioManager.Instance.PlayClipOneShot(AudioManager.Instance.PlayerSpeaker2, "speaker after boss");

        yield return new WaitForSeconds(3f);

        // play the dissolve particle effect and move the main key to the boss position
        _dissolveParticle.Play();
        _mainKey.position = transform.position;

        yield return new WaitForSeconds(3f);

        // play the teleport sound and move the boss away from the scene
        AudioManager.Instance.PlayClipOneShot(AudioManager.Instance.PlayerSpeaker2, "teleport forth", 0.6f, 1f);
        transform.position = new Vector3(0f, -1000f, 0f);

        // add the game end script to the custom update manager
        GameManager.Instance.CustomUpdateManager.AddCustomUpdatable(_gameEndScript);

        yield return new WaitForSeconds(1f);

        // remove the custom update manager from the boss and deactivate the game object
        GameManager.Instance.CustomUpdateManager.RemoveCustomUpdatable(this);
        gameObject.SetActive(false);
    }

    #endregion




    #region Public and Event Methods

    // Used to start the event
    public void FollowPlayer()
    {
        GameManager.Instance.CustomUpdateManager.AddCustomUpdatable(this);
        _currentState = BossState.CHASE;
        _audioSourceSteps.Play();
        Debug.Log("START TO FOLLOW PLAYER");
    }

    // called by weapon script
    public void GetHit()
    {
        _bossHealth -= 10;
        if (_bossHealth <= 50 && !_hasHalfHealth) return;
        if (_currentState != BossState.STUN && _bossHealth > 0 && !_isHit) _currentState = BossState.STUN;
        else if (_bossHealth <= 0) _currentState = BossState.DIE;
    }

    // called by animation event
    private void AttackAnim()
    {
        GameManager.Instance.Player.TakeDamage(_bossDamage);
        AudioManager.Instance.PlayClipOneShot(gameObject.GetInstanceID(), "mannequin hit", 0.6f, 1f);
    }

    #endregion
}
