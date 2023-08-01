using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UniqueIDComponent))]
public class Enemy : MonoBehaviour
{
    public int health = 100;
    public bool isDead;
    [SerializeField] float chaseTimer = 10f;

    void Start() 
    {
        isDead = false;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            isDead = true;
            Die();
        }
        StartCoroutine(EnemyShot());
    }

    void Die()
    {
        transform.gameObject.SetActive(false);
    }

    IEnumerator EnemyShot()
    {
        GameManager.Instance.enemyShot = true;
        GameManager.Instance.shotTarget = transform;
        yield return new WaitForSeconds(chaseTimer);
        GameManager.Instance.enemyShot = false;
    }

     private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        // Calculate the forward direction of the enemy
        Vector3 forwardDirection = transform.forward;

        // Draw the ray in the forward direction with the desired fovRange
        Gizmos.DrawRay(transform.position, forwardDirection * EnemyBT.fovRange);
    }
}
