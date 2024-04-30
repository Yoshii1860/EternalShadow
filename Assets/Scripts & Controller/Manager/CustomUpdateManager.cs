using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region Custom Update Manager Interface

// Interface for custom updatable objects
public interface ICustomUpdatable
{
    // Custom update method with deltaTime parameter
    void CustomUpdate(float deltaTime);
}

#endregion




// Manager class responsible for updating custom updatable objects
public class CustomUpdateManager : MonoBehaviour
{
    #region Fields

    // List to store custom updatable objects
    private List<ICustomUpdatable> customUpdatables = new List<ICustomUpdatable>();
    public bool PrintUpdatables;

    #endregion




    #region Add and Remove Custom Updatables

    // Add a custom updatable object to the list
    public void AddCustomUpdatable(ICustomUpdatable updatable)
    {
        if (!customUpdatables.Contains(updatable))
        {
            customUpdatables.Add(updatable);
        }
    }

    // Remove a custom updatable object from the list
    public void RemoveCustomUpdatable(ICustomUpdatable updatable)
    {
        customUpdatables.Remove(updatable);
    }

    public void ClearCustomUpdatables()
    {
        customUpdatables.Clear();
    }

    #endregion




    #region Unity Update Method

    // Unity's Update method, called every frame
    private void Update()
    {   
        if (PrintUpdatables)
        {
            PrintUpdatables = false;
            string[] allCustomUpdateables = AllCustomUpdateables();
            Debug.Log("All Custom Updateables: " + string.Join(", ", allCustomUpdateables));
        }

        if (GameManager.Instance.IsLoading) return;
        
        // Calculate deltaTime once for all custom updates
        float deltaTime = Time.deltaTime;

        // Call CustomUpdate for all registered scripts
        foreach (var updatable in customUpdatables.ToArray())
        {
            updatable.CustomUpdate(deltaTime);
        }
    }

    #endregion




    #region Get Custom Updatables

    public string[] AllCustomUpdateables()
    {
        string[] allCustomUpdateables = new string[customUpdatables.Count];
        for (int i = 0; i < customUpdatables.Count; i++)
        {
            allCustomUpdateables[i] = customUpdatables[i].ToString();
        }
        return allCustomUpdateables;
    }

    // Get a string representation of all custom updatable objects
    public string GetCustomUpdatables()
    {
        // Initialize an empty string to store updatable names
        string customUpdatablesString = "";

        // Build the string by appending each updatable's name
        foreach (var updatable in customUpdatables)
        {
            customUpdatablesString += updatable.ToString() + ", ";
        }

        // Remove the trailing comma and space
        if (!string.IsNullOrEmpty(customUpdatablesString))
        {
            customUpdatablesString = customUpdatablesString.Substring(0, customUpdatablesString.Length - 2);
        }

        // Return the final string
        return customUpdatablesString;
    }

    #endregion
}