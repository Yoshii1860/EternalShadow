using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class S_DecisionSensing : Node
{
    #region Fields

    // Maximum range for chasing the player, consistent with AISensor.cs
    float maxChaseRange = 15f;

    // References to components
    Transform transform;
    Animator animator;
    AISensor aiSensor;

    private bool debugMode;

    #endregion

    #region Constructors

    // Constructor to initialize references to components
    public S_DecisionSensing(bool debugMode, Transform transform)
    {
        this.transform = transform;
        animator = transform.GetComponent<Animator>();
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
            // Return FAILURE to indicate that the decision cannot be made
            if (debugMode) Debug.Log("D - Sensing: FAILURE (Game is paused)");
            state = NodeState.FAILURE;
            return state;
        }
        ////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////
        // No Attack Mode
        ////////////////////////////////////////////////////////////////////////
        if (GameManager.Instance.noAttackMode)
        {
            // Return FAILURE as the decision is disabled in debug mode
            if (debugMode) Debug.Log("D - Sensing: FAILURE (No Attack Mode)");
            state = NodeState.FAILURE;
            return state;
        }
        ////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////
        // FAILURE CHECKS
        ////////////////////////////////////////////////////////////////////////
        if (aiSensor.hidden)
        {
            // Clear the target and last known position, and return FAILURE
            ClearData("target");
            ClearData("lastKnownPosition");
            if (debugMode) Debug.Log("D - Sensing: FAILURE (AISensor Hidden)");
            state = NodeState.FAILURE;
            return state;
        }
        ////////////////////////////////////////////////////////////////////////


        // Retrieve the target and last known position from the blackboard
        object obj = GetData("target");
        object obj2 = GetData("lastKnownPosition");

        // If no target is set
        if (obj == null)
        {
            // Check if the player is in sight
            if (aiSensor.playerInSight)
            {
                // Clear last known position, set the player as the target, and return SUCCESS
                ClearData("lastKnownPosition");
                parent.parent.SetData("target", GameManager.Instance.player.transform);

                if (debugMode) Debug.Log("D - Sensing: SUCCESS (Player in Sight)");
                state = NodeState.SUCCESS;
                return state;
            }
            // If there's no target and no last known position, return FAILURE
            else if (obj2 == null)
            {
                if (debugMode) Debug.Log("D - Sensing: FAILURE (No Target or Last Known Position)");
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
                // Save the last known position, clear the target, and return FAILURE
                Vector3 lastKnownPosition = GameManager.Instance.player.transform.position;
                parent.parent.SetData("lastKnownPosition", lastKnownPosition);
                ClearData("target");

                if (debugMode) Debug.Log("D - Sensing: FAILURE (Target Out of Range)");
                state = NodeState.FAILURE;
                return state;
            }
            // If the target is within range, return SUCCESS
            else
            {
                if (debugMode) Debug.Log("D - Sensing: SUCCESS (Target in Range)");
                state = NodeState.SUCCESS;
                return state;
            }
        }

        // Default case, return FAILURE
        if (debugMode) Debug.Log("D - Sensing: FAILURE (Default)");
        state = NodeState.FAILURE;
        return state;
    }

    #endregion
}