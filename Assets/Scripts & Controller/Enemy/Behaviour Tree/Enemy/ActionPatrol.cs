using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BehaviorTree;

public class ActionPatrol : Node
{
    #region Fields

    // References to components
    private Transform transform;
    private Animator animator;
    private Transform[] waypoints;
    private NavMeshAgent agent;

    // Patrol-related variables
    private int currentWaypointIndex = 0;
    private float waitTime = 1f;
    private float waitCounter = 0f;
    private bool isWaiting = true;

    #endregion

    #region Constructors

    // Constructor to initialize references and waypoints
    public ActionPatrol(Transform transform, Transform[] waypoints, NavMeshAgent agent)
    {
        this.transform = transform;
        this.animator = transform.GetComponent<Animator>();
        this.waypoints = waypoints;
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

        if (waypoints[0] == null)
        {
            // Set state to FAILURE and return
            state = NodeState.FAILURE;
            return state;
        }

        // Check if there is a last known position, if yes, abort patrol
        object lastKnownPos = GetData("lastKnownPosition");
        if (lastKnownPos != null)
        {
            // Set state to FAILURE and return
            state = NodeState.FAILURE;
            return state;
        }

        // Check if waiting
        if (isWaiting)
        {
            waitCounter += Time.deltaTime;

            // Check if waiting time is over
            if (waitCounter >= waitTime)
            {
                isWaiting = false;
                animator.SetBool("Walk", true);
            }
        }
        else
        {
            Transform currentWaypoint = waypoints[currentWaypointIndex];

            // Check if reached the current waypoint
            if (Vector3.Distance(transform.position, currentWaypoint.position) < 0.1f)
            {
                transform.position = currentWaypoint.position;
                waitCounter = 0f;
                isWaiting = true;

                // Switch to the next waypoint in a loop
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;

                // Reset animation states
                animator.SetBool("Run", false);
                animator.SetBool("Walk", false);
            }
            else
            {
                // Move towards the current waypoint
                agent.speed = EnemyBT.walkSpeed;
                agent.SetDestination(currentWaypoint.position);
                animator.SetBool("Walk", true);
            }
        }

        // Return RUNNING to indicate that the action is ongoing
        state = NodeState.RUNNING;
        return state;
    }

    #endregion
}