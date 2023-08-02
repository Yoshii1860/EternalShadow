using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class DecisionIsShot : Node
{
    int characterLayerMask = 1 << 7;
    float maxChaseRange = 30f;

    Transform transform;
    Animator animator;

    public DecisionIsShot(Transform transform)
    {
        this.transform = transform;
        animator = transform.GetComponent<Animator>();
    }

    public override NodeState Evaluate()
    {
        object obj = GetData("target");

        bool isShot = false;

        // Check if the enemy is shot by accessing the EnemyManager
        Enemy enemyData;
        if (EnemyManager.Instance.TryGetEnemy(transform, out enemyData))
        {
            isShot = enemyData.isShot;
        }

        if (obj == null && isShot)
        {
            // Use OverlapSphereNonAlloc to get colliders in the sphere
            Collider[] colliders = new Collider[10]; // You can adjust the size of this array based on the expected number of nearby colliders
            int count = Physics.OverlapSphereNonAlloc(transform.position, maxChaseRange, colliders, characterLayerMask);

            // Filter the colliders based on their direction from the enemy and tag
            for (int i = 0; i < colliders.Length; i++)
            {
                Transform target = colliders[i].transform;
                Vector3 directionToTarget = target.position - transform.position;
                float distanceToTarget = directionToTarget.magnitude;

                // Check if the distance to the target is within the maxChaseRange and the target has the "Player" tag
                if (distanceToTarget <= maxChaseRange && target.CompareTag("Player"))
                {
                    // Set the player as the target
                    parent.parent.SetData("target", target);
                    animator.SetBool("Walking", true);

                    state = NodeState.SUCCESS;
                    return state;
                }
            }

            state = NodeState.FAILURE;
            return state;
        }
        else if (obj != null && isShot)
        {
            state = NodeState.SUCCESS;
            return state;
        }

        state = NodeState.FAILURE;
        return state;
    }
}