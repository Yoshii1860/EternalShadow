using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System;

public class EventData : MonoBehaviour
{
    public string[] eventNames;
    public Dictionary<string, bool> eventDictionary;
    public Dictionary<string, string> scriptNames = new Dictionary<string, string>(); // Map event names to script names

    void Start()
    {
        eventDictionary = new Dictionary<string, bool>();

        for (int i = 0; i < eventNames.Length; i++)
        {
            eventDictionary.Add(eventNames[i], false);
        }

        // Add the script names to the dictionary
        for (int i = 0; i < eventNames.Length; i++)
        {
            switch (eventNames[i])
            {
                case "Button":
                    AddScript(eventNames[i], "");
                    break;
                case "Light":
                    AddScript(eventNames[i], "LightSwitchCode");
                    break;
                case "Skull":
                    AddScript(eventNames[i], "SkullCode");
                    break;
                case "Musicbox":
                    AddScript(eventNames[i], "");
                    break;
                case "Mirror":
                    AddScript(eventNames[i], "");
                    break;
                case "Flipped":
                    AddScript(eventNames[i], "CellEvent");
                    break;
                case "Keypad":
                    AddScript(eventNames[i], "Keypad");
                    break;
                case "Rails":
                    AddScript(eventNames[i], "CrowbarCode");
                    break;
                case "Yard":
                    AddScript(eventNames[i], "YardEvent");
                    break;
                case "Mannequin":
                    AddScript(eventNames[i], "MannequinEvent");
                    break;
                default:
                    Debug.LogWarning($"No script found for event '{eventNames[i]}'.");
                    break;
            }
        }
    }

    public void SetEvent(string eventName)
    {
        eventDictionary[eventName] = true;
    }

    public bool CheckEvent(string eventName)
    {
        return eventDictionary[eventName];
    }

    public void AddScript(string eventName, string scriptName)
    {
        scriptNames.Add(eventName, scriptName);
    }

    public void TriggerEvent(string eventName)
    {
        if (eventDictionary.TryGetValue(eventName, out bool isActive) && isActive)
        {
            // Get the script name from the dictionary
            if (scriptNames.TryGetValue(eventName, out string scriptName))
            {
                if (string.IsNullOrEmpty(scriptName))
                {
                    Debug.LogWarning($"No script found for event '{eventName}'.");
                    return;
                }
                
                // Load the script type dynamically
                Type scriptType = Type.GetType(scriptName);
                if (scriptType == null)
                {
                    Debug.LogError($"Script '{scriptName}' not found.");
                    return;
                }

                // Call the EventLoad method
                MethodInfo methodInfo = scriptType.GetMethod("EventLoad");
                if (methodInfo != null)
                {
                    // Find the GameObject with the script (assuming a single instance)
                    GameObject scriptObject = (GameObject)GameObject.FindObjectOfType(scriptType);  // Add explicit cast to GameObject
                    if (scriptObject != null)
                    {
                        methodInfo.Invoke(scriptObject.GetComponent(scriptType), null); // Invoke the method
                    }
                    else
                    {
                        Debug.LogError($"GameObject with script '{scriptName}' not found.");
                    }
                }
                else
                {
                    Debug.LogWarning($"Script '{scriptName}' does not have a 'EventLoad' method.");
                }
            }
            else
            {
                Debug.LogError($"Script name not found for event '{eventName}'.");
            }
        }
    }
}