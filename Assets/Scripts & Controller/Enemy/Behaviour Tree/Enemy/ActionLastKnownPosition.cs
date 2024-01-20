using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BehaviorTree;

public class ActionLastKnownPosition : Node
{
    Transform transform;
    NavMeshAgent agent;
    AISensor aiSensor;
    Animator animator;

    public ActionLastKnownPosition(Transform transform, NavMeshAgent agent)
    {
        this.transform = transform;
        this.agent = agent;
        aiSensor = transform.GetComponent<AISensor>();
        animator = transform.GetComponent<Animator>();
    }

    public override NodeState Evaluate()
    {
        if (GameManager.Instance.isPaused) return NodeState.RUNNING;

        object obj = GetData("lastKnownPosition");

        if (obj == null || aiSensor.playerInSight)
        {
            state = NodeState.FAILURE;
            return state;
        }

        Vector3 lastKnownPosition = (Vector3)obj;

        if (Vector3.Distance(transform.position, lastKnownPosition) < 0.1f)
        {
            Debug.Log("ActionLastKnownPosition: Reached last known position!");
            animator.SetBool("Walk", false);
            ClearData("lastKnownPosition");
            state = NodeState.SUCCESS;
            return state;
        }

        Debug.Log("ActionLastKnownPosition: On the way");
        agent.speed = EnemyBT.walkSpeed;
        agent.SetDestination(lastKnownPosition);
        animator.SetBool("Walk", true);

        state = NodeState.RUNNING;
        return state;
    }
}