using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BehaviorTree;

public class ActionPatrol : Node
{
    Transform transform;
    Animator animator;
    Transform[] waypoints;
    NavMeshAgent agent;

    int currentWaypointIndex = 0;

    float waitTime = 1f;
    float waitCounter = 0f;
    bool isWaiting = true;

    Vector3 lastKnownPosition;
    bool temporaryWaypoint = false;

    public ActionPatrol(Transform transform, Transform[] waypoints, NavMeshAgent agent)
    {
        this.transform = transform;
        animator = transform.GetComponent<Animator>();
        this.waypoints = waypoints;
        this.agent = agent;
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
            Transform wp;
            if (GetData("lastKnownPosition") != null)
            {
                animator.SetBool("Walking", true);
                lastKnownPosition = (Vector3)GetData("lastKnownPosition");
                temporaryWaypoint = true;
            }
            else
            {
                wp = waypoints[currentWaypointIndex];
                lastKnownPosition = wp.position;
            }

            if (temporaryWaypoint)
            {
                if (Vector3.Distance(transform.position, lastKnownPosition) < 0.01f)
                {
                    transform.position = lastKnownPosition;
                    waitCounter = 0f;
                    isWaiting = true;

                    animator.SetBool("Walking", false);
                    temporaryWaypoint = false;
                    ClearData("lastKnownPosition");
                }
                else
                {
                    agent.speed = EnemyBT.walkSpeed;
                    agent.SetDestination(lastKnownPosition);
                }
            }
            else
            {
                if (Vector3.Distance(transform.position, lastKnownPosition) < 0.01f)
                {
                    transform.position = lastKnownPosition;
                    waitCounter = 0f;
                    isWaiting = true;

                    currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
                    animator.SetBool("Walking", false);
                }
                else
                {
                    agent.speed = EnemyBT.walkSpeed;
                    agent.SetDestination(lastKnownPosition);
                }
            }
        }

        state = NodeState.RUNNING;
        return state;
    }
}