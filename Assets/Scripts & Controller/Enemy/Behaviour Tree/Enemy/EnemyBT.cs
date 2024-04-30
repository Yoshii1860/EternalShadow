using System.Collections.Generic;
using BehaviorTree;

public class EnemyBT : Tree
{
    #region Public Fields

    // Waypoints for patrolling
    public UnityEngine.Transform[] Waypoints;

    // Reference to the NavMeshAgent
    public UnityEngine.AI.NavMeshAgent Agent;

    // Debug mode for the behavior tree
    public bool DebugMode = false;

    #endregion




    #region Tree Setup

    // Override method to set up the behavior tree
    protected override Node SetupTree()
    {
        // Creating the root node with a Selector
        Node root = new Selector(new List<Node>
        {
            // Sequence for attacking behavior
            new Sequence(new List<Node>
            {
                new DecisionAttackRange(DebugMode, transform),
                new ActionAttack(DebugMode, transform)
            }),

            // Sequence for chasing behavior when shot
            new Sequence(new List<Node>
            {
                new DecisionIsShot(DebugMode, transform, Agent),
                new ActionShot(DebugMode, transform, Agent)
            }),

            // Sequence for responding to noise
            new Sequence(new List<Node>
            {
                new DecisionNoiseSensing(DebugMode, transform),
                new ActionCheckNoise(DebugMode, transform, Agent)
            }),

            // Sequence for moving to the last known position
            new Sequence(new List<Node>
            {
                new DecisionLastKnownPosition(DebugMode, transform),
                new ActionLastKnownPosition(DebugMode, transform, Agent)
            }),

            // Sequence for general sensing and moving towards the target
            new Sequence(new List<Node>
            {
                new DecisionSensing(DebugMode, transform),
                new ActionGoToTarget(DebugMode, transform, Agent)
            }),

            // Action node for patrolling
            new ActionPatrol(DebugMode, transform, Waypoints, Agent)
        });

        return root;
    }

    #endregion




    #region Public Methods

    // Reset data of all nodes recursively
    public void ResetTree()
    {
        if (_root != null)
        {
            ClearNodeDataRecursive(_root);
        }
    }

    #endregion
}