using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BehaviorTree;

public class S_ActionLastKnownPosition : Node
{
    #region Fields

    // References to components
    private Transform transform;
    private NavMeshAgent agent;
    private Animator animator;
    private Enemy enemy;

    // Debug mode flag
    private bool debugMode;

    #endregion

    #region Constructors

    // Constructor to initialize references
    public S_ActionLastKnownPosition(bool debugMode, Transform transform, NavMeshAgent agent)
    {
        this.transform = transform;
        this.agent = agent;
        animator = transform.GetComponent<Animator>();
        enemy = transform.GetComponent<Enemy>();
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
            // Return RUNNING to indicate that the action is ongoing
            if (debugMode) Debug.Log("A - LastKnownPosition: RUNNING (game is paused)");
            state = NodeState.RUNNING;
            return state;
        }

        // Retrieve last known position from blackboard
        object obj = GetData("lastKnownPosition");

        // Check if there is no last known position or if the player is in sight
        if (obj == null)
        {
            // Set state to FAILURE and return
            if (debugMode) Debug.Log("A - LastKnownPosition: FAILURE (lastKnownPosition = null)");
            state = NodeState.FAILURE;
            return state;
        }

        // Get last known position
        Vector3 lastKnownPosition = (Vector3)obj;

        // Check if AI has reached the last known position
        if (Vector3.Distance(transform.position, lastKnownPosition) < 0.1f)
        {
            // Debug message and animation state update
            animator.SetBool("walk", false);

            // Clear the last known position data and set state to SUCCESS
            ClearData("lastKnownPosition");
            if (debugMode) Debug.Log("A - LastKnownPosition: SUCCESS (Reached Last Known Position)");
            state = NodeState.SUCCESS;
            return state;
        }

        /*if (enemy.enteredDoor)
        {
            parent.parent.SetData("destination", lastKnownPosition);
            state = NodeState.FAILURE;
            return state;
        }*/

        agent.speed = EnemyBT.walkSpeed;
        agent.SetDestination(lastKnownPosition);
        animator.SetBool("walk", true);

        AudioManager.Instance.ToggleSlenderAudio(transform.gameObject, false);

        // Return RUNNING to indicate that the action is ongoing
        if (debugMode) Debug.Log("A - LastKnownPosition: RUNNING (Moving to Last Known Position)");
        state = NodeState.RUNNING;
        return state;
    }

    #endregion
}