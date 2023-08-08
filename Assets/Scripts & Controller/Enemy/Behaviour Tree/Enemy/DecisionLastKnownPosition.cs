using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class DecisionLastKnownPosition : Node
{
    Transform transform;

    public DecisionLastKnownPosition(Transform transform)
    {
        this.transform = transform;
    }

    public override NodeState Evaluate()
    {
        if (GameManager.Instance.isPaused) return NodeState.RUNNING;

        object obj = GetData("lastKnownPosition");

        if (obj != null)
        {
            state = NodeState.SUCCESS;
            return state;
        }

        state = NodeState.FAILURE;
        return state;
    }
}