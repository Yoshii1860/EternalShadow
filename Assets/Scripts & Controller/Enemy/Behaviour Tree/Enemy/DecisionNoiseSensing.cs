using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class DecisionNoiseSensing : Node
{
    Transform transform;
    Animator animator;
    AISensor aiSensor;

    public DecisionNoiseSensing(Transform transform)
    {
        this.transform = transform;
        animator = transform.GetComponent<Animator>();
        aiSensor = transform.GetComponent<AISensor>();
    }

    public override NodeState Evaluate()
    {
        if (GameManager.Instance.isPaused) return NodeState.FAILURE;
        
        ///////////////////////////
        // DEBUG FUNCTION
        if(GameManager.Instance.noNoiseMode)
        {
            state = NodeState.FAILURE;
            return state;
        }    
        ///////////////////////////

        object obj = GetData("target");

        if (obj != null || aiSensor.playerInSight)
        {
            ClearData("noiseLevel");
            state = NodeState.FAILURE;
            return state;
        }

        // save noise Level as the original noise level is always changing
        object obj2 = GetData("noiseLevel");
        float? noiseLevel = (float?)obj2;

        if (noiseLevel == null)
        {
            parent.parent.SetData("noiseLevel", NoiseManager.Instance.noiseLevel);
            noiseLevel = NoiseManager.Instance.noiseLevel;
        }
        else
        {
            state = NodeState.SUCCESS;
            return state;
        }

        if (Vector3.Distance(transform.position, GameManager.Instance.player.transform.position) <= noiseLevel)
        {
            Debug.Log("DecisionNoiseSensing: Noise heard!");

            // Player is in sight, store the player as the target
            Vector3 noisePosition = GameManager.Instance.player.transform.position;
            parent.parent.SetData("noisePosition", noisePosition);
            animator.SetBool("Walking", true);

            state = NodeState.SUCCESS;
            return state;
        }

        ClearData("noiseLevel");

        state = NodeState.FAILURE;
        return state;
    }
}