using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BehaviorTree;

public class ActionCheckNoise : Node
{
    Transform transform;
    UnityEngine.AI.NavMeshAgent agent;
    Animator animator;
    AISensor aiSensor;
    bool isWaiting = false;
    float waitTime = 1f;
    float waitCounter = 0f;

    public ActionCheckNoise(Transform transform, UnityEngine.AI.NavMeshAgent agent)
    {
        this.transform = transform;
        this.agent = agent;
        animator = transform.GetComponent<Animator>();
        aiSensor = transform.GetComponent<AISensor>();
    }

    public override NodeState Evaluate()
    {
        if (GameManager.Instance.isPaused) return NodeState.RUNNING;
        
        object obj = GetData("noisePosition");
        if (obj == null || aiSensor.playerInSight)
        {
            state = NodeState.FAILURE;
            return state;
        }

        Vector3 noisePos = (Vector3)obj;

        if (isWaiting)
        {
            waitCounter += Time.deltaTime;
            if (waitCounter >= waitTime)
            {
                isWaiting = false;
                animator.SetBool("Walking", true);
                ClearData("noisePosition");
                ClearData("noiseLevel");

                state = NodeState.SUCCESS;
                return state;
            }
        }
        else
        {
            if (Vector3.Distance(transform.position, noisePos) < 0.01f)
            {
                transform.position = noisePos;
                waitCounter = 0f;
                isWaiting = true;

                animator.SetBool("Walking", false);
            }
            else
            {
                Debug.Log("ActionCheckNoise: On the way");
                agent.speed = EnemyBT.walkSpeed;
                agent.SetDestination(noisePos);
            }
        }

        state = NodeState.RUNNING;
        return state;
    }
}