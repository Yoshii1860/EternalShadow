using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BehaviorTree;

public class ActionGoToTarget : Node
{
    Animator animator;
    Transform transform;
    NavMeshAgent agent;

    public ActionGoToTarget(Transform transform, NavMeshAgent agent)
    {
        this.transform = transform;
        this.agent = agent;
        animator = transform.GetComponent<Animator>();
    }

    public override NodeState Evaluate()
    {
        if (GameManager.Instance.isPaused) return NodeState.FAILURE;

        object obj = GetData("target");

        if (obj == null)
        {
            state = NodeState.FAILURE;
            return state;
        }
        
        Transform target = (Transform)obj;

        Debug.Log("ActionGoToTarget: " + target.name);

        agent.speed = EnemyBT.runSpeed;
        agent.SetDestination(target.position);
        animator.SetBool("Walk", false);
        animator.SetBool("Run", true);

        state = NodeState.RUNNING;
        return state;
    }
}