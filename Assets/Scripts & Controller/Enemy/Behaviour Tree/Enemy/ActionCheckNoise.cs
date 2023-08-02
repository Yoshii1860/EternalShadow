using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BehaviorTree;

public class ActionCheckNoise : Node
{
    Transform transform;
    UnityEngine.AI.NavMeshAgent agent;

    public ActionCheckNoise(Transform transform, UnityEngine.AI.NavMeshAgent agent)
    {
        this.transform = transform;
        this.agent = agent;
    }

    public override NodeState Evaluate()
    {
        agent.speed = EnemyBT.walkSpeed;
        Vector3 targetPosition = (Vector3)GetData("lastKnownPosition");
        agent.SetDestination(targetPosition);

        state = NodeState.RUNNING;
        return state;
    }
}