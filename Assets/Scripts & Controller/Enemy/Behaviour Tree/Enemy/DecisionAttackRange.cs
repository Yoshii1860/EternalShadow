
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class DecisionAttackRange : Node
{
    Transform transform;
    Animator animator;

    public DecisionAttackRange(Transform transform)
    {
        this.transform = transform;
        animator = transform.GetComponent<Animator>();
    }

    public override NodeState Evaluate()
    {
        object obj = GetData("target");
        if (obj == null)
        {
            state = NodeState.FAILURE;
            return state;
        }

        Transform target = (Transform)obj;
        if (Vector3.Distance(transform.position, target.position) <= EnemyBT.attackRange)
        {
            animator.SetBool("Attacking", true);
            animator.SetBool("Walking", false);
            state = NodeState.SUCCESS;
            return state;
        }

        state = NodeState.FAILURE;
        return state;
    }
}
