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
                transform.position = Vector3.MoveTowards(transform.position, wp.position, EnemyBT.speed * Time.deltaTime);
                transform.LookAt(wp.position);
            }
        }

        state = NodeState.RUNNING;
        return state;
    }
}
