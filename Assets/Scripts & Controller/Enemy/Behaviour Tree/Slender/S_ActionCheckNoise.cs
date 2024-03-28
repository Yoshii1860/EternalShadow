using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BehaviorTree;

public class S_ActionCheckNoise : Node
{
    #region Fields

    // References to components
    private Transform transform;
    private NavMeshAgent agent;
    private Animator animator;
    private AISensor aiSensor;

    // Variables for waiting behavior
    private bool isWaiting = false;
    private float waitTime = 1f;
    private float waitCounter = 0f;
    private Enemy enemy;

    private float elapsedTime = 0f;
    private float stuckTimer = 30f;

    // Debug mode flag
    private bool debugMode;

    #endregion

    #region Constructors

    // Constructor to initialize references
    public S_ActionCheckNoise(bool debugMode, Transform transform, NavMeshAgent agent)
    {
        this.transform = transform;
        this.agent = agent;
        animator = transform.GetComponent<Animator>();
        enemy = transform.GetComponent<Enemy>();
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
            // Return RUNNING to indicate that the action is ongoing
            if (debugMode) Debug.Log("A - CheckNoise: RUNNING (game is paused)");
            state = NodeState.RUNNING;
            return state;
        }
        ////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////
        // FAILURE CHECKS
        ////////////////////////////////////////////////////////////////////////
        object obj = GetData("target");
        object objPos = GetData("noisePosition");
        if (obj != null)
        {
            if (debugMode) Debug.Log("A - CheckNoise: FAILURE (target != null)");
            state = NodeState.FAILURE;
            return state;
        }
        else if (aiSensor.playerInSight)
        {
            // Set state to FAILURE
            if (debugMode) Debug.Log("A - CheckNoise: FAILURE (player in sight)");
            parent.parent.SetData("target", GameManager.Instance.player.transform);
            state = NodeState.FAILURE;
            return state;
        }
        else if (objPos == null)
        {
            ClearData("noisePosition");
            ClearData("noiseLevel");
            // Set state to FAILURE
            if (debugMode) Debug.Log("A - CheckNoise: FAILURE (noisePosition = null)");
            state = NodeState.FAILURE;
            return state;
        }
        ////////////////////////////////////////////////////////////////////////

        // Get noise position
        Vector3 noisePos = (Vector3)objPos;

        // Check if the AI is waiting
        if (isWaiting)
        {
            // Increment wait counter
            waitCounter += Time.deltaTime;

            // Check if the waiting time is over
            if (waitCounter >= waitTime)
            {
                // Reset waiting flags, clear noise data, and set state to SUCCESS
                isWaiting = false;
                ClearData("noisePosition");
                ClearData("noiseLevel");
                waitCounter = 0f;
                elapsedTime = 0f;
                if (debugMode) Debug.Log("A - CheckNoise: SUCCESS (wait time over)");
                state = NodeState.SUCCESS;
                return state;
            }
        }
        else
        {
            // Check if AI has reached the noise position
            if (Vector3.Distance(transform.position, noisePos) < 0.1f)
            {
                isWaiting = true;
                
                // Stop walking animation
                animator.SetBool("walk", false);
            }
            else
            {
                elapsedTime += Time.deltaTime;

                // Check if AI is stuck
                if (elapsedTime >= stuckTimer)
                {
                    isWaiting = true;

                    // Stop walking animation
                    animator.SetBool("walk", false);

                    if (debugMode) Debug.Log("A - CheckNoise: STUCK");
                }
                else
                {
                    // Increment elapsed time
                    elapsedTime += Time.deltaTime;
                }

                // Set agent speed, set destination, and start walking animation
                agent.speed = EnemyBT.walkSpeed;
                agent.SetDestination(noisePos);
                animator.SetBool("walk", true);

                AudioManager.Instance.ToggleSlenderAudio(transform.gameObject, false);
            }
        }

        // If the action is still ongoing, return RUNNING
        if (debugMode) Debug.Log("A - CheckNoise: RUNNING (on the way)");
        state = NodeState.RUNNING;
        return state;
    }

    #endregion
}