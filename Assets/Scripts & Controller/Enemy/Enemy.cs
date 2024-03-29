using System.Collections;
using UnityEngine;

[RequireComponent(typeof(UniqueIDComponent))]
[RequireComponent(typeof(AISensor))]
public class Enemy : MonoBehaviour
{
    #region Fields

    public int health = 100;
    public bool isDead = false;
    public bool isShot = false;
    public float damage = 10f;
    [Tooltip("The time the enemy stays down after being shot.")]
    [SerializeField] float downTimer = 30f;
    [Tooltip("The time the enemy starts with get up animation after being shot.")]
    [SerializeField] float reducedTimer = 25f;

    #endregion

    #region Public Methods

    /// Inflict damage to the enemy.
    public void TakeDamage(int damage)
    {
        health -= damage;
        
        // Check if the enemy has no health left
        if (health <= 0)
        {
            isDead = true;
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

    /// Coroutine to handle the enemy being shot and the chase timer.
    private IEnumerator EnemyShot()
    {
        // Set the enemy as shot
        isShot = true;

        transform.GetComponent<Animator>().SetTrigger("die");

        StartCoroutine(EnemyGetUp());

        // Wait for the specified chase timer duration
        yield return new WaitForSeconds(downTimer);

        // Reset the shot state
        isShot = false;
    }

    IEnumerator EnemyGetUp()
    {
        yield return new WaitForSeconds(reducedTimer);
        transform.GetComponent<Animator>().SetTrigger("getup");
    }

    #endregion
}