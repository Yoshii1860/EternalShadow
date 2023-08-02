using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class DecisionNoiseSensing : Node
{
    // When remove debug "No Attack Mode", make int static
    int characterLayerMask = 1 << 7;

    Transform transform;
    Animator animator;

    public DecisionNoiseSensing(Transform transform)
    {
        this.transform = transform;
        animator = transform.GetComponent<Animator>();
    }

    public override NodeState Evaluate()
    {
        ///////////////////////////
        // DEBUG FUNCTION
        if(GameManager.Instance.noNoiseMode)
        {
            state = NodeState.FAILURE;
            return state;
        }    
        ///////////////////////////

        // Use OverlapSphereNonAlloc to get colliders in the sphere
        Collider[] colliders = new Collider[10]; // You can adjust the size of this array based on the expected number of nearby colliders
        int count = Physics.OverlapSphereNonAlloc(transform.position, NoiseManager.Instance.noiseLevel, colliders, characterLayerMask);

        // Filter the colliders based on their direction from the enemy
        for (int i = 0; i < count; i++)
        {
            if(colliders[i].transform.CompareTag("Player"))
            {
                // Player is in sight, store the player as the target
                parent.parent.SetData("lastKnownPosition", colliders[i].transform.position);
                animator.SetBool("Walking", true);

                state = NodeState.SUCCESS;
                return state;
            }
        }
        state = NodeState.FAILURE;
        return state;
    }
}