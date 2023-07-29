using System.Collections.Generic;
using BehaviorTree;

public class EnemyBT : Tree
{
    public UnityEngine.Transform[] waypoints;

    public static float speed = 2f;
    public static float fovRange = 12f;
    public static float attackRange = 2f;
    public static float rotationSpeed = 2f;
    public static float fovAngle = 140f;

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
                new DecisionSensing(transform),
                new ActionGoToTarget(transform)
            }),
            new ActionPatrol(transform, waypoints)
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