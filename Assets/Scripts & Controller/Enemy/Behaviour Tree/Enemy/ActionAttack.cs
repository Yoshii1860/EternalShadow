using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class ActionAttack : Node
{
    #region Fields

    // References
    private Animator _animator;
    private AISensor _aiSensor;

    // variables for attack behavior
    private float _attackCounter = 0f;
    private float _attackInterval;

    private bool _debugMode;

    #endregion




    #region Constructors

    public ActionAttack(bool debugMode, Transform transform)
    {
        _debugMode = debugMode;
        _animator = transform.GetComponent<Animator>();
        _attackInterval = transform.GetComponent<Enemy>().AttackInterval;
        _aiSensor = transform.GetComponent<AISensor>();
    }

    #endregion




    #region Public Methods

    public override NodeState Evaluate()
    {

        #region FAILURE CHECKS

        if (GameManager.Instance.IsGamePaused)
        {
            if (_debugMode) Debug.Log("A - ActionAttack: FAILURE (Paused)");
            return NodeState.FAILURE;
        }

        if (_aiSensor.IsPlayerHidden)
        {
            if (_debugMode) Debug.Log("A - Attack (Hidden)");
            ClearData("target");
            return NodeState.FAILURE;
        }

        object obj = GetData("target");
        if (obj == null)
        {
            if (_debugMode) Debug.Log("A - Attack (No target found)");
            return NodeState.FAILURE;
        }
        
        #endregion




        #region ACTION

        Transform target = (Transform)obj;

        _attackCounter += Time.deltaTime;
        if (_attackCounter >= _attackInterval)
        {       
            bool playerIsDead = GameManager.Instance.Player.IsDead;

            if (playerIsDead)
            {
                ClearData("target");
                _animator.SetBool("attack", false);
                _attackCounter = 0f;

                if (_debugMode) Debug.Log("A - Attack (Player is dead)");
                return NodeState.SUCCESS;
            }

            _animator.SetTrigger("attack");

            int randomizer = Random.Range(0, 100);
            if (randomizer <= 15)
            {
                if (!GameManager.Instance.Player.IsDizzy) GameManager.Instance.Player.Dizzy();
            }
            randomizer = Random.Range(0, 100);
            if (randomizer <= 10)
            {
                if (!GameManager.Instance.Player.IsBleeding) GameManager.Instance.Player.Bleeding();
            }

            _attackCounter = 0f;
        }

        if (_debugMode) Debug.Log("A - Attack (Attacking)");
        return NodeState.RUNNING;

        #endregion
    }

    #endregion
}