    using System.Collections.Generic;
using BehaviorTree;

public class EnemyBT : Tree
{
    #region Public Fields

    // Waypoints for patrolling
    public UnityEngine.Transform[] waypoints;

    // Reference to the NavMeshAgent
    public UnityEngine.AI.NavMeshAgent agent;

    // Debug mode for the behavior tree
    public bool debugMode = false;

    // Type of the enemy: 0 - Slender, 1 - Girl
    public int enemyType = 1;

    #endregion

    #region Static Fields

    // Speed values and ranges for the enemy behavior
    public static float walkSpeed = 0.4f;
    public static float runSpeed = 2.1f;
    public static float attackRange = 3f;
    public static float attackInterval = 1f;
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
                new DecisionAttackRange(debugMode, transform),
                new ActionAttack(debugMode, transform, enemyType)
            }),

            // Sequence for chasing behavior when shot
            new Sequence(new List<Node>
            {
                new DecisionIsShot(debugMode, transform, agent),
                new ActionShot(debugMode, transform, agent)
            }),

            // Sequence for responding to noise
            new Sequence(new List<Node>
            {
                new DecisionNoiseSensing(debugMode, transform),
                new ActionCheckNoise(debugMode, transform, agent, enemyType)
            }),

            // Sequence for moving to the last known position
            new Sequence(new List<Node>
            {
                new DecisionLastKnownPosition(debugMode, transform),
                new ActionLastKnownPosition(debugMode, transform, agent, enemyType)
            }),

            // Sequence for general sensing and moving towards the target
            new Sequence(new List<Node>
            {
                new DecisionSensing(debugMode, transform),
                new ActionGoToTarget(debugMode, transform, agent, enemyType)
            }),

            // Action node for patrolling
            new ActionPatrol(debugMode, transform, waypoints, agent, enemyType)
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