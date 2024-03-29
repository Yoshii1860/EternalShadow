using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class DecisionLastKnownPosition : Node
{
    #region Fields

    // Reference to the transform component
    private Transform transform;
    private AISensor aiSensor;

    private bool debugMode;

    #endregion

    #region Constructors

    // Constructor to initialize the transform reference
    public DecisionLastKnownPosition(bool debugMode, Transform transform)
    {
        this.transform = transform;
        aiSensor = transform.GetComponent<AISensor>();
        this.debugMode = debugMode;
    }

    #endregion

    #region Public Methods

    // Evaluate method to determine the state of the node
    public override NodeState Evaluate()
    {

        ////////////////////////////////////////////////////////////////////////
        // PAUSE GAME
        ////////////////////////////////////////////////////////////////////////
        if (GameManager.Instance.isPaused)
        {
            // Return RUNNING to indicate that the decision is inconclusive
            if (debugMode) Debug.Log("D - LastKnownPosition: RUNNING (Game is paused)");
            state = NodeState.RUNNING;
            return state;
        }
        ////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////
        // FAILURE CHECKS
        ////////////////////////////////////////////////////////////////////////
        object obj = GetData("target");

        if (obj != null)
        {
            ClearData("lastKnownPosition");
            if (debugMode) Debug.Log("D - LastKnownPosition: FAILURE (Target exists)");
            state = NodeState.FAILURE;
            return state;
        }
        else if (aiSensor.hidden)
        {
            ClearData("lastKnownPosition");
            if (debugMode) Debug.Log("D - LastKnownPosition: FAILURE (Hidden)");
            state = NodeState.FAILURE;
            return state;
        }
        else if (aiSensor.playerInSight)
        {
            ClearData("lastKnownPosition");
            parent.parent.SetData("target", GameManager.Instance.player.transform);
            if (debugMode) Debug.Log("D - LastKnownPosition: FAILURE (Player in sight)");
            state = NodeState.FAILURE;
            return state;
        }
        ////////////////////////////////////////////////////////////////////////

        // Retrieve the last known position from the blackboard
        object objPos = GetData("lastKnownPosition");

        // If the last known position exists, set state to SUCCESS
        if (objPos != null)
        {
            if (debugMode) Debug.Log("D - LastKnownPosition: SUCCESS (Last Known Position)");
            state = NodeState.SUCCESS;
            return state;
        }

        // If there is no last known position, set state to FAILURE
        if (debugMode) Debug.Log("D - LastKnownPosition: FAILURE (No Last Known Position)");
        state = NodeState.FAILURE;
        return state;
    }

    #endregion
}