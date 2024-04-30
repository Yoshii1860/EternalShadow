using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System;

public class EventDataEntry
{
    public string EventName;
    public Type ScriptType;
    public bool Active;
}

public class EventData : MonoBehaviour
{
    #region Fields

    [Tooltip("Debug mode will print all event data entries to the console.")]
    [SerializeField] private bool _debugMode = false;

    #endregion




    #region Unity Methods

    // Update is called once per frame - just for debug
    void Update()
    {
        if (_debugMode)
        {
            _debugMode = false;
            Debug.Log("Event Data Entries:");
            foreach (EventDataEntry eventDataEntry in eventDataEntries)
            {
                Debug.Log("Event Name: " + eventDataEntry.EventName + " - Active: " + eventDataEntry.Active);
            }
        }
    }

    #endregion




    #region Event Data

    public List<EventDataEntry> eventDataEntries = new List<EventDataEntry>()
    {
        new EventDataEntry() { EventName = "Button", ScriptType = typeof(ButtonCode), Active = false },
        new EventDataEntry() { EventName = "Light", ScriptType = typeof(LightSwitchCode), Active = false },
        new EventDataEntry() { EventName = "Skull", ScriptType = typeof(SkullCode), Active = false },
        new EventDataEntry() { EventName = "Musicbox", ScriptType = typeof(Musicbox), Active = false },
        new EventDataEntry() { EventName = "Mirror", ScriptType = typeof(ButtonCode), Active = false },
        new EventDataEntry() { EventName = "Bathroom", ScriptType = typeof(BathroomEvent), Active = false },
        new EventDataEntry() { EventName = "Flipped", ScriptType = typeof(FlippedEvent), Active = false },
        new EventDataEntry() { EventName = "Keypad", ScriptType = typeof(NavKeypad.Keypad), Active = false },
        new EventDataEntry() { EventName = "Rails", ScriptType = typeof(CrowbarCode), Active = false },
        new EventDataEntry() { EventName = "Yard", ScriptType = typeof(YardEvent), Active = false },
        new EventDataEntry() { EventName = "Mannequin", ScriptType = typeof(MannequinEvent), Active = false }
    };   

    #endregion




    #region Event Methods

    // Set the event to active
    public void SetEvent(string eventName)
    {
        EventDataEntry eventDataEntry = eventDataEntries.Find(x => x.EventName == eventName);
        if (eventDataEntry != null)
        {
            eventDataEntry.Active = true;
        }
    }

    // Check if the event is active
    public bool CheckEvent(string eventName)
    {
        EventDataEntry eventDataEntry = eventDataEntries.Find(x => x.EventName == eventName);
        if (eventDataEntry != null)
        {
            return eventDataEntry.Active;
        }
        return false;
    }

    // Trigger the event
    public void TriggerEvent(string eventName)
    {
        EventDataEntry eventDataEntry = eventDataEntries.Find(x => x.EventName == eventName);
        if (eventDataEntry != null)
        {
            GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag("EventTrigger");
            foreach (GameObject go in taggedObjects)
            {
                if (go.GetComponent(eventDataEntry.ScriptType) != null)
                {
                    go.GetComponent(eventDataEntry.ScriptType).SendMessage("EventLoad");
                    Debug.Log("Event triggered: " + eventName);
                    break;
                }
            }
        }
    }

    #endregion
}
