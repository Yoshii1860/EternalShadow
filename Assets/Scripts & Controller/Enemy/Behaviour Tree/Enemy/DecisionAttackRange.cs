using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class DecisionAttackRange : Node
{
    #region Fields

    // References to components
    private Transform transform;
    private Animator animator;

    #endregion

    #region Constructors

    // Constructor to initialize references
    public DecisionAttackRange(Transform transform)
    {
        this.transform = transform;
        this.animator = transform.GetComponent<Animator>();
    }

    #endregion

    #region Public Methods

    // Evaluate method to determine the state of the node
    public override NodeState Evaluate()
    {
        // Check if the game is paused
        if (GameManager.Instance.isPaused)
        {
            // Return RUNNING to indicate that the decision is ongoing
            return NodeState.RUNNING;
        }

        // Retrieve the target from the blackboard
        object obj = GetData("target");

        // Check if the target is null
        if (obj == null)
        {
            // Set state to FAILURE and return
            state = NodeState.FAILURE;
            return state;
        }

        // Calculate the distance between the agent and the target
        Transform target = (Transform)obj;
        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        // Check if the distance is within the attack range
        if (distanceToTarget <= EnemyBT.attackRange)
        {
            Debug.Log("DecisionAttackRange: Within Attack Range");
            state = NodeState.SUCCESS;
            return state;
        }

        // If not within attack range, update animator and set state to FAILURE
        animator.SetBool("Attack", false);
        state = NodeState.FAILURE;
        return state;
    }

    #endregion
}