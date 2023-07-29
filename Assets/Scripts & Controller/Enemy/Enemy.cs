using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UniqueIDComponent))]
public class Enemy : MonoBehaviour
{
    public int health = 100;
    public bool isDead;
    public EnemyFOVVisualizer fovVisualizer;

    void Start() 
    {
        isDead = false;
        fovVisualizer = GameObject.Find("EnemyFOVVisualizerHolder").GetComponent<EnemyFOVVisualizer>();
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
        Gizmos.color = Color.green;

        // Calculate the forward direction of the enemy
        Vector3 forwardDirection = transform.forward;

        // Draw the ray in the forward direction with the desired fovRange
        Gizmos.DrawRay(transform.position, forwardDirection * EnemyBT.fovRange);

        int rayCount = 30; // Adjust this value to set the number of rays in the FOV cone
        float meshHeight = 1f; // Adjust this value to set the height of the FOV cone mesh
        fovVisualizer.DrawFOVVisualization(transform, EnemyBT.fovRange, EnemyBT.fovAngle, rayCount, meshHeight);
    }
}
