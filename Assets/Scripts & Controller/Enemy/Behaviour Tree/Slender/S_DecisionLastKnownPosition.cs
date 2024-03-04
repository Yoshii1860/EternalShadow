using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class S_DecisionLastKnownPosition : Node
{
    #region Fields

    // Reference to the transform component
    private Transform transform;
    private AISensor aiSensor;

    private bool debugMode;

    #endregion

    #region Constructors

    // Constructor to initialize the transform reference
    public S_DecisionLastKnownPosition(bool debugMode, Transform transform)
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
        // Check if the game is paused
        if (GameManager.Instance.isPaused)
        {
            // Return RUNNING to indicate that the decision is inconclusive
            if (debugMode) Debug.Log("D - LastKnownPosition: RUNNING (Game is paused)");
            state = NodeState.RUNNING;
            return state;
        }

        if (aiSensor.hidden)
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

        // Retrieve the last known position from the blackboard
        object obj = GetData("lastKnownPosition");

        // If the last known position exists, set state to SUCCESS
        if (obj != null)
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