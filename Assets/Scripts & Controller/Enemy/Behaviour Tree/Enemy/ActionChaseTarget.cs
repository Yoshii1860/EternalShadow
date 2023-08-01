using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BehaviorTree;

public class ActionChaseTarget : Node
{
    Transform transform;
    NavMeshAgent agent;

    public ActionChaseTarget(Transform transform, NavMeshAgent agent)
    {
        this.transform = transform;
        this.agent = agent;
    }

    public override NodeState Evaluate()
    {
        Transform target = (Transform)GetData("target");

        if(target == null)
        {
            state = NodeState.FAILURE;
            return state;
        }

        agent.speed = EnemyBT.runSpeed;
        agent.SetDestination(target.position);

        state = NodeState.RUNNING;
        return state;
    }
}