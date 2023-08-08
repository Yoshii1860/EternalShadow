using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BehaviorTree;

public class ActionChaseTarget : Node
{
    NavMeshAgent agent;
    float chaseTimer = 0f;
    float chaseDuration = 10f; // same as in Enemy.cs

    public ActionChaseTarget(NavMeshAgent agent)
    {        
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

        chaseTimer += Time.deltaTime;
        if (chaseTimer >= chaseDuration)
        {
            Debug.Log("ChaseTarget: Time over!");
            ClearData("target");
            chaseTimer = 0f;
            state = NodeState.SUCCESS;
            return state;
        }

        state = NodeState.RUNNING;
        return state;
    }
}