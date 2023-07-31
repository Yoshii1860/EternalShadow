using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace BehaviorTree
{
    public class ActionCheckLastKnownPosition : Node
    {
        Transform transform;
        NavMeshAgent agent;

        public ActionCheckLastKnownPosition(Transform transform, NavMeshAgent agent)
        {
            this.transform = transform;
            this.agent = agent;
        }

        public override NodeState Evaluate()
        {
            if (GetData("lastKnownPosition") == null)
            {
                return NodeState.SUCCESS;
            }

            Debug.Log("ACLKP - WALK TO POINT");
            // Move towards the last known position
            Vector3 lastKnownPosition = (Vector3)GetData("lastKnownPosition");
            agent.speed = EnemyBT.walkSpeed;
            agent.SetDestination(lastKnownPosition);
            ClearData("target");

            // Check if the agent has reached the last known position
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                // Reset lastKnownPosition and return to patrol
                ClearData("lastKnownPosition");
                agent.isStopped = true;
                return NodeState.FAILURE;
            }

            return NodeState.RUNNING;
        }
    }
}