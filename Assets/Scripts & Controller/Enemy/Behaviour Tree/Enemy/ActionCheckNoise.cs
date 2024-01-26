using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BehaviorTree;

public class ActionCheckNoise : Node
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

    #endregion

    #region Constructors

    // Constructor to initialize references
    public ActionCheckNoise(Transform transform, NavMeshAgent agent)
    {
        this.transform = transform;
        this.agent = agent;
        animator = transform.GetComponent<Animator>();
        aiSensor = transform.GetComponent<AISensor>();
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
            return NodeState.RUNNING;
        }

        // Retrieve noise position from blackboard
        object obj = GetData("noisePosition");

        // Check if there is no noise position or if the player is in sight
        if (obj == null || aiSensor.playerInSight)
        {
            // Clear noise data and set state to FAILURE
            ClearData("noisePosition");
            ClearData("noiseLevel");
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
                animator.SetBool("Walk", false);
            }
            else
            {
                // Debug message indicating that AI is on the way
                Debug.Log("ActionCheckNoise: On the way");

                // Set agent speed, set destination, and start walking animation
                agent.speed = EnemyBT.walkSpeed;
                agent.SetDestination(noisePos);
                animator.SetBool("Walk", true);
            }
        }

        // If the action is still ongoing, return RUNNING
        state = NodeState.RUNNING;
        return state;
    }

    #endregion
}