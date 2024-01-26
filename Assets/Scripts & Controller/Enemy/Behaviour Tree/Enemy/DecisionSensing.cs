using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class DecisionSensing : Node
{
    #region Fields

    // Maximum range for chasing the player, consistent with AISensor.cs
    float maxChaseRange = 15f;

    // References to components
    Transform transform;
    Animator animator;
    AISensor aiSensor;

    #endregion

    #region Constructors

    // Constructor to initialize references to components
    public DecisionSensing(Transform transform)
    {
        this.transform = transform;
        animator = transform.GetComponent<Animator>();
        aiSensor = transform.GetComponent<AISensor>();
    }

    #endregion

    #region Public Methods

    // Evaluate method to determine the state of the node
    public override NodeState Evaluate()
    {
        // Check if the game is paused
        if (GameManager.Instance.isPaused)
        {
            // Return FAILURE to indicate that the decision cannot be made
            return NodeState.FAILURE;
        }

        #region Debug Mode

        // Check if the game is in "No Attack Mode" for debugging
        if (GameManager.Instance.noAttackMode)
        {
            // Return FAILURE as the decision is disabled in debug mode
            state = NodeState.FAILURE;
            return state;
        }

        #endregion

        // Retrieve the target and last known position from the blackboard
        object obj = GetData("target");
        object obj2 = GetData("lastKnownPosition");

        // If no target is set
        if (obj == null)
        {
            // Check if the player is in sight
            if (aiSensor.playerInSight)
            {
                Debug.Log("DecisionSensing: Player in sight!");
                // Clear last known position, set the player as the target, and return SUCCESS
                ClearData("lastKnownPosition");
                parent.parent.SetData("target", GameManager.Instance.player.transform);

                state = NodeState.SUCCESS;
                return state;
            }
            // If there's no target and no last known position, return FAILURE
            else if (obj2 == null)
            {
                state = NodeState.FAILURE;
                return state;
            }
        }
        // If a target is set
        else if (obj != null)
        {
            Transform target = (Transform)obj;
            float distanceToTarget = Vector3.Distance(transform.position, target.position);

            // If the target is out of the maximum chase range
            if (distanceToTarget > maxChaseRange)
            {
                Debug.Log("DecisionSensing: Player out of range!");
                // Save the last known position, clear the target, and return FAILURE
                Vector3 lastKnownPosition = GameManager.Instance.player.transform.position;
                parent.parent.SetData("lastKnownPosition", lastKnownPosition);
                ClearData("target");

                state = NodeState.FAILURE;
                return state;
            }
            // If the target is within range, return SUCCESS
            else
            {
                state = NodeState.SUCCESS;
                return state;
            }
        }

        // Default case, return FAILURE
        state = NodeState.FAILURE;
        return state;
    }

    #endregion
}