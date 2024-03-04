using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BehaviorTree;

public class S_NavMoveTo : Node
{
    #region Fields

    // References to components
    private NavMeshAgent agent;
    private Vector3 destination;

    #endregion

    #region Constructors

    // Constructor to initialize references
    public S_NavMoveTo(NavMeshAgent agent)
    {
        this.agent = agent;
    }

    #endregion

    #region Public Methods

    // Evaluate method to determine the state of the node
    public override NodeState Evaluate()
    {
        // Retrieve destination from tree data
        destination = (Vector3)GetData("destination");

        if (agent.remainingDistance < agent.stoppingDistance)
        {
            // Agent has reached the destination
            return NodeState.SUCCESS;
        }
        else
        {
            // Move towards the destination
            agent.SetDestination(destination);
            ClearData("destination");
            return NodeState.RUNNING;
        }
    }

    #endregion
}