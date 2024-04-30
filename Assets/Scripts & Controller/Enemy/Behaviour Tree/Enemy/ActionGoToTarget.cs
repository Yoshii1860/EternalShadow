using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BehaviorTree;

public class ActionGoToTarget : Node
{
    #region Fields

    // References to components
    private Animator _animator;
    private Transform _transform;
    private NavMeshAgent _agent;
    private AISensor _aiSensor;
    private EnemyAudio _enemyAudio;

    // Enemy attributes
    private float _attackRange;
    private float _runSpeed;

    private bool _debugMode;

    #endregion




    #region Constructors

    // Constructor to initialize references
    public ActionGoToTarget(bool debugMode, Transform transform, NavMeshAgent agent)
    {
        _transform = transform;
        _agent = agent;
        _animator = transform.GetComponent<Animator>();
        _attackRange = transform.GetComponent<Enemy>().AttackRange;
        _runSpeed = transform.GetComponent<Enemy>().RunSpeed;
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
            if (_debugMode) Debug.Log("A - GoToTarget: FAILURE (game is paused)");
            return NodeState.FAILURE;
        }

        if (_aiSensor.IsPlayerHidden)
        {
            if (_debugMode) Debug.Log("A - GoToTarget: FAILURE (Hidden)");
            ClearData("target");
            return NodeState.FAILURE;
        }

        object obj = GetData("target");
        if (obj == null)
        {
            if (_debugMode) Debug.Log("A - GoToTarget: FAILURE (target = null)");
            return NodeState.FAILURE;
        }
        
        #endregion




        #region ACTION

        // Get target transform
        Transform target = (Transform)obj;

        float distanceToTarget = Vector3.Distance(_transform.position, target.position);
        if (distanceToTarget < _attackRange)
        {
            if (_debugMode) Debug.Log("A - GoToTarget: SUCCESS (Target in Attack Range)");
            return NodeState.SUCCESS;
        }

        // Set agent speed, set destination, and trigger running animation
        _agent.speed = _runSpeed;
        _animator.SetBool("walk", false);
        _animator.SetBool("run", true);
        _agent.SetDestination(target.position);

        _enemyAudio.VolumeFloorChanger();

        // Return RUNNING to indicate that the action is ongoing
        if (_debugMode) Debug.Log("A - GoToTarget: RUNNING (Going to target)");
        return NodeState.RUNNING;

        #endregion
    }

    #endregion
}