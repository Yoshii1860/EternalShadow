using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class DecisionNoiseSensing : Node
{
    #region Fields

    // References to components
    private Transform _transform;
    private Animator _animator;
    private AISensor _aiSensor;

    // Reduction factor for noise level through walls
    private float _noiseReduction = 0.6f;

    private bool _debugMode;

    #endregion




    #region Constructors

    // Constructor to initialize references to components
    public DecisionNoiseSensing(bool debugMode, Transform transform)
    {
        _transform = transform;
        _animator = transform.GetComponent<Animator>();
        _aiSensor = transform.GetComponent<AISensor>();
        _debugMode = debugMode;
    }

    #endregion




    #region Public Methods

    // Evaluate method to determine the state of the node
    public override NodeState Evaluate()
    {

        #region FAILURE CHECKS

        if (GameManager.Instance.IsGamePaused || GameManager.Instance.NoNoiseMode)
        {
            // Return FAILURE to indicate that the decision cannot be made
            if (_debugMode) Debug.Log("D - NoiseSensing: FAILURE (Game is paused or no noise mode active)");
            return NodeState.FAILURE;
        }

        if (GetData("target") != null || _aiSensor.IsPlayerHidden)
        {
            // Clear noise data and return FAILURE
            ClearData("noisePosition");
            ClearData("noiseLevel");
            if (_debugMode) Debug.Log("D - NoiseSensing: FAILURE (target exists or player hidden)");
            return NodeState.FAILURE;
        }

        if (_aiSensor.IsPlayerInSight)
        {
            // Clear noise data and set the player as the target
            ClearData("noisePosition");
            ClearData("noiseLevel");
            parent.parent.SetData("target", GameManager.Instance.Player.transform);
            if (_debugMode) Debug.Log("D - NoiseSensing: FAILURE (Player in sight)");
            return NodeState.FAILURE;
        }
        
        #endregion




        #region DECISION

        _animator.SetBool("run", false);
        _animator.SetBool("walk", true);

        object noiseObj = GetData("noiseLevel");
        float? noiseLevel = (float?)noiseObj;

        // If noise level is not available, save the current noise level
        if (noiseLevel == null)
        {
            parent.parent.SetData("noiseLevel", NoiseManager.Instance.NoiseLevel);
            noiseLevel = NoiseManager.Instance.NoiseLevel;
        }
        else
        {
            if (_debugMode) Debug.Log("D - NoiseSensing: Noise Level Available");
            return NodeState.SUCCESS;
        }

        // Check if the distance to the player is within the hearing range
        if (Vector3.Distance(_transform.position, GameManager.Instance.Player.transform.position) <= noiseLevel)
        {
            // Check if there is a collider between the enemy and the player
            RaycastHit hit;
            Vector3 playerPosition = GameManager.Instance.Player.transform.position;
            Vector3 directionToPlayer = playerPosition - _transform.position;

            // No wall in between, set noise position
            if (!Physics.Linecast(_transform.position, playerPosition, out hit, LayerMask.GetMask("Wall")))
            {
                parent.parent.SetData("noisePosition", GameManager.Instance.Player.transform.position);
                
                if (_debugMode) Debug.Log("D - NoiseSensing: SUCCESS (Noise heard! No wall in between!)");
                return NodeState.SUCCESS;
            }
            // Wall in between, check if it's within reduced hearing range
            else if (Vector3.Distance(_transform.position, playerPosition) <= noiseLevel * _noiseReduction)
            {
                parent.parent.SetData("noisePosition", GameManager.Instance.Player.transform.position);

                if (_debugMode) Debug.Log("D - NoiseSensing: SUCCESS (Noise heard through wall!)");
                return NodeState.SUCCESS;
            }
            else
            {
                parent.parent.ClearData("noiseLevel");
                if (_debugMode) Debug.Log("D - NoiseSensing: FAILURE (Noise heard but too far!)");
                return NodeState.FAILURE;
            }
        }

        ClearData("noiseLevel");
        if (_debugMode) Debug.Log("D - NoiseSensing: FAILURE (No noise heard!)");
        return NodeState.FAILURE;

        #endregion
    }

    #endregion
}