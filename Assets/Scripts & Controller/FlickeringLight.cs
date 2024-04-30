using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlickeringLight : MonoBehaviour, ICustomUpdatable
{
    #region Public Fields

    [Header("Flickering Settings")]
    [Tooltip("Minimum random light intensity")]
    public float MinIntensity = 0f;
    [Tooltip("Maximum random light intensity")]
    public float MaxIntensity = 1f;
    [Tooltip("Smooth out the randomness")]

    [Range(1, 50)]
    public int SmoothingFactor = 5;

    #endregion




    #region Private Fields    

    // Reference to the light component
    private Light _light;

    // Continuous average calculation via FIFO queue
    private Queue<float> _intensityQueue;
    private float _lastSum = 0;

    #endregion





    #region Unity Lifecycle Methods

    void Start()
    {
        _intensityQueue = new Queue<float>(SmoothingFactor);
        _light = GetComponent<Light>();
    }

    #endregion





    #region Flickering Implementation

    /// Reset the randomness and start again
    public void ResetFlicker()
    {
        _intensityQueue.Clear();
        _lastSum = 0f;
    }

    public void CustomUpdate(float deltaTime)
    {
        if (_light == null) return;

        // pop off an item if too big
        while (_intensityQueue.Count >= SmoothingFactor)
        {
            _lastSum -= _intensityQueue.Dequeue();
        }

        // Generate random new item, calculate new average
        float newIntensity = Random.Range(MinIntensity, MaxIntensity);
        _intensityQueue.Enqueue(newIntensity);
        _lastSum += newIntensity;

        // Calculate new smoothed average
        _light.intensity = _lastSum / (float)_intensityQueue.Count;
    }

    #endregion
}