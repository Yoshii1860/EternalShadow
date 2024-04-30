using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BehaviorTree;

public class ActionPatrol : Node
{
    #region Fields

    // References to components
    private Transform _transform;
    private Animator _animator;
    private Transform[] _waypoints;
    private NavMeshAgent _agent;
    private AISensor _aiSensor;
    private EnemyAudio _enemyAudio;

    // Patrol-related variables
    private int _currentWaypointIndex = 0;
    private float _waitTime = 10f;
    private float _waitCounter = 0f;
    private bool _isWaiting = true;

    // Enemy attributes
    private float _walkSpeed;

    private bool _debugMode;

    #endregion




    #region Constructors

    // Constructor to initialize references and waypoints
    public ActionPatrol(bool debugMode, Transform transform, Transform[] waypoints, NavMeshAgent agent)
    {
        _transform = transform;
        _animator = transform.GetComponent<Animator>();
        _waypoints = waypoints;
        _agent = agent;
        _walkSpeed = transform.GetComponent<Enemy>().WalkSpeed;
        _aiSensor = transform.GetComponent<AISensor>();
        _enemyAudio = transform.GetComponent<EnemyAudio>();
        _debugMode = debugMode;
    }

    #endregion




    #region Public Methods

    // Evaluate method to determine the state of the node
    public override NodeState Evaluate()
    {

        #region FAILURE CHECKS

        if (GameManager.Instance.IsGamePaused)
        {
            // Return RUNNING to indicate that the action is ongoing
            if (_debugMode) Debug.Log("A - Patrol: RUNNING (game is paused)");
            return NodeState.RUNNING;
        }
        
        if (_aiSensor.IsPlayerInSight && !_aiSensor.IsPlayerHidden)
        {
            // Set state to FAILURE and return
            parent.SetData("target", GameManager.Instance.Player.transform);
            if (_debugMode) Debug.Log("A - Patrol: FAILURE (Player in Sight)");
            return NodeState.FAILURE;
        }
        
        if (HasOtherPatrolDisruptions())
        {
            // Set state to FAILURE and return
            if (_debugMode) Debug.Log("A - Patrol: FAILURE (Last known position, target or noise position exists)");
            return NodeState.FAILURE;
        }
        
        #endregion




        #region ACTION

        if (_isWaiting)
        {
            _animator.SetBool("walk", false);
            _animator.SetBool("run", false);
            _waitCounter += Time.deltaTime;

            // Check if waiting time is over
            if (_waitCounter >= _waitTime)
            {
                _isWaiting = false;
                _animator.SetBool("walk", true);
                _animator.SetBool("run", false);
            }
        }
        else
        {
            _animator.SetBool("run", false);
            _animator.SetBool("walk", true);

            Transform currentWaypoint = _waypoints[_currentWaypointIndex];

            // Check if reached the current waypoint
            if (Vector3.Distance(_transform.position, currentWaypoint.position) < 0.1f)
            {
                _transform.position = new Vector3(currentWaypoint.position.x, _transform.position.y, currentWaypoint.position.z);
                _waitCounter = 0f;
                _isWaiting = true;

                // Switch to the next waypoint
                _currentWaypointIndex = (_currentWaypointIndex + 1) % _waypoints.Length;

                _animator.SetBool("run", false);
                _animator.SetBool("walk", false);
            }
            else
            {
                // Move towards the current waypoint
                _agent.speed = _walkSpeed;
                _agent.SetDestination(currentWaypoint.position);
                _animator.SetBool("walk", true);
                _animator.SetBool("run", false);

                _enemyAudio.VolumeFloorChanger();
            }
        }

        if (_debugMode) Debug.Log("A - Patrol: RUNNING (Patrolling)");
        return NodeState.RUNNING;

        #endregion
    }

    #endregion




    #region Helper Methods

    private bool HasOtherPatrolDisruptions()
    {
        return GetData("target") != null || GetData("lastKnownPosition") != null || GetData("noisePosition") != null;
    }

    #endregion

}