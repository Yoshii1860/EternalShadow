using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class DecisionIsShot : Node
{
    Transform transform;
    Animator animator;

    public DecisionIsShot(Transform transform)
    {
        this.transform = transform;
        animator = transform.GetComponent<Animator>();
    }

    public override NodeState Evaluate()
    {
        if (GameManager.Instance.isPaused) return NodeState.FAILURE;
        
        object obj = GetData("target");

        bool isShot = false;

        // Check if the enemy is shot by accessing the EnemyManager
        Enemy enemyData;
        if (EnemyManager.Instance.TryGetEnemy(transform, out enemyData))
        {
            isShot = enemyData.isShot;
        }

        if (isShot)
        {
            // Set the player as the target
            Debug.Log("DecisionIsShot: Player is shot!");

            if (obj == null) parent.parent.SetData("target", GameManager.Instance.player.transform);
            animator.SetBool("Walking", true);

            state = NodeState.SUCCESS;
            return state;
        }

        state = NodeState.FAILURE;
        return state;
    }
}