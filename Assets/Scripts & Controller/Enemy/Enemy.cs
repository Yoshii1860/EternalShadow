using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UniqueIDComponent))]
[RequireComponent(typeof(AISensor))]
public class Enemy : MonoBehaviour
{
    public int health = 100;
    public bool isDead = false;
    public bool isShot = false;
    [SerializeField] float chaseTimer = 10f;

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
        isShot = true;
        yield return new WaitForSeconds(chaseTimer);
        isShot = false;
    }
}
