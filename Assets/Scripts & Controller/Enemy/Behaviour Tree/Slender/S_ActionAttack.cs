using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class S_ActionAttack : Node
{
    #region Fields

    // Reference to the animator component.
    private Animator animator;

    private Transform transform;

    // Reference to the player instance.
    private Player player = GameManager.Instance.player;

    // Counter for tracking attack intervals.
    private float attackCounter = 0f;

    // Damage value for the attack.
    private float damage;

    private bool debugMode;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ActionAttack"/> class.
    /// </summary>
    /// <param name="transform">Transform of the attacking entity.</param>
    public S_ActionAttack(bool debugMode, Transform transform)
    {
        this.debugMode = debugMode;
        animator = transform.GetComponent<Animator>();
        damage = transform.GetComponent<Enemy>().damage;
        this.transform = transform;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Evaluates the attack action node.
    /// </summary>
    /// <returns>The resulting state after evaluation.</returns>
    public override NodeState Evaluate()
    {
        if (GameManager.Instance.isPaused)
        {
            if (debugMode) Debug.Log("A - Attack (Game is paused)");
            state = NodeState.FAILURE;
            return state;
        }

        object obj = GetData("target");

        if (obj == null)
        {
            if (debugMode) Debug.Log("A - Attack (No target found)");
            state = NodeState.FAILURE;
            return state;
        }

        Transform target = (Transform)obj;

        attackCounter += Time.deltaTime;
        if (attackCounter >= EnemyBT.attackInterval)
        {
            animator.SetBool("attack", true);

            int i = Random.Range(0, 1);
            if (i >= 0.5f) animator.SetTrigger("left");
            else animator.SetTrigger("right");

            AudioManager.Instance.FadeOut(transform.GetChild(0).gameObject.GetInstanceID(), 0.5f);

            // Perform attack and check if the player is dead.
            bool playerIsDead = player.TakeDamage(damage);

            if (playerIsDead)
            {
                // Player is dead, reset and return success state.
                ClearData("target");
                animator.SetBool("attack", false);
                attackCounter = 0f;

                if (debugMode) Debug.Log("A - Attack (Player is dead)");
                state = NodeState.SUCCESS;
                return state;
            }
            else
            {
                // Player is alive, reset attack counter.
                attackCounter = 0f;
            }
        }

        // Still in the process of attacking.
        if (debugMode) Debug.Log("A - Attack (Attacking)");
        state = NodeState.RUNNING;
        return state;
    }

    #endregion
}