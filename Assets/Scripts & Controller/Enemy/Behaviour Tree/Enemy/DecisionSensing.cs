using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class DecisionSensing : Node
{
    #region Fields

    // References to components
    Transform _transform;
    AISensor _aiSensor;

    // Enemy attributes
    float _maxChaseRange;

    private bool _debugMode;

    #endregion




    #region Constructors

    // Constructor to initialize references to components
    public DecisionSensing(bool debugMode, Transform transform)
    {
        _transform = transform;
        _aiSensor = transform.GetComponent<AISensor>();
        _maxChaseRange = transform.GetComponent<Enemy>().MaxChaseRange;
        _debugMode = debugMode;
    }

    #endregion




    #region Public Methods

    public override NodeState Evaluate()
    {

        #region FAILURE CHECKS

        if (GameManager.Instance.IsGamePaused)
        {
            if (_debugMode) Debug.Log("D - Sensing: FAILURE (Game is paused)");
            return NodeState.FAILURE;
        }

        if (GameManager.Instance.NoAttackMode)
        {
            if (_debugMode) Debug.Log("D - Sensing: FAILURE (No Attack Mode)");
            return NodeState.FAILURE;
        }

        if (_aiSensor.IsPlayerHidden)
        {
            ClearData("target");
            ClearData("lastKnownPosition");
            if (_debugMode) Debug.Log("D - Sensing: FAILURE (AISensor Hidden)");
            return NodeState.FAILURE;
        }
        
        #endregion




        #region DECISION

        object obj = GetData("target");
        if (obj == null)
        {
            if (_aiSensor.IsPlayerInSight)
            {
                ClearData("lastKnownPosition");
                parent.parent.SetData("target", GameManager.Instance.Player.transform);

                if (_debugMode) Debug.Log("D - Sensing: SUCCESS (Player in Sight)");
                return NodeState.SUCCESS;
            }
            else if (GetData("lastKnownPosition") == null)
            {
                if (_debugMode) Debug.Log("D - Sensing: FAILURE (No Target or Last Known Position)");
                return NodeState.FAILURE;
            }
        }
        else if (obj != null)
        {
            Transform target = (Transform)obj;
            float distanceToTarget = Vector3.Distance(_transform.position, target.position);

            if (distanceToTarget > _maxChaseRange)
            {
                Vector3 lastKnownPosition = GameManager.Instance.Player.transform.position;
                parent.parent.SetData("lastKnownPosition", lastKnownPosition);
                ClearData("target");

                if (_debugMode) Debug.Log("D - Sensing: FAILURE (Target Out of Range)");
                return NodeState.FAILURE;
            }
            // If the target is within range, return SUCCESS
            else 
            {
                if (_debugMode) Debug.Log("D - Sensing: SUCCESS (Target in Range)");
                return NodeState.SUCCESS;
            }
        }

        if (_debugMode) Debug.Log("D - Sensing: FAILURE (Default)");
        return NodeState.FAILURE;

        #endregion
    }

    #endregion
}