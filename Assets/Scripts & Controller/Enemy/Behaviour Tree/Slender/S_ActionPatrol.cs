using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BehaviorTree;

public class S_ActionPatrol : Node
{
    #region Fields

    // References to components
    private Transform transform;
    private Animator animator;
    private Transform[] waypoints;
    private NavMeshAgent agent;
    private Enemy enemy;
    private AISensor sensor;

    // Patrol-related variables
    private int currentWaypointIndex = 0;
    private float waitTime = 1f;
    private float waitCounter = 0f;
    private bool isWaiting = true;

    // Debug mode flag
    private bool debugMode;

    #endregion

    #region Constructors

    // Constructor to initialize references and waypoints
    public S_ActionPatrol(bool debugMode, Transform transform, Transform[] waypoints, NavMeshAgent agent)
    {
        this.transform = transform;
        this.animator = transform.GetComponent<Animator>();
        this.waypoints = waypoints;
        this.agent = agent;
        enemy = transform.GetComponent<Enemy>();
        sensor = transform.GetComponent<AISensor>();
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
            if (debugMode) Debug.Log("A - Patrol: RUNNING (game is paused)");
            state = NodeState.RUNNING;
            return state;
        }

        if (sensor.playerInSight)
        {
            // Set state to FAILURE and return
            parent.parent.SetData("target", GameManager.Instance.player.transform);
            if (debugMode) Debug.Log("A - Patrol: FAILURE (Player in Sight)");
            state = NodeState.FAILURE;
            return state;
        }

        // Check if there is a last known position, if yes, abort patrol
        object lastKnownPos = GetData("lastKnownPosition");
        if (lastKnownPos != null)
        {
            // Set state to FAILURE and return
            if (debugMode) Debug.Log("A - Patrol: FAILURE (lastKnownPosition != null)");
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
                agent.enabled = true;
                animator.SetBool("walk", true);
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
                animator.SetBool("run", false);
                animator.SetBool("walk", false);

                agent.enabled = false;
            }
            else
            {
                /*if (enemy.enteredDoor)
                {
                    parent.SetData("destination", currentWaypoint.position);
                    animator.SetBool("walk", false);
                    animator.SetBool("run", false);
                    return NodeState.FAILURE;
                }*/
                // Move towards the current waypoint
                agent.speed = SlenderBT.walkSpeed;
                agent.SetDestination(currentWaypoint.position);
                animator.SetBool("walk", true);

                AudioManager.Instance.ToggleSlenderAudio(transform.gameObject, false);
            }
        }

        // Return RUNNING to indicate that the action is ongoing
        if (debugMode) Debug.Log("A - Patrol: RUNNING (Patrolling)");
        state = NodeState.RUNNING;
        return state;
    }

    #endregion
}