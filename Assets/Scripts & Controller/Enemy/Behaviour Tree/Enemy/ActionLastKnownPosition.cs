using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BehaviorTree;

public class ActionLastKnownPosition : Node
{
    #region Fields

    // References to components
    private Transform transform;
    private NavMeshAgent agent;
    private AISensor aiSensor;
    private Animator animator;

    #endregion

    #region Constructors

    // Constructor to initialize references
    public ActionLastKnownPosition(Transform transform, NavMeshAgent agent)
    {
        this.transform = transform;
        this.agent = agent;
        aiSensor = transform.GetComponent<AISensor>();
        animator = transform.GetComponent<Animator>();
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
            return NodeState.RUNNING;
        }

        // Retrieve last known position from blackboard
        object obj = GetData("lastKnownPosition");

        // Check if there is no last known position or if the player is in sight
        if (obj == null || aiSensor.playerInSight)
        {
            // Set state to FAILURE and return
            state = NodeState.FAILURE;
            return state;
        }

        // Get last known position
        Vector3 lastKnownPosition = (Vector3)obj;

        // Check if AI has reached the last known position
        if (Vector3.Distance(transform.position, lastKnownPosition) < 0.1f)
        {
            // Debug message and animation state update
            Debug.Log("ActionLastKnownPosition: Reached last known position!");
            animator.SetBool("Walk", false);

            // Clear the last known position data and set state to SUCCESS
            ClearData("lastKnownPosition");
            state = NodeState.SUCCESS;
            return state;
        }

        // Debug message and movement towards the last known position
        Debug.Log("ActionLastKnownPosition: On the way");
        agent.speed = EnemyBT.walkSpeed;
        agent.SetDestination(lastKnownPosition);
        animator.SetBool("Walk", true);

        // Return RUNNING to indicate that the action is ongoing
        state = NodeState.RUNNING;
        return state;
    }

    #endregion
}