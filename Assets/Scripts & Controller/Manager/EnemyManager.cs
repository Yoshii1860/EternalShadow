using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    static EnemyManager instance;

    // Singleton pattern to ensure only one instance exists
    public static EnemyManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<EnemyManager>();

                if (instance == null)
                {
                    GameObject obj = new GameObject("EnemyManager");
                    instance = obj.AddComponent<EnemyManager>();
                }
            }

            return instance;
        }
    }

    // Store enemy-related data here
    public Dictionary<Transform, Enemy> enemyDataDictionary = new Dictionary<Transform, Enemy>();

    #region Enemy Manager

    // Initialize enemy pool either from GameManager or find by tag
    public void InitializeEnemyPool()
    {
        if (GameManager.Instance.enemyPool != null)
        {
            FillEnemyArray(GameManager.Instance.enemyPool);
        }
        else
        {
            Transform enemyPool = GameObject.FindGameObjectWithTag("EnemyPool").transform;
            FillEnemyArray(enemyPool);
        }
    }

    // Fill the enemy array with data from the enemy pool
    private void FillEnemyArray(Transform enemyPool)
    {
        foreach (Transform enemyTransform in enemyPool)
        {
            Enemy enemyData = enemyTransform.GetComponent<Enemy>();
            AddOrUpdateEnemy(enemyTransform, enemyData);
        }
    }

    // Function to add or update enemy data
    public void AddOrUpdateEnemy(Transform enemyTransform, Enemy enemyData)
    {
        if (enemyDataDictionary.ContainsKey(enemyTransform))
        {
            enemyDataDictionary[enemyTransform] = enemyData;
        }
        else
        {
            enemyDataDictionary.Add(enemyTransform, enemyData);
        }
    }

    // Function to get enemy data based on enemy's transform
    public bool TryGetEnemy(Transform enemyTransform, out Enemy enemyData)
    {
        return enemyDataDictionary.TryGetValue(enemyTransform, out enemyData);
    }

    #endregion
}