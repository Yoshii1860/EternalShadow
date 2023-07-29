using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class UniqueIDManager
{
    // This dictionary will store the unique IDs as keys, and the corresponding GameObjects as values
    private static Dictionary<string, GameObject> uniqueIDs = new Dictionary<string, GameObject>();

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

    // Register a new unique ID along with the corresponding GameObject
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

    // Remove a unique ID entry when an object is destroyed
    public static void UnregisterUniqueID(string uniqueID)
    {
        if (uniqueIDs.ContainsKey(uniqueID))
        {
            uniqueIDs.Remove(uniqueID);
        }
    }

    // Check if a unique ID is already used
    public static bool IsUniqueIDUsed(string uniqueID)
    {
        return uniqueIDs.ContainsKey(uniqueID);
    }
}