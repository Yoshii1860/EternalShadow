using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BehaviorTree;

public class ActionShot : Node
{
    #region Fields

    // References to components
    private Animator _animator;
    private NavMeshAgent _agent;

    // Debug mode flag
    private bool _debugMode;

    #endregion




    #region Constructors

    // Constructor to initialize references
    public ActionShot(bool debugMode, Transform transform, NavMeshAgent agent)
    {
        _animator = transform.GetComponent<Animator>();
        _agent = agent;
        _debugMode = debugMode;
    }

    #endregion




    #region Public Methods

    // Evaluate method to determine the state of the node
    public override NodeState Evaluate()
    {

        #region FAILURE CHECKS

        if (GameManager.Instance.IsGamePaused)
        {
            // Return RUNNING to indicate that the action is ongoing
            if (_debugMode) Debug.Log("A - isShot: RUNNING (game is paused)");
            return NodeState.RUNNING;
        }
        
        #endregion




        #region ACTION

        _agent.isStopped = true;
        _animator.SetBool("walk", false);
        _animator.SetBool("run", false);

        if (_debugMode) Debug.Log("A - isShot: RUNNING (dead)");
        return NodeState.RUNNING;

        #endregion
    }

    #endregion
}