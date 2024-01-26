using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    // Singleton pattern to ensure only one instance exists
    public static EnemyManager Instance { get; private set; }

    // Store enemy-related data here
    public Dictionary<Transform, Enemy> enemyDataDictionary = new Dictionary<Transform, Enemy>();

    #region Singleton Pattern

    // Awake is called before Start
    private void Awake()
    {
        // Singleton pattern to ensure only one instance exists
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Keep the EnemyManager between scene changes
        DontDestroyOnLoad(gameObject);
    }

    #endregion

    #region Enemy Manager

    // Start is called before the first frame update
    void Start()
    {
        InitializeEnemyPool();
    }

    // Initialize enemy pool either from GameManager or find by tag
    private void InitializeEnemyPool()
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