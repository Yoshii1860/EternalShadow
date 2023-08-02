using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseManager : MonoBehaviour
{
    // Singleton pattern to ensure only one instance exists
    public static NoiseManager Instance { get; private set; }

    public NoiseData noiseData;

    private const float NoiseCooldownTime = 2f;
    private float lastNoiseChangeTime = 0f;
    
    private int lastNoisePriority = 0;

    // Backing fields for properties
    private float _currentNoiseData = 0f;
    private float _noiseLevelMultiplier = 0f;

    public float noiseLevel = 0f;
    
    // Properties
    public float currentNoiseData
    {
        get => _currentNoiseData;
        set
        {
            if (_currentNoiseData != value)
            {
                _currentNoiseData = value;
                UpdateNoiseLevel();
            }
        }
    }
    
    public float noiseLevelMultiplier
    {
        get => _noiseLevelMultiplier;
        set
        {
            if (_noiseLevelMultiplier != value)
            {
                _noiseLevelMultiplier = value;
                UpdateNoiseLevel();
            }
        }
    }

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

    // Function to update the current noise data based on different sources (surface, hit objects, and shooting)
    public void UpdateCurrentNoiseData(float newNoiseData, int priority) // priority 1 (lowest) to 3 (highest)
    {
        // Check if the new noise data should overwrite the current noise data
        if (Time.time >= lastNoiseChangeTime + NoiseCooldownTime || priority >= lastNoisePriority)
        {
            lastNoisePriority = priority;
            currentNoiseData = newNoiseData;
            lastNoiseChangeTime = Time.time;
        }
    }

    // Function to update the noise level when noiseLevelMultiplier or currentNoiseData changes
    private void UpdateNoiseLevel()
    {
        noiseLevel = _noiseLevelMultiplier * _currentNoiseData;
    }
}