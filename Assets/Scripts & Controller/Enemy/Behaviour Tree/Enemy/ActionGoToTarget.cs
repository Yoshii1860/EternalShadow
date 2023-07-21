using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class ActionGoToTarget : Node
{
    Transform transform;

    public ActionGoToTarget(Transform transform)
    {
        this.transform = transform;
    }

    public override NodeState Evaluate()
    {
        Transform target = (Transform)GetData("target");

        if (Vector3.Distance(transform.position, target.position) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.position, EnemyBT.speed * Time.deltaTime);
            transform.LookAt(target.position);
        }

        state = NodeState.RUNNING;
        return state;
    }
}
