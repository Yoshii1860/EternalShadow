using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class DecisionSensing : Node
{
    static int characterLayerMask = 1 << 7;

    Transform transform;
    Animator animator;

    public DecisionSensing(Transform transform)
    {
        this.transform = transform;
        animator = transform.GetComponent<Animator>();
    }

    public override NodeState Evaluate()
    {
        object obj = GetData("target");
        if (obj == null)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, EnemyBT.fovRange, characterLayerMask);

            if (colliders.Length > 0)
            {
                parent.parent.SetData("target", colliders[0].transform);
                animator.SetBool("Walking", true);

                state = NodeState.SUCCESS;
                return state;
            }

            state = NodeState.FAILURE;
            return state;
        }

        state = NodeState.SUCCESS;
        return state;
    }
}