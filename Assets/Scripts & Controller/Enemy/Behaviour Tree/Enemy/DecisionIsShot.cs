using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BehaviorTree;

public class DecisionIsShot : Node
{
    #region Fields

    // References
    private Transform _transform;
    private NavMeshAgent _agent;

    private bool _debugMode;

    #endregion




    #region Constructors

    public DecisionIsShot(bool debugMode, Transform transform, NavMeshAgent agent)
    {
        _transform = transform;
        _debugMode = debugMode;
        _agent = agent;
    }

    #endregion





    #region Public Methods

    public override NodeState Evaluate()
    {
        
        #region FAILURE CHECKS

        if (GameManager.Instance.IsGamePaused)
        {
            if (_debugMode) Debug.Log("D - IsShot: FAILURE (Game is paused)");
            return NodeState.FAILURE;
        }

        #endregion




        #region DECISION

        bool isShot = false;
        Enemy enemyData;

        // Get the information from the EnemyManager
        if (EnemyManager.Instance.TryGetEnemy(_transform, out enemyData))
        {
            isShot = enemyData.IsShot;
        }

        // If the enemy is shot, set the player as the target
        if (isShot)
        {
            ClearData("target");
            ClearData("lastKnownPosition");
            ClearData("noisePosition");

            if (_debugMode) Debug.Log("D - IsShot: SUCCESS (Enemy is shot)");
            return NodeState.SUCCESS;
        }
        else
        {
            _agent.isStopped = false;
        }

        if (_debugMode) Debug.Log("D - IsShot: FAILURE (Enemy is not shot)");
        return NodeState.FAILURE;

        #endregion
    }

    #endregion
}