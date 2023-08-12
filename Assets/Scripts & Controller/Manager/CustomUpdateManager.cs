using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICustomUpdatable
{
    void CustomUpdate(float deltaTime);
}

public class CustomUpdateManager : MonoBehaviour
{
    private List<ICustomUpdatable> customUpdatables = new List<ICustomUpdatable>();

    public void AddCustomUpdatable(ICustomUpdatable updatable)
    {
        if (!customUpdatables.Contains(updatable))
        {
            customUpdatables.Add(updatable);
        }
    }

    public void RemoveCustomUpdatable(ICustomUpdatable updatable)
    {
        customUpdatables.Remove(updatable);
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;

        // Call CustomUpdate for all registered scripts
        foreach (var updatable in customUpdatables)
        {
            updatable.CustomUpdate(deltaTime);
        }
    }

    public string GetCustomUpdatables()
    {
        string customUpdatablesString = "";

        foreach (var updatable in customUpdatables)
        {
            customUpdatablesString += updatable.ToString() + ", ";
        }

        customUpdatablesString = customUpdatablesString.Substring(0, customUpdatablesString.Length - 2);

        return customUpdatablesString;
    }
}