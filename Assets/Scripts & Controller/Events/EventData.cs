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
    public bool debugMode = false;

    void Update()
    {
        if (debugMode)
        {
            debugMode = false;
            Debug.Log("Event Data Entries:");
            foreach (EventDataEntry eventDataEntry in eventDataEntries)
            {
                Debug.Log("Event Name: " + eventDataEntry.EventName + " - Active: " + eventDataEntry.Active);
            }
        }
    }

    public List<EventDataEntry> eventDataEntries = new List<EventDataEntry>()
    {
        new EventDataEntry() { EventName = "Button", ScriptType = null, Active = false },
        new EventDataEntry() { EventName = "Light", ScriptType = typeof(LightSwitchCode), Active = false },
        new EventDataEntry() { EventName = "Skull", ScriptType = typeof(SkullCode), Active = false },
        new EventDataEntry() { EventName = "Musicbox", ScriptType = null, Active = false },
        new EventDataEntry() { EventName = "Mirror", ScriptType = null, Active = false },
        new EventDataEntry() { EventName = "Bathroom", ScriptType = typeof(BathroomEvent), Active = false },
        new EventDataEntry() { EventName = "Flipped", ScriptType = typeof(CellEvent), Active = false },
        new EventDataEntry() { EventName = "Keypad", ScriptType = typeof(NavKeypad.Keypad), Active = false },
        new EventDataEntry() { EventName = "Rails", ScriptType = typeof(CrowbarCode), Active = false },
        new EventDataEntry() { EventName = "Yard", ScriptType = typeof(YardEvent), Active = false },
        new EventDataEntry() { EventName = "Mannequin", ScriptType = typeof(MannequinEvent), Active = false }
    };   

    public void SetEvent(string eventName)
    {
        EventDataEntry eventDataEntry = eventDataEntries.Find(x => x.EventName == eventName);
        if (eventDataEntry != null)
        {
            eventDataEntry.Active = true;
        }
    }

    public bool CheckEvent(string eventName)
    {
        EventDataEntry eventDataEntry = eventDataEntries.Find(x => x.EventName == eventName);
        if (eventDataEntry != null)
        {
            return eventDataEntry.Active;
        }
        return false;
    }

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
}
