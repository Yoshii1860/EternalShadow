using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class DecisionAttackRange : Node
{
    #region Fields

    // References to components
    private Transform transform;
    private Animator animator;
    private AISensor aiSensor;

    // Debug mode flag
    private bool debugMode;

    #endregion

    #region Constructors

    // Constructor to initialize references
    public DecisionAttackRange(bool debugMode, Transform transform)
    {
        this.transform = transform;
        this.animator = transform.GetComponent<Animator>();
        this.debugMode = debugMode;
        aiSensor = transform.GetComponent<AISensor>();
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
            // Return RUNNING to indicate that the decision is ongoing
            if (debugMode) Debug.Log("D - AttackRange: RUNNING (Paused)");
            state = NodeState.RUNNING;
            return state;
        }
        ////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////
        // FAILURE CHECKS
        ////////////////////////////////////////////////////////////////////////
        object obj = GetData("target");

        if (obj == null)
        {
            if (debugMode) Debug.Log("D - AttackRange: FAILURE (No Target)");
            state = NodeState.FAILURE;
            return state;
        }
        else if (aiSensor.hidden)
        {
            if (debugMode) Debug.Log("D - AttackRange: FAILURE (Hidden)");
            state = NodeState.FAILURE;
            return state;
        }
        ////////////////////////////////////////////////////////////////////////

        // Calculate the distance between the agent and the target
        Transform target = (Transform)obj;
        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        // Check if the distance is within the attack range
        if (distanceToTarget <= EnemyBT.attackRange)
        {
            animator.SetBool("walk", false);
            animator.SetBool("run", false);
            if (debugMode) Debug.Log("D - AttackRange: SUCCESS (Within Attack Range)");
            state = NodeState.SUCCESS;
            return state;
        }

        if (debugMode) Debug.Log("D - AttackRange: FAILURE (Not Within Attack Range)");
        state = NodeState.FAILURE;
        return state;
    }

    #endregion
}