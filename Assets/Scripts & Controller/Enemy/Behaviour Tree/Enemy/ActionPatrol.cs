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

    public ActionPatrol(Transform transform, Transform[] waypoints, NavMeshAgent agent)
    {
        this.transform = transform;
        animator = transform.GetComponent<Animator>();
        this.waypoints = waypoints;
        this.agent = agent;
    }

    public override NodeState Evaluate()
    {
        if (GameManager.Instance.isPaused) return NodeState.RUNNING;

        object obj = GetData("lastKnownPosition");
        if (obj != null)
        {
            state = NodeState.FAILURE;
            return state;
        }

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
            wp = waypoints[currentWaypointIndex];
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
                agent.speed = EnemyBT.walkSpeed;
                agent.SetDestination(wp.position);
            }
        }

        state = NodeState.RUNNING;
        return state;
    }
}