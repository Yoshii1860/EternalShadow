using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class DecisionNoiseSensing : Node
{
    Transform transform;
    Animator animator;
    AISensor aiSensor;
    float noiseReduction = 0.6f;

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
            Debug.Log("DecisionNoiseSensing: NoiseLevel: " + noiseLevel);
        }

        if (Vector3.Distance(transform.position, GameManager.Instance.player.transform.position) <= noiseLevel)
        {
            Debug.Log("DecisionNoiseSensing: Noise heard!");
            // Check if there is a collider between the enemy and the player
            RaycastHit hit;
            Vector3 playerPosition = GameManager.Instance.player.transform.position;
            Vector3 directionToPlayer = playerPosition - transform.position;

            if (!Physics.Linecast(transform.position, playerPosition, out hit, LayerMask.GetMask("Wall")))
            {
                Debug.Log("DecisionNoiseSensing: Noise heard! No wall in between!");
                Vector3 noisePosition = GameManager.Instance.player.transform.position;
                parent.parent.SetData("noisePosition", GameManager.Instance.player.transform.position);

                state = NodeState.SUCCESS;
                return state;
            }
            else if (Vector3.Distance(transform.position, GameManager.Instance.player.transform.position) <= noiseLevel * noiseReduction)
            {
                Debug.Log("DecisionNoiseSensing: Noise heard through wall!");
                Vector3 noisePosition = GameManager.Instance.player.transform.position;
                parent.parent.SetData("noisePosition", GameManager.Instance.player.transform.position);

                state = NodeState.SUCCESS;
                return state;
            }
            else
            {

                Debug.Log("DecisionNoiseSensing: Noise heard! Wall in between - too far away!");
                parent.parent.ClearData("noiseLevel");
                state = NodeState.FAILURE;
                return state;
            }
        }

        Debug.Log("DecisionNoiseSensing: No noise heard!");
        ClearData("noiseLevel");

        state = NodeState.FAILURE;
        return state;
    }
}