using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class DecisionIsShot : Node
{
    #region Fields

    // References to components
    private Transform transform;
    private Animator animator;

    #endregion

    #region Constructors

    // Constructor to initialize references
    public DecisionIsShot(Transform transform)
    {
        this.transform = transform;
        this.animator = transform.GetComponent<Animator>();
    }

    #endregion

    #region Public Methods

    // Evaluate method to determine the state of the node
    public override NodeState Evaluate()
    {
        // Check if the game is paused
        if (GameManager.Instance.isPaused)
        {
            // Return FAILURE to indicate that the decision is not satisfied
            return NodeState.FAILURE;
        }

        // Retrieve the target from the blackboard
        object obj = GetData("target");

        // Initialize variable to track if the enemy is shot
        bool isShot = false;

        // Check if the enemy is shot by accessing the EnemyManager
        Enemy enemyData;
        if (EnemyManager.Instance.TryGetEnemy(transform, out enemyData))
        {
            isShot = enemyData.isShot;
        }

        // If the enemy is shot, set the player as the target
        if (isShot)
        {
            Debug.Log("DecisionIsShot: Enemy is shot!");

            // If the target is not already set, set it to the player
            if (obj == null) 
            {
                parent.parent.SetData("target", GameManager.Instance.player.transform);
            }

            // Set state to SUCCESS
            state = NodeState.SUCCESS;
            return state;
        }

        // If the enemy is not shot, set state to FAILURE
        state = NodeState.FAILURE;
        return state;
    }

    #endregion
}