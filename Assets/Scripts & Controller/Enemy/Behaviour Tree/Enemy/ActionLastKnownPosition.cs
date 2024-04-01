using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BehaviorTree;

public class ActionLastKnownPosition : Node
{
    #region Fields

    // References to components
    private Transform transform;
    private NavMeshAgent agent;
    private Animator animator;
    private Enemy enemy;
    private AISensor aiSensor;

    // Debug mode flag
    private bool debugMode;

    private int enemyType;

    #endregion

    #region Constructors

    // Constructor to initialize references
    public ActionLastKnownPosition(bool debugMode, Transform transform, NavMeshAgent agent, int enemyType)
    {
        this.transform = transform;
        this.agent = agent;
        animator = transform.GetComponent<Animator>();
        enemy = transform.GetComponent<Enemy>();
        this.debugMode = debugMode;
        aiSensor = transform.GetComponent<AISensor>();
        this.enemyType = enemyType;
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
            if (debugMode) Debug.Log("A - LastKnownPosition: RUNNING (game is paused)");
            state = NodeState.RUNNING;
            return state;
        }
        ////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////
        // FAILURE CHECKS
        ////////////////////////////////////////////////////////////////////////
        object obj = GetData("target");
        object objPos = GetData("lastKnownPosition");

        if (obj != null)
        {
            if (debugMode) Debug.Log("A - LastKnownPosition: FAILURE (target = null)");
            ClearData("lastKnownPosition");
            state = NodeState.FAILURE;
            return state;
        }
        else if (aiSensor.playerInSight)
        {
            // Set state to FAILURE and return
            if (debugMode) Debug.Log("A - LastKnownPosition: FAILURE (Player in Sight)");
            ClearData("lastKnownPosition");
            parent.parent.SetData("target", GameManager.Instance.player.transform);
            state = NodeState.FAILURE;
            return state;
        }
        if (objPos == null)
        {
            // Set state to FAILURE and return
            if (debugMode) Debug.Log("A - LastKnownPosition: FAILURE (lastKnownPosition = null) or (Player in Sight)");
            ClearData("lastKnownPosition");
            state = NodeState.FAILURE;
            return state;
        }
        ////////////////////////////////////////////////////////////////////////

        // Get last known position
        Vector3 lastKnownPosition = (Vector3)objPos;

        // Check if AI has reached the last known position
        if (Vector3.Distance(transform.position, lastKnownPosition) < 0.1f)
        {
            // Debug message and animation state update
            animator.SetBool("walk", false);

            // Clear the last known position data and set state to SUCCESS
            ClearData("lastKnownPosition");
            if (debugMode) Debug.Log("A - LastKnownPosition: SUCCESS (Reached Last Known Position)");
            state = NodeState.SUCCESS;
            return state;
        }

        agent.speed = EnemyBT.walkSpeed;
        agent.SetDestination(lastKnownPosition);
        animator.SetBool("walk", true);

        AudioManager.Instance.ToggleEnemyAudio(transform.gameObject, false, enemyType);

        // Return RUNNING to indicate that the action is ongoing
        if (debugMode) Debug.Log("A - LastKnownPosition: RUNNING (Moving to Last Known Position)");
        state = NodeState.RUNNING;
        return state;
    }

    #endregion
}