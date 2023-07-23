using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int uniqueID;
    public int health = 100;
    public bool isDead;

    void Awake()
    {
        uniqueID = GetInstanceID();
    }

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
}
