using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UniqueIDComponent))]
public class Enemy : MonoBehaviour
{
    public int health = 100;
    public bool isDead;

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
    }

    void Die()
    {
        transform.gameObject.SetActive(false);
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
