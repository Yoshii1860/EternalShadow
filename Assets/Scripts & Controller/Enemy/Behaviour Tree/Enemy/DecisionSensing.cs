using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class DecisionSensing : Node
{
    // When remove debug "No Attack Mode", make int static
    float maxChaseRange = 15f; // Same as in AISensor.cs

    Transform transform;
    Animator animator;
    AISensor aiSensor;

    public DecisionSensing(Transform transform)
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
        if (GameManager.Instance.noAttackMode)
        {
            state = NodeState.FAILURE;
            return state;
        }
        ///////////////////////////

        object obj = GetData("target");
        object obj2 = GetData("lastKnownPosition");

        if (obj == null)
        {
            if (aiSensor.playerInSight)
            {
                Debug.Log("DecisionSensing: Player in sight!");
                ClearData("lastKnownPosition");
                parent.parent.SetData("target", GameManager.Instance.player.transform);

                state = NodeState.SUCCESS;
                return state;
            }
            else if (obj2 == null)
            {
                state = NodeState.FAILURE;
                return state;
            }
        }
        else if (obj != null)
        {
            Transform target = (Transform)obj;
            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            if (distanceToTarget > maxChaseRange)
            {
                Debug.Log("DecisionSensing: Player out of range!");
                Vector3 lastKnownPosition = GameManager.Instance.player.transform.position;
                parent.parent.SetData("lastKnownPosition", lastKnownPosition);
                ClearData("target");

                state = NodeState.FAILURE;
                return state;
            }
            else
            {
                state = NodeState.SUCCESS;
                return state;
            }
        }

        state = NodeState.FAILURE;
        return state;
    }
}