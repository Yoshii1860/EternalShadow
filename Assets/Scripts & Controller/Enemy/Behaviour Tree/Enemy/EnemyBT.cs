using System.Collections.Generic;
using BehaviorTree;

public class EnemyBT : Tree
{
    public UnityEngine.Transform[] waypoints;
    public UnityEngine.AI.NavMeshAgent agent;

    public static float walkSpeed = 2f;
    public static float runSpeed = 3.5f;
    public static float attackRange = 3f;
    public static float attackIntervall = 1f;
    public static float chaseRange = 30f;

    protected override Node SetupTree()
    {
        Node root = new Selector(new List<Node>
        {
            new Sequence(new List<Node>
            {
                new DecisionAttackRange(transform),
                new ActionAttack(transform)
            }),
            new Sequence(new List<Node>
            {
                new DecisionIsShot(transform),
                new ActionChaseTarget(agent)
            }),
            new Sequence(new List<Node>
            {
                new DecisionNoiseSensing(transform),
                new ActionCheckNoise(transform, agent)
            }),
            new Sequence(new List<Node>
            {
                new DecisionLastKnownPosition(transform),
                new ActionLastKnownPosition(transform, agent)
            }),
            new Sequence(new List<Node>
            {
                new DecisionSensing(transform),
                new ActionGoToTarget(transform, agent)
            }),
            new ActionPatrol(transform, waypoints, agent)
        });

        return root;
    }

    // Reset data of all nodes recursively
    public void ResetTree()
    {
        if (_root != null)
        {
            ClearNodeDataRecursive(_root);
        }
    }
}