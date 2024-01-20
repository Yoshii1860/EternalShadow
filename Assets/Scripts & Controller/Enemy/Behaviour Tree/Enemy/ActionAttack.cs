    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using BehaviorTree;

    public class ActionAttack : Node
    {
        Animator animator;
        Player player = GameManager.Instance.player;

        float attackCounter = 0f;

        public ActionAttack(Transform transform)
        {
            animator = transform.GetComponent<Animator>();
        }

        public override NodeState Evaluate()
        {
            if (GameManager.Instance.isPaused) return NodeState.RUNNING;
            
            object obj = GetData("target");

            if (obj == null)
            {
                state = NodeState.FAILURE;
                return state;
            }

            Transform target = (Transform)obj;

            attackCounter += Time.deltaTime;
            if (attackCounter >= EnemyBT.attackIntervall)
            {
                Debug.Log("ActionAttack: Attack");
                animator.SetBool("Attack", true);
                bool playerIsDead = player.TakeDamage();
                if (playerIsDead)
                {
                    ClearData("target");
                    animator.SetBool("Attack", false);
                    attackCounter = 0f;

                    state = NodeState.SUCCESS;
                    return state;
                }
                else
                {
                    attackCounter = 0f;
                }
            }

            state = NodeState.RUNNING;
            return state;
        }
    }