using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BehaviorTree;

public class ActionLastKnownPosition : Node
{
    #region Fields

    // References to components
    private Transform _transform;
    private NavMeshAgent _agent;
    private Animator _animator;
    private AISensor _aiSensor;
    private EnemyAudio _enemyAudio;

    // Enemy attributes
    private float _walkSpeed;

    // Debug mode flag
    private bool _debugMode;

    #endregion




    #region Constructors

    // Constructor to initialize references
    public ActionLastKnownPosition(bool debugMode, Transform transform, NavMeshAgent agent)
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

    // Evaluate method to determine the state of the node
    public override NodeState Evaluate()
    {
        #region FAILURE CHECKS

        if (GameManager.Instance.IsGamePaused)
        {
            if (_debugMode) Debug.Log("A - LastKnownPosition: RUNNING (game is paused)");
            return NodeState.RUNNING;
        }

        if (GetData("target") != null)
        {
            if (_debugMode) Debug.Log("A - LastKnownPosition: FAILURE (target = null)");
            ClearData("lastKnownPosition");
            return NodeState.FAILURE;
        }

        if (_aiSensor.IsPlayerInSight)
        {
            if (_debugMode) Debug.Log("A - LastKnownPosition: FAILURE (Player in Sight)");
            ClearData("lastKnownPosition");
            parent.parent.SetData("target", GameManager.Instance.Player.transform);
            return NodeState.FAILURE;
        }

        object obj = GetData("lastKnownPosition");
        if (obj == null)
        {
            if (_debugMode) Debug.Log("A - LastKnownPosition: FAILURE (lastKnownPosition = null) or (Player in Sight)");
            ClearData("lastKnownPosition");
            return NodeState.FAILURE;
        }
        
        #endregion




        #region ACTION

        Vector3 lastKnownPosition = (Vector3)obj;

        // Check if AI has reached the last known position
        if (Vector3.Distance(_transform.position, lastKnownPosition) < 0.1f)
        {
            _animator.SetBool("walk", false);
            ClearData("lastKnownPosition");
            if (_debugMode) Debug.Log("A - LastKnownPosition: SUCCESS (Reached Last Known Position)");
            return NodeState.SUCCESS;
        }

        _agent.speed = _walkSpeed;
        _agent.SetDestination(lastKnownPosition);
        _animator.SetBool("walk", true);
        _enemyAudio.VolumeFloorChanger();

        if (_debugMode) Debug.Log("A - LastKnownPosition: RUNNING (Moving to Last Known Position)");
        return NodeState.RUNNING;

        #endregion
    }

    #endregion
}
