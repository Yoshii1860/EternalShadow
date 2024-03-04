using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BehaviorTree;

public class S_ActionChaseTarget : Node
{
    #region Fields

    // References to components
    private Animator animator;
    private NavMeshAgent agent;
    private Enemy enemy;
    private Transform transform;

    // Timer variables for chasing
    private float chaseTimer = 0f;
    private float chaseDuration = 10f; // Same as in Enemy.cs

    // Debug mode flag
    private bool debugMode;

    #endregion

    #region Constructors

    // Constructor to initialize references
    public S_ActionChaseTarget(bool debugMode, Transform transform, NavMeshAgent agent)
    {
        this.transform = transform;
        animator = transform.GetComponent<Animator>();
        this.agent = agent;
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
            if (debugMode) Debug.Log("A - ChaseTarget: RUNNING (game is paused)");
            state = NodeState.RUNNING;
            return state;
        }

        // Retrieve the target from the blackboard
        object obj = GetData("target");

        if(obj == null)
        {
            // If there is no target, set state to FAILURE and return
            if (debugMode) Debug.Log("A - ChaseTarget: FAILURE (target = null)");
            state = NodeState.FAILURE;
            return state;
        }

        Transform target = (Transform)obj;

        // Adjust agent speed and set destination for chasing
        /*if (enemy.enteredDoor)
        {
            parent.parent.SetData("destination", target.position);
        }*/
        agent.speed = EnemyBT.runSpeed;
        agent.SetDestination(target.position);
        animator.SetBool("run", true);

        AudioManager.Instance.ToggleSlenderAudio(transform.gameObject, true);

        // Update the chase timer
        chaseTimer += Time.deltaTime;

        // Check if the chase duration is over
        if (chaseTimer >= chaseDuration)
        {
            // Clear the target data, reset timer, set state to SUCCESS, and stop running animation
            ClearData("target");
            chaseTimer = 0f;
            animator.SetBool("run", false);
            if (debugMode) Debug.Log("A - ChaseTarget: SUCCESS (chase duration over)");
            state = NodeState.SUCCESS;
            return state;
        }

        // If the chase is still ongoing, return RUNNING
        if (debugMode) Debug.Log("A - ChaseTarget: RUNNING (chasing)");
        state = NodeState.RUNNING;
        return state;
    }

    #endregion
}