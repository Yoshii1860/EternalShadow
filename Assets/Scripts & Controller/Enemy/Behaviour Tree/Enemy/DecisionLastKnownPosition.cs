using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class DecisionLastKnownPosition : Node
{
    #region Fields

    // Reference to the transform component
    private Transform transform;

    #endregion

    #region Constructors

    // Constructor to initialize the transform reference
    public DecisionLastKnownPosition(Transform transform)
    {
        this.transform = transform;
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
            return NodeState.RUNNING;
        }

        // Retrieve the last known position from the blackboard
        object obj = GetData("lastKnownPosition");

        // If the last known position exists, set state to SUCCESS
        if (obj != null)
        {
            state = NodeState.SUCCESS;
            return state;
        }

        // If there is no last known position, set state to FAILURE
        state = NodeState.FAILURE;
        return state;
    }

    #endregion
}