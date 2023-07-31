using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BehaviorTree;

public class ActionGoToTarget : Node
{
    Transform transform;
    NavMeshAgent agent;

    public ActionGoToTarget(Transform transform, NavMeshAgent agent)
    {
        this.transform = transform;
        this.agent = agent;
    }

    public override NodeState Evaluate()
    {
        Transform target = (Transform)GetData("target");

        agent.speed = EnemyBT.runSpeed;
        agent.SetDestination(target.position);

        state = NodeState.RUNNING;
        return state;
    }
}