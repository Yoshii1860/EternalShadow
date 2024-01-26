using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class UniqueIDManager
{
    #region Fields

    // This dictionary will store the unique IDs as keys, and the corresponding GameObjects as values
    private static Dictionary<string, GameObject> uniqueIDs = new Dictionary<string, GameObject>();

    #endregion

    #region Constructor

    static UniqueIDManager()
    {
        // Initialize the dictionary with existing unique IDs in the scene
        UniqueIDComponent[] objectsWithUniqueIDs = GameObject.FindObjectsOfType<UniqueIDComponent>();
        foreach (UniqueIDComponent obj in objectsWithUniqueIDs)
        {
            UniqueIDComponent uniqueIDComponent = obj;
            if (!string.IsNullOrEmpty(uniqueIDComponent.UniqueID))
            {
                RegisterUniqueID(uniqueIDComponent.UniqueID, obj.gameObject);
            }
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Register a new unique ID along with the corresponding GameObject.
    /// </summary>
    public static void RegisterUniqueID(string uniqueID, GameObject gameObject)
    {
        if (!uniqueIDs.ContainsKey(uniqueID))
        {
            uniqueIDs.Add(uniqueID, gameObject);
        }
        else
        {
            // If the unique ID already exists, display a warning in the console
            Debug.LogWarning($"Object '{gameObject.name}' has a duplicate unique ID: {uniqueID}");
        }
    }

    /// <summary>
    /// Remove a unique ID entry when an object is destroyed.
    /// </summary>
    public static void UnregisterUniqueID(string uniqueID)
    {
        if (uniqueIDs.ContainsKey(uniqueID))
        {
            uniqueIDs.Remove(uniqueID);
        }
    }

    /// <summary>
    /// Check if a unique ID is already used.
    /// </summary>
    public static bool IsUniqueIDUsed(string uniqueID)
    {
        return uniqueIDs.ContainsKey(uniqueID);
    }

    #endregion
}