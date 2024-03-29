using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class S_DecisionIsShot : Node
{
    #region Fields

    // References to components
    private Transform transform;
    private Animator animator;
    private UnityEngine.AI.NavMeshAgent agent;

    // Debug mode flag
    private bool debugMode;

    #endregion

    #region Constructors

    // Constructor to initialize references
    public S_DecisionIsShot(bool debugMode, Transform transform, UnityEngine.AI.NavMeshAgent agent)
    {
        this.transform = transform;
        this.animator = transform.GetComponent<Animator>();
        this.debugMode = debugMode;
        this.agent = agent;
    }

    #endregion

    #region Public Methods

    // Evaluate method to determine the state of the node
    public override NodeState Evaluate()
    {
        ////////////////////////////////////////////////////////////////////////
        // PAUSE GAME
        ////////////////////////////////////////////////////////////////////////
        if (GameManager.Instance.isPaused)
        {
            // Return FAILURE to indicate that the decision is not satisfied
            if (debugMode) Debug.Log("D - IsShot: FAILURE (Game is paused)");
            state = NodeState.FAILURE;
            return state;
        }
        ////////////////////////////////////////////////////////////////////////

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
            ClearData("target");
            ClearData("lastKnownPosition");
            ClearData("noisePosition");

            // Set state to SUCCESS
            if (debugMode) Debug.Log("D - IsShot: SUCCESS (Enemy is shot)");
            state = NodeState.SUCCESS;
            return state;
        }
        else
        {
            agent.isStopped = false;
        }

        // If the enemy is not shot, set state to FAILURE
        if (debugMode) Debug.Log("D - IsShot: FAILURE (Enemy is not shot)");
        state = NodeState.FAILURE;
        return state;
    }

    #endregion
}