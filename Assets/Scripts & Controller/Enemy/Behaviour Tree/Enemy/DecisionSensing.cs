using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class DecisionSensing : Node
{
    // When remove debug "No Attack Mode", make int static
    int characterLayerMask = 1 << 7;

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
        
        ///////////////////////////
        // DEBUG FUNCTION
        if(GameManager.Instance.noAttackMode)
        {
            state = NodeState.FAILURE;
            return state;
        }    
        ///////////////////////////

        if (obj == null)
        {
            // Use OverlapSphereNonAlloc to get colliders in the sphere
            Collider[] colliders = new Collider[10]; // You can adjust the size of this array based on the expected number of nearby colliders
            int count = Physics.OverlapSphereNonAlloc(transform.position, EnemyBT.fovRange, colliders, characterLayerMask);

            // Filter the colliders based on their direction from the enemy
            for (int i = 0; i < count; i++)
            {
                Transform target = colliders[i].transform;
                Vector3 directionToTarget = target.position - transform.position;

                // Calculate the angle between the enemy's forward direction and the direction to the target
                float angleToTarget = Vector3.Angle(directionToTarget, transform.forward);

                // Check if the angle to the target is within the FOV angle
                if (angleToTarget < EnemyBT.fovAngle * 0.5f)
                {
                    // Check if the player is in the enemy's line of sight
                    RaycastHit hit;
                    if (Physics.Raycast(transform.position, directionToTarget, out hit, EnemyBT.fovRange))
                    {
                        // Make sure the raycast hits the player and not any other object in the way
                        if (hit.transform.CompareTag("Player"))
                        {
                            // Player is in sight, store the player as the target
                            parent.parent.SetData("target", target);
                            animator.SetBool("Walking", true);

                            state = NodeState.SUCCESS;
                            return state;
                        }
                    }
                }
            }

            state = NodeState.FAILURE;
            return state;
        }

        state = NodeState.SUCCESS;
        return state;
    }
}