using System.Collections.Generic;
using BehaviorTree;

public class EnemyBT : Tree
{
    public UnityEngine.Transform[] waypoints;

    public static float speed = 2f;
    public static float fovRange = 8f;
    public static float attackRange = 2f;

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
}