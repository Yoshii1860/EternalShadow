using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class ActionAttack : Node
{
    Animator animator;

    Transform lastTarget;
    Player player;

    float attackTime = 1f;
    float attackCounter = 0f;

    public ActionAttack(Transform transform)
    {
        animator = transform.GetComponent<Animator>();
    }

    public override NodeState Evaluate()
    {
        Transform target = (Transform)GetData("target");
        if (target != lastTarget)
        {
            player = target.GetComponent<Player>();
            lastTarget = target;
        }

        attackCounter += Time.deltaTime;
        if (attackCounter >= attackTime)
        {
            bool playerIsDead = player.TakeDamage();
            if (playerIsDead)
            {
                ClearData("target");
                animator.SetBool("Attacking", false);
                animator.SetBool("Walking", true);
            }
            else
            {
                attackCounter = 0f;
            }
        }

        state = NodeState.RUNNING;
        return state;
    }
}