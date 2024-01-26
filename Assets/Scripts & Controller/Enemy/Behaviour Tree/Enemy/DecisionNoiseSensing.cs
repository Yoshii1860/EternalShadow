using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class DecisionNoiseSensing : Node
{
    #region Fields

    // References to components
    private Transform transform;
    private Animator animator;
    private AISensor aiSensor;

    // Reduction factor for noise level through walls
    private float noiseReduction = 0.6f;

    #endregion

    #region Constructors

    // Constructor to initialize references to components
    public DecisionNoiseSensing(Transform transform)
    {
        this.transform = transform;
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
            // Return FAILURE to indicate that the decision cannot be made
            return NodeState.FAILURE;
        }

        #region Debug Mode
        
        if (GameManager.Instance.noNoiseMode)
        {
            // If in debug mode, return FAILURE
            state = NodeState.FAILURE;
            return state;
        }

        #endregion

        // Retrieve the target from the blackboard
        object obj = GetData("target");

        // Check if the player is in sight or there's a target
        if (obj != null || aiSensor.playerInSight)
        {
            // Clear noise data and return FAILURE
            ClearData("noiseLevel");
            state = NodeState.FAILURE;
            return state;
        }

        // Retrieve the saved noise level from the blackboard
        object obj2 = GetData("noiseLevel");
        float? noiseLevel = (float?)obj2;

        // If noise level is not available, save the current noise level
        if (noiseLevel == null)
        {
            parent.parent.SetData("noiseLevel", NoiseManager.Instance.noiseLevel);
            noiseLevel = NoiseManager.Instance.noiseLevel;
            Debug.Log("DecisionNoiseSensing: NoiseLevel: " + noiseLevel);
        }

        // Check if the distance to the player is within the hearing range
        if (Vector3.Distance(transform.position, GameManager.Instance.player.transform.position) <= noiseLevel)
        {
            Debug.Log("DecisionNoiseSensing: Noise heard!");

            // Check if there is a collider between the enemy and the player
            RaycastHit hit;
            Vector3 playerPosition = GameManager.Instance.player.transform.position;
            Vector3 directionToPlayer = playerPosition - transform.position;

            // No wall in between, set noise position
            if (!Physics.Linecast(transform.position, playerPosition, out hit, LayerMask.GetMask("Wall")))
            {
                Debug.Log("DecisionNoiseSensing: Noise heard! No wall in between!");
                parent.parent.SetData("noisePosition", GameManager.Instance.player.transform.position);

                state = NodeState.SUCCESS;
                return state;
            }
            // Wall in between, check if it's within reduced hearing range
            else if (Vector3.Distance(transform.position, playerPosition) <= noiseLevel * noiseReduction)
            {
                Debug.Log("DecisionNoiseSensing: Noise heard through wall!");
                parent.parent.SetData("noisePosition", GameManager.Instance.player.transform.position);

                state = NodeState.SUCCESS;
                return state;
            }
            else
            {
                Debug.Log("DecisionNoiseSensing: Noise heard! Wall in between - too far away!");
                parent.parent.ClearData("noiseLevel");
                state = NodeState.FAILURE;
                return state;
            }
        }

        // No noise heard, clear noise data and return FAILURE
        Debug.Log("DecisionNoiseSensing: No noise heard!");
        ClearData("noiseLevel");

        state = NodeState.FAILURE;
        return state;
    }

    #endregion
}