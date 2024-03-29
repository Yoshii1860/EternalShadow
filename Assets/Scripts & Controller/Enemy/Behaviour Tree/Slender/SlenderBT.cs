using System.Collections.Generic;
using BehaviorTree;

public class SlenderBT : Tree
{
    #region Public Fields

    // Waypoints for patrolling
    public UnityEngine.Transform[] waypoints;

    // Reference to the NavMeshAgent
    public UnityEngine.AI.NavMeshAgent agent;

    // Debug mode for the behavior tree
    public bool debugMode = false;

    #endregion

    #region Static Fields

    // Speed values and ranges for the enemy behavior
    public static float walkSpeed = 0.6f;
    public static float runSpeed = 4f;
    public static float attackRange = 3f;
    public static float attackInterval = 5f;
    public static float chaseRange = 30f;

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
                new S_DecisionAttackRange(debugMode, transform),
                new S_ActionAttack(debugMode, transform)
            }),

            // Sequence for chasing behavior when shot
            new Sequence(new List<Node>
            {
                new S_DecisionIsShot(debugMode, transform, agent),
                new S_ActionShot(debugMode, transform, agent)
            }),

            // Sequence for responding to noise
            new Sequence(new List<Node>
            {
                new S_DecisionNoiseSensing(debugMode, transform),
                new S_ActionCheckNoise(debugMode, transform, agent)
            }),

            // Sequence for moving to the last known position
            new Sequence(new List<Node>
            {
                new S_DecisionLastKnownPosition(debugMode, transform),
                new S_ActionLastKnownPosition(debugMode, transform, agent)
            }),

            // Sequence for general sensing and moving towards the target
            new Sequence(new List<Node>
            {
                new S_DecisionSensing(debugMode, transform),
                new S_ActionGoToTarget(debugMode, transform, agent)
            }),

            // Action node for patrolling
            new S_ActionPatrol(debugMode, transform, waypoints, agent)
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