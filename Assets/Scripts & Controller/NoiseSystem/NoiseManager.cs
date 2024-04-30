using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseManager : MonoBehaviour
{
    #region Singleton

    // Singleton pattern to ensure only one instance exists
    public static NoiseManager Instance { get; private set; }

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




    #region Fields and Properties

    [Header("Noise Data")]
    [Tooltip("The noise data to use for the noise system.")]
    public NoiseData NoiseData;
    [Space(10)]

    [Header("Noise Level")]
    [Tooltip("The current noise level of the player.")]
    public float NoiseLevel = 0f;

    // Constants
    private const float NOISE_COOLDOWN_TIME = 2f;

    // Variables for noise management
    private float _lastNoiseChangeTime = 0f;
    private int _lastNoisePriority = 0;
    private float _currentNoiseData = 0f;
    private float _noiseLevelMultiplier = 0f;

    // Properties
    public float CurrentNoiseData
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

    public float NoiseLevelMultiplier
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

    #endregion




    #region Noise Management

    // Function to update the current noise data based on different sources (surface, hit objects, and shooting)
    public void UpdateCurrentNoiseData(float newNoiseData, int priority) // priority 1 (lowest) to 3 (highest)
    {
        // Check if the new noise data should overwrite the current noise data
        if (Time.time >= _lastNoiseChangeTime + NOISE_COOLDOWN_TIME || priority >= _lastNoisePriority)
        {
            _lastNoisePriority = priority;
            CurrentNoiseData = newNoiseData;
            _lastNoiseChangeTime = Time.time;
        }
    }

    // Function to update the noise level when NoiseLevelMultiplier or CurrentNoiseData changes
    private void UpdateNoiseLevel()
    {
        NoiseLevel = _noiseLevelMultiplier * _currentNoiseData;
    }

    #endregion
}