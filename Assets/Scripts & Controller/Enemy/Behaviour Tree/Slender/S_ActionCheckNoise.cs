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

    // Variables for waiting behavior
    private bool isWaiting = false;
    private float waitTime = 1f;
    private float waitCounter = 0f;
    private Enemy enemy;

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
    }

    #endregion

    #region Public Methods

    // Evaluate method to determine the state of the node
    public override NodeState Evaluate()
    {
        // Check if the game is paused
        if (GameManager.Instance.isPaused)
        {
            // Return RUNNING to indicate that the action is ongoing
            if (debugMode) Debug.Log("A - CheckNoise: RUNNING (game is paused)");
            state = NodeState.RUNNING;
            return state;
        }

        // Retrieve noise position from blackboard
        object obj = GetData("noisePosition");

        // Check if there is no noise position or if the player is in sight
        if (obj == null)
        {
            ClearData("noisePosition");
            ClearData("noiseLevel");
            // Set state to FAILURE
            if (debugMode) Debug.Log("A - CheckNoise: FAILURE (noisePosition = null)");
            state = NodeState.FAILURE;
            return state;
        }

        // Get noise position
        Vector3 noisePos = (Vector3)obj;

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
                if (debugMode) Debug.Log("A - CheckNoise: SUCCESS (wait time over)");
                state = NodeState.SUCCESS;
                return state;
            }
        }
        else
        {
            // Check if AI has reached the noise position
            if (Vector3.Distance(transform.position, noisePos) < 0.05f)
            {
                // Set AI position to noise position, reset wait counter, and enter waiting state
                transform.position = noisePos;
                waitCounter = 0f;
                isWaiting = true;

                // Stop walking animation
                animator.SetBool("walk", false);
            }
            else
            {
                /*if (enemy.enteredDoor)
                {
                    parent.parent.SetData("destination", noisePos);
                    animator.SetBool("walk", false);
                    animator.SetBool("run", false);
                    return NodeState.FAILURE;
                }*/

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