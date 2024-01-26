using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BehaviorTree;

public class ActionChaseTarget : Node
{
    #region Fields

    // References to components
    private Animator animator;
    private NavMeshAgent agent;

    // Timer variables for chasing
    private float chaseTimer = 0f;
    private float chaseDuration = 10f; // Same as in Enemy.cs

    #endregion

    #region Constructors

    // Constructor to initialize references
    public ActionChaseTarget(Transform transform, NavMeshAgent agent)
    {
        animator = transform.GetComponent<Animator>();
        this.agent = agent;
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

        // Retrieve the target from the blackboard
        object obj = GetData("target");

        if(obj == null)
        {
            // If there is no target, set state to FAILURE and return
            state = NodeState.FAILURE;
            return state;
        }

        Transform target = (Transform)obj;

        // Debug message indicating that the AI is chasing the target
        Debug.Log("ChaseTarget: Chasing!");

        // Adjust agent speed and set destination for chasing
        agent.speed = EnemyBT.runSpeed;
        agent.SetDestination(target.position);
        animator.SetBool("Run", true);

        // Update the chase timer
        chaseTimer += Time.deltaTime;

        // Check if the chase duration is over
        if (chaseTimer >= chaseDuration)
        {
            // Debug message indicating that the chase duration is over
            Debug.Log("ChaseTarget: Time over!");

            // Clear the target data, reset timer, set state to SUCCESS, and stop running animation
            ClearData("target");
            chaseTimer = 0f;
            state = NodeState.SUCCESS;
            animator.SetBool("Run", false);
            return state;
        }

        // If the chase is still ongoing, return RUNNING
        state = NodeState.RUNNING;
        return state;
    }

    #endregion
}