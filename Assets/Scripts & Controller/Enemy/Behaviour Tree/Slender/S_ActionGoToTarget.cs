using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BehaviorTree;

public class S_ActionGoToTarget : Node
{
    #region Fields

    // References to components
    private Animator animator;
    private Transform transform;
    private NavMeshAgent agent;
    private Enemy enemy;
    
    private bool debugMode;

    #endregion

    #region Constructors

    // Constructor to initialize references
    public S_ActionGoToTarget(bool debugMode, Transform transform, NavMeshAgent agent)
    {
        this.transform = transform;
        this.agent = agent;
        animator = transform.GetComponent<Animator>();
        enemy = transform.GetComponent<Enemy>();
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
            // Return FAILURE to indicate that the action cannot be executed
            if (debugMode) Debug.Log("A - GoToTarget: FAILURE (game is paused)");
            state = NodeState.FAILURE;
            return state;
        }
        ////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////
        // FAILURE CHECKS
        ////////////////////////////////////////////////////////////////////////
        object obj = GetData("target");

        if (obj == null)
        {
            // Set state to FAILURE and return
            if (debugMode) Debug.Log("A - GoToTarget: FAILURE (target = null)");
            state = NodeState.FAILURE;
            return state;
        }
        ////////////////////////////////////////////////////////////////////////

        // Get target transform
        Transform target = (Transform)obj;

        // Set agent speed, set destination, and trigger running animation
        agent.speed = SlenderBT.runSpeed;
        animator.SetBool("walk", false);
        animator.SetBool("run", true);
        agent.SetDestination(target.position);

        AudioManager.Instance.ToggleSlenderAudio(transform.gameObject, true);

        // Return RUNNING to indicate that the action is ongoing
        if (debugMode) Debug.Log("A - GoToTarget: RUNNING (Going to target)");
        state = NodeState.RUNNING;
        return state;
    }

    #endregion
}