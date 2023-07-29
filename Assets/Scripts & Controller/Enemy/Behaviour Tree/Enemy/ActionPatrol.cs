using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class ActionPatrol : Node
{
    Transform transform;
    Animator animator;
    Transform[] waypoints;

    int currentWaypointIndex = 0;

    float waitTime = 1f;
    float waitCounter = 0f;
    bool isWaiting = true;

    public ActionPatrol(Transform transform, Transform[] waypoints)
    {
        this.transform = transform;
        animator = transform.GetComponent<Animator>();
        this.waypoints = waypoints;
    }

    public override NodeState Evaluate()
    {
        if (isWaiting)
        {
            waitCounter += Time.deltaTime;
            if (waitCounter >= waitTime) 
            {
                isWaiting = false;
                animator.SetBool("Walking", true);
            }
        }
        else
        {
            Transform wp = waypoints[currentWaypointIndex];
            if (Vector3.Distance(transform.position, wp.position) < 0.01f)
            {
                transform.position = wp.position;
                waitCounter = 0f;
                isWaiting = true;

                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
                animator.SetBool("Walking", false);
            }
            else
            {
                // Move the enemy towards the waypoint
                transform.position = Vector3.MoveTowards(transform.position, wp.position, EnemyBT.speed * Time.deltaTime);

                // Calculate the direction from the enemy to the waypoint
                Vector3 directionToWaypoint = wp.position - transform.position;
                directionToWaypoint.y = 0f; // Set the Y-component to 0 to ensure the enemy does not tilt up or down.

                if (directionToWaypoint != Vector3.zero)
                {
                    // Rotate the enemy to face the waypoint direction
                    Quaternion targetRotation = Quaternion.LookRotation(directionToWaypoint);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, EnemyBT.rotationSpeed * Time.deltaTime);
                }
            }
        }

        state = NodeState.RUNNING;
        return state;
    }
}