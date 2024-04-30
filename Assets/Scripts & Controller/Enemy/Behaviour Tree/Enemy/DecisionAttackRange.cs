using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class DecisionAttackRange : Node
{
    #region Fields

    // References to components
    private Transform _transform;
    private Animator _animator;
    private AISensor _aiSensor;

    // Enemy attributes
    private float _attackRange;

    // Debug mode flag
    private bool _debugMode;

    #endregion




    #region Constructors

    // Constructor to initialize references
    public DecisionAttackRange(bool debugMode, Transform transform)
    {
        _transform = transform;
        _animator = transform.GetComponent<Animator>();
        _debugMode = debugMode;
        _aiSensor = transform.GetComponent<AISensor>();
        _attackRange = transform.GetComponent<Enemy>().AttackRange;
    }

    #endregion




    #region Public Methods

    // Evaluate method to determine the state of the node
    public override NodeState Evaluate()
    {

        #region FAILURE CHECKS
    
        if (GameManager.Instance.IsGamePaused)
        {
            // Return RUNNING to indicate that the decision is ongoing
            if (_debugMode) Debug.Log("D - AttackRange: RUNNING (Paused)");
            return NodeState.RUNNING;
        }

        object obj = GetData("target");
        if (obj == null || _aiSensor.IsPlayerHidden)
        {
            if (_debugMode) Debug.Log("D - AttackRange: FAILURE (No Target)");
            return NodeState.FAILURE;
        }
        
        #endregion




        #region DECISION MAKING

        // Calculate the distance between the agent and the target
        Transform target = (Transform)obj;
        float distanceToTarget = Vector3.Distance(_transform.position, target.position);

        // Check if the distance is within the attack range
        if (distanceToTarget <= _attackRange)
        {
            _animator.SetBool("walk", false);
            _animator.SetBool("run", false);
            if (_debugMode) Debug.Log("D - AttackRange: SUCCESS (Within Attack Range)");
            return NodeState.SUCCESS;
        }

        if (_debugMode) Debug.Log("D - AttackRange: FAILURE (Not Within Attack Range)");
        return NodeState.FAILURE;

        #endregion
    }

    #endregion
}