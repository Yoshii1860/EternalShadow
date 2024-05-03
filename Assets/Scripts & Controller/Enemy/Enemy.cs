using System.Collections;
using UnityEngine;

#region Enemy Type Enum

public enum EnemyType { SLENDER, GIRL }

#endregion

[RequireComponent(typeof(UniqueIDComponent))]
[RequireComponent(typeof(AISensor))]
public class Enemy : MonoBehaviour
{
    #region Fields

    public EnemyType EnemyType;

    [Header("Enemy Properties")]
    public int HealthPoints = 100;
    public float EnemyDamage = 10f;
    public bool IsDead = false;
    public bool IsShot = false;
    [Space(10)]

    [Header("Enemy Attributes for BT")]
    public float WalkSpeed = 0.4f;
    public float RunSpeed = 2.1f;
    public float AttackRange = 3f;
    public float AttackInterval = 1f;
    public float MaxChaseRange = 15f;
    [Space(10)]

    [Header("Enemy Stun Properties")]
    [Tooltip("The time the enemy stays down after being shot.")]
    [SerializeField] private float _stunDuration = 30f;
    [Tooltip("The time the enemy starts with get up animation after being shot.")]
    [SerializeField] private float _getUpDelay = 25f;

    #endregion




    #region Public Methods

    /// Inflict damage to the enemy.
    public void TakeDamage(int EnemyDamage)
    {
        HealthPoints -= EnemyDamage;
        
        // Check if the enemy has no health left
        if (HealthPoints <= 0)
        {
            IsDead = true;
            Die();
        }

        // Trigger the enemy shot coroutine
        StartCoroutine(EnemyShot());
    }

    #endregion




    #region Private Methods

    /// Handles the death of the enemy.
    private void Die()
    {
        // Deactivate the enemy game object
        gameObject.SetActive(false);
    }

    #endregion




    #region Coroutines

    /// Coroutine to handle the enemy being shot and the chase timer.
    private IEnumerator EnemyShot()
    {
        // Set the enemy as shot
        IsShot = true;

        transform.GetComponent<Animator>().SetTrigger("die");

        StartCoroutine(EnemyGetUp());

        // Wait for the specified chase timer duration
        yield return new WaitForSeconds(_stunDuration);

        // Reset the shot state
        IsShot = false;
    }

    private IEnumerator EnemyGetUp()
    {
        yield return new WaitForSeconds(_getUpDelay);
        transform.GetComponent<Animator>().SetTrigger("getup");
    }

    #endregion
}