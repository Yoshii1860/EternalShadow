using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BehaviorTree;

public class ActionChaseTarget : Node
{
    Animator animator;
    NavMeshAgent agent;
    float chaseTimer = 0f;
    float chaseDuration = 10f; // same as in Enemy.cs

    public ActionChaseTarget(Transform transform, NavMeshAgent agent)
    {        
        animator = transform.GetComponent<Animator>();
        this.agent = agent;
    }

    public override NodeState Evaluate()
    {
        if (GameManager.Instance.isPaused) return NodeState.RUNNING;
        
        object obj = GetData("target");

        if(obj == null)
        {
            state = NodeState.FAILURE;
            return state;
        }

        Transform target = (Transform)obj;

        Debug.Log("ChaseTarget: Chasing!");
        agent.speed = EnemyBT.runSpeed;
        agent.SetDestination(target.position);
        animator.SetBool("Run", true);

        chaseTimer += Time.deltaTime;
        if (chaseTimer >= chaseDuration)
        {
            Debug.Log("ChaseTarget: Time over!");
            ClearData("target");
            chaseTimer = 0f;
            state = NodeState.SUCCESS;
            animator.SetBool("Run", false);
            return state;
        }

        state = NodeState.RUNNING;
        return state;
    }
}