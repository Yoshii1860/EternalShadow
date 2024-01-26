using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlickeringLight : MonoBehaviour, ICustomUpdatable
{
    #region Public Fields

    [Tooltip("Minimum random light intensity")]
    public float minIntensity = 0f;

    [Tooltip("Maximum random light intensity")]
    public float maxIntensity = 1f;

    [Tooltip("Smooth out the randomness")]
    [Range(1, 50)]
    public int smoothing = 5;

    #endregion

    #region Private Fields    

    private Light mainLight;

    // Continuous average calculation via FIFO queue
    // Saves us iterating every time we update; we just change by the delta
    private Queue<float> smoothQueue;
    private float lastSum = 0;

    #endregion

    #region Unity Lifecycle Methods

    void Start()
    {
        smoothQueue = new Queue<float>(smoothing);
        mainLight = GetComponent<Light>();
    }

    #endregion

    #region Flickering Implementation

    /// <summary>
    /// Reset the randomness and start again. You usually don't need to call
    /// this; deactivating/reactivating is usually fine, but if you want a strict
    /// restart, you can do.
    /// </summary>
    public void Reset()
    {
        smoothQueue.Clear();
        lastSum = 0;
    }

    public void CustomUpdate(float deltaTime)
    {
        if (mainLight == null)
            return;

        // pop off an item if too big
        while (smoothQueue.Count >= smoothing)
        {
            lastSum -= smoothQueue.Dequeue();
        }

        // Generate random new item, calculate new average
        float newVal = Random.Range(minIntensity, maxIntensity);
        smoothQueue.Enqueue(newVal);
        lastSum += newVal;

        // Calculate new smoothed average
        mainLight.intensity = lastSum / (float)smoothQueue.Count;
    }

    #endregion
}