using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BehaviorTree;

public class S_ActionShot : Node
{
    #region Fields

    // References to components
    private Transform transform;
    private Animator animator;
    private NavMeshAgent agent;

    // Debug mode flag
    private bool debugMode;

    #endregion

    #region Constructors

    // Constructor to initialize references
    public S_ActionShot(bool debugMode, Transform transform, NavMeshAgent agent)
    {
        this.transform = transform;
        animator = transform.GetComponent<Animator>();
        this.agent = agent;
        this.debugMode = debugMode;
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
            // Return RUNNING to indicate that the action is ongoing
            if (debugMode) Debug.Log("A - ChaseTarget: RUNNING (game is paused)");
            state = NodeState.RUNNING;
            return state;
        }
        ////////////////////////////////////////////////////////////////////////

        agent.isStopped = true;
        animator.SetBool("walk", false);
        animator.SetBool("run", false);

        // If the chase is still ongoing, return RUNNING
        if (debugMode) Debug.Log("A - ChaseTarget: RUNNING (dead)");
        state = NodeState.RUNNING;
        return state;
    }

    #endregion
}