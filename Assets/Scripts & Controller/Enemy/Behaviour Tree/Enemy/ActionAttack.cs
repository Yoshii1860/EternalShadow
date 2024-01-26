using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class ActionAttack : Node
{
    #region Fields

    // Reference to the animator component.
    private Animator animator;

    // Reference to the player instance.
    private Player player = GameManager.Instance.player;

    // Counter for tracking attack intervals.
    private float attackCounter = 0f;

    // Damage value for the attack.
    private float damage;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ActionAttack"/> class.
    /// </summary>
    /// <param name="transform">Transform of the attacking entity.</param>
    public ActionAttack(Transform transform)
    {
        animator = transform.GetComponent<Animator>();
        damage = transform.GetComponent<Enemy>().damage;
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
            return NodeState.RUNNING;
        }

        object obj = GetData("target");

        if (obj == null)
        {
            state = NodeState.FAILURE;
            return state;
        }

        Transform target = (Transform)obj;

        attackCounter += Time.deltaTime;
        if (attackCounter >= EnemyBT.attackInterval)
        {
            Debug.Log("ActionAttack: Attack");
            animator.SetBool("Attack", true);

            // Perform attack and check if the player is dead.
            bool playerIsDead = player.TakeDamage(damage);

            if (playerIsDead)
            {
                // Player is dead, reset and return success state.
                ClearData("target");
                animator.SetBool("Attack", false);
                attackCounter = 0f;

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
        state = NodeState.RUNNING;
        return state;
    }

    #endregion
}