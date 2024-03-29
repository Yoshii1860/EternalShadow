using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class ActionAttack : Node
{
    #region Fields

    // Reference to the animator component.
    private Animator animator;
    private Transform transform;
    // Reference to the player instance.
    private Player player = GameManager.Instance.player;
    private AISensor aiSensor;

    // Counter for tracking attack intervals.
    private float attackCounter = 0f;

    // Damage value for the attack.
    private float damage;

    private bool debugMode;

    #endregion

    #region Constructors

    /// Initializes a new instance of the <see cref="ActionAttack"/> class.
    public ActionAttack(bool debugMode, Transform transform)
    {
        this.debugMode = debugMode;
        animator = transform.GetComponent<Animator>();
        damage = transform.GetComponent<Enemy>().damage;
        this.transform = transform;
        aiSensor = transform.GetComponent<AISensor>();
    }

    #endregion

    #region Public Methods

    /// Evaluates the attack action node.
    public override NodeState Evaluate()
    {

        ////////////////////////////////////////////////////////////////////////
        // PAUSE GAME
        ////////////////////////////////////////////////////////////////////////
        if (GameManager.Instance.isPaused)
        {
            // Return RUNNING to indicate that the decision is ongoing
            if (debugMode) Debug.Log("A - ActionAttack: FAILURE (Paused)");
            state = NodeState.FAILURE;
            return state;
        }
        ////////////////////////////////////////////////////////////////////////

        object obj = GetData("target");

        if (obj == null)
        {
            if (debugMode) Debug.Log("A - Attack (No target found)");
            state = NodeState.FAILURE;
            return state;
        }
        else if (aiSensor.hidden)
        {
            if (debugMode) Debug.Log("A - Attack (Hidden)");
            ClearData("target");
            state = NodeState.FAILURE;
            return state;
        }
        ////////////////////////////////////////////////////////////////////////

        Transform target = (Transform)obj;

        attackCounter += Time.deltaTime;
        if (attackCounter >= EnemyBT.attackInterval)
        {
            animator.SetBool("attack", true);

            // Perform attack and check if the player is dead.
            // Chances to apply status effects on the player.
            bool playerIsDead = player.TakeDamage(damage);

            int randomizer = Random.Range(0, 100);
            if (randomizer <= 15)
            {
                if (!player.isPoisoned) player.Poisoned();
            }
            randomizer = Random.Range(0, 100);
            if (randomizer <= 10)
            {
                if (!player.isBleeding) player.Bleeding();
            }

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