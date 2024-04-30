using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BehaviorTree;

public class ActionCheckNoise : Node
{
    #region Fields

    // References to components
    private Transform _transform;
    private NavMeshAgent _agent;
    private Animator _animator;
    private AISensor _aiSensor;
    private EnemyAudio _enemyAudio;

    // Variables for waiting behavior
    private bool _isWaiting = false;
    private float _waitTime = 1f;
    private float _waitCounter = 0f;

    private float _elapsedTime = 0f;
    private float _stuckTimer = 30f;

    // Enemy attributes
    private float _walkSpeed;

    private bool _debugMode;

    #endregion




    #region Constructors

    // Constructor to initialize references
    public ActionCheckNoise(bool debugMode, Transform transform, NavMeshAgent agent)
    {
        _transform = transform;
        _agent = agent;
        _animator = transform.GetComponent<Animator>();
        _walkSpeed = transform.GetComponent<Enemy>().WalkSpeed;
        _debugMode = debugMode;
        _aiSensor = transform.GetComponent<AISensor>();
        _enemyAudio = transform.GetComponent<EnemyAudio>();
    }

    #endregion




    #region Public Methods

    public override NodeState Evaluate()
    {

        #region FAILURE CHECKS

        if (GameManager.Instance.IsGamePaused)
        {
            if (_debugMode) Debug.Log("A - CheckNoise: RUNNING (game is paused)");
            return NodeState.RUNNING;
        }

        if (GetData("target") != null)
        {
            if (_debugMode) Debug.Log("A - CheckNoise: FAILURE (target != null)");
            return NodeState.FAILURE;
        }
        
        if (_aiSensor.IsPlayerInSight)
        {
            // Set state to FAILURE
            if (_debugMode) Debug.Log("A - CheckNoise: FAILURE (player in sight)");
            parent.parent.SetData("target", GameManager.Instance.Player.transform);
            return NodeState.FAILURE;
        }

        object objPos = GetData("noisePosition");
        if (objPos == null)
        {
            ClearData("noisePosition");
            ClearData("noiseLevel");
            // Set state to FAILURE
            if (_debugMode) Debug.Log("A - CheckNoise: FAILURE (noisePosition = null)");
            return NodeState.FAILURE;
        }
        
        #endregion




        #region ACTION

        Vector3 noisePos = (Vector3)objPos;

        // Check if the AI is waiting
        if (_isWaiting)
        {
            _waitCounter += Time.deltaTime;
            if (_waitCounter >= _waitTime)
            {
                _isWaiting = false;
                ClearData("noisePosition");
                ClearData("noiseLevel");
                _waitCounter = 0f;
                _elapsedTime = 0f;
                if (_debugMode) Debug.Log("A - CheckNoise: SUCCESS (wait time over)");
                return NodeState.SUCCESS;
            }
        }
        else
        {
            // Check if AI has reached the noise position
            if (Vector3.Distance(_transform.position, noisePos) < 0.1f)
            {
                _isWaiting = true;
                _animator.SetBool("walk", false);
            }
            else
            {
                _elapsedTime += Time.deltaTime;

                // Check if AI is stuck
                if (_elapsedTime >= _stuckTimer)
                {
                    _isWaiting = true;
                    _animator.SetBool("walk", false);
                    if (_debugMode) Debug.Log("A - CheckNoise: STUCK");
                }
                else
                {
                    _elapsedTime += Time.deltaTime;
                }

                _agent.speed = _walkSpeed;
                _agent.SetDestination(noisePos);
                _animator.SetBool("walk", true);
                _enemyAudio.VolumeFloorChanger();
            }
        }

        if (_debugMode) Debug.Log("A - CheckNoise: RUNNING (on the way)");
        return NodeState.RUNNING;

        #endregion
    }

    #endregion
}