using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class S_DecisionNoiseSensing : Node
{
    #region Fields

    // References to components
    private Transform transform;
    private Animator animator;
    private AISensor aiSensor;

    // Reduction factor for noise level through walls
    private float noiseReduction = 0.6f;

    // Debug mode flag
    private bool debugMode;

    #endregion

    #region Constructors

    // Constructor to initialize references to components
    public S_DecisionNoiseSensing(bool debugMode, Transform transform)
    {
        this.transform = transform;
        animator = transform.GetComponent<Animator>();
        aiSensor = transform.GetComponent<AISensor>();
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
            // Return FAILURE to indicate that the decision cannot be made
            if (debugMode) Debug.Log("D - NoiseSensing: FAILURE (Game is paused)");
            state = NodeState.FAILURE;
            return state;
        }
        ////////////////////////////////////////////////////////////////////////
        
        ////////////////////////////////////////////////////////////////////////
        // No Noise Mode
        ////////////////////////////////////////////////////////////////////////
        if (GameManager.Instance.noNoiseMode)
        {
            // If in debug mode, return FAILURE
            if (debugMode) Debug.Log("D - NoiseSensing: FAILURE (No Noise Mode)");
            state = NodeState.FAILURE;
            return state;
        }
        ////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////
        // FAILURE CHECKS
        ////////////////////////////////////////////////////////////////////////
        object obj = GetData("target");

        if (obj != null)
        {
            // Clear noise data and return FAILURE
            ClearData("noiseLevel");
            ClearData("noiseLevel");
            if (debugMode) Debug.Log("D - NoiseSensing: FAILURE (Target exists)");
            state = NodeState.FAILURE;
            return state;
        }
        else if (aiSensor.hidden)
        {
            ClearData("noisePosition");
            ClearData("noiseLevel");
            if (debugMode) Debug.Log("D - NoiseSensing: FAILURE (AISensor Hidden)");
            state = NodeState.FAILURE;
            return state;
        }
        else if (aiSensor.playerInSight)
        {
            // Clear noise data and set the player as the target
            ClearData("noisePosition");
            ClearData("noiseLevel");
            parent.parent.SetData("target", GameManager.Instance.player.transform);
            if (debugMode) Debug.Log("D - NoiseSensing: FAILURE (Player in sight)");
            state = NodeState.FAILURE;
            return state;
        }
        ////////////////////////////////////////////////////////////////////////

        animator.SetBool("run", false);
        animator.SetBool("walk", true);

        // Retrieve the saved noise level from the blackboard
        object obj2 = GetData("noiseLevel");
        float? noiseLevel = (float?)obj2;

        // If noise level is not available, save the current noise level
        if (noiseLevel == null)
        {
            parent.parent.SetData("noiseLevel", NoiseManager.Instance.noiseLevel);
            noiseLevel = NoiseManager.Instance.noiseLevel;
        }

        // Check if the distance to the player is within the hearing range
        if (Vector3.Distance(transform.position, GameManager.Instance.player.transform.position) <= noiseLevel)
        {
            // Check if there is a collider between the enemy and the player
            RaycastHit hit;
            Vector3 playerPosition = GameManager.Instance.player.transform.position;
            Vector3 directionToPlayer = playerPosition - transform.position;

            // No wall in between, set noise position
            if (!Physics.Linecast(transform.position, playerPosition, out hit, LayerMask.GetMask("Wall")))
            {
                parent.parent.SetData("noisePosition", GameManager.Instance.player.transform.position);

                if (debugMode) Debug.Log("D - NoiseSensing: SUCCESS (Noise heard! No wall in between!)");
                state = NodeState.SUCCESS;
                return state;
            }
            // Wall in between, check if it's within reduced hearing range
            else if (Vector3.Distance(transform.position, playerPosition) <= noiseLevel * noiseReduction)
            {
                parent.parent.SetData("noisePosition", GameManager.Instance.player.transform.position);

                if (debugMode) Debug.Log("D - NoiseSensing: SUCCESS (Noise heard through wall!)");
                state = NodeState.SUCCESS;
                return state;
            }
            else
            {
                parent.parent.ClearData("noiseLevel");
                if (debugMode) Debug.Log("D - NoiseSensing: FAILURE (Noise heard but too far!)");
                state = NodeState.FAILURE;
                return state;
            }
        }

        // No noise heard, clear noise data and return FAILURE
        ClearData("noiseLevel");
        if (debugMode) Debug.Log("D - NoiseSensing: FAILURE (No noise heard!)");
        state = NodeState.FAILURE;
        return state;
    }

    #endregion
}