using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class DecisionLastKnownPosition : Node
{
    #region Fields

    // References
    private AISensor _aiSensor;

    private bool _debugMode;

    #endregion




    #region Constructors

    public DecisionLastKnownPosition(bool debugMode, Transform transform)
    {
        _aiSensor = transform.GetComponent<AISensor>();
        _debugMode = debugMode;
    }

    #endregion




    #region Public Methods

    public override NodeState Evaluate()
    {

        #region FAILURE CHECKS

        if (GameManager.Instance.IsGamePaused)
        {
            if (_debugMode) Debug.Log("D - LastKnownPosition: RUNNING (Game is paused)");
            return NodeState.RUNNING;
        }

        if (GetData("target") != null || _aiSensor.IsPlayerHidden)
        {
            ClearData("lastKnownPosition");
            if (_debugMode) Debug.Log("D - LastKnownPosition: FAILURE (Target exists or player is hidden)");
            return NodeState.FAILURE;
        }

        if (_aiSensor.IsPlayerInSight)
        {
            ClearData("lastKnownPosition");
            parent.parent.SetData("target", GameManager.Instance.Player.transform);
            if (_debugMode) Debug.Log("D - LastKnownPosition: FAILURE (Player in sight)");
            return NodeState.FAILURE;
        }
        
        #endregion




        #region DECISION

        object objPos = GetData("lastKnownPosition");

        if (objPos != null)
        {
            if (_debugMode) Debug.Log("D - LastKnownPosition: SUCCESS (Last Known Position)");
            return NodeState.SUCCESS;
        }

        if (_debugMode) Debug.Log("D - LastKnownPosition: FAILURE (No Last Known Position)");
        return NodeState.FAILURE;

        #endregion
    }

    #endregion
}