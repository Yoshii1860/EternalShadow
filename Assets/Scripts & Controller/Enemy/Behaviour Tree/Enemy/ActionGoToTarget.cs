using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BehaviorTree;

public class ActionGoToTarget : Node
{
    #region Fields

    // References to components
    private Animator animator;
    private Transform transform;
    private NavMeshAgent agent;

    #endregion

    #region Constructors

    // Constructor to initialize references
    public ActionGoToTarget(Transform transform, NavMeshAgent agent)
    {
        this.transform = transform;
        this.agent = agent;
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
            // Return FAILURE to indicate that the action cannot be executed
            return NodeState.FAILURE;
        }

        // Retrieve target from blackboard
        object obj = GetData("target");

        // Check if there is no target
        if (obj == null)
        {
            // Set state to FAILURE and return
            state = NodeState.FAILURE;
            return state;
        }

        // Get target transform
        Transform target = (Transform)obj;

        // Debug message indicating the target
        Debug.Log("ActionGoToTarget: " + target.name);

        // Set agent speed, set destination, and trigger running animation
        agent.speed = EnemyBT.runSpeed;
        agent.SetDestination(target.position);
        animator.SetBool("Walk", false);
        animator.SetBool("Run", true);

        // Return RUNNING to indicate that the action is ongoing
        state = NodeState.RUNNING;
        return state;
    }

    #endregion
}