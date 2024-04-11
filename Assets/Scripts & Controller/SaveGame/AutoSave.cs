using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UniqueIDComponent))]
public class AutoSave : MonoBehaviour
{
    #region Unity Events

    public bool active = true;
    public bool hasCondition = false;
    public bool conditionMet = false;

    // This method is called when another Collider enters the trigger zone
    void OnTriggerEnter(Collider other) 
    {
        // Check if the entering collider has the "Player" tag
        if (other.gameObject.CompareTag("Player"))
        {
            if (!hasCondition)
            {
                // Log a message for debugging
                Debug.Log("AutoSave");

                active = false;

                // Trigger the SaveData method in the GameManager
                GameManager.Instance.SaveData();

                GetComponent<Collider>().enabled = false;

                Debug.Log("AutoSave: SaveData - no condition");
            }
            else if (hasCondition)
            {
                if (conditionMet)
                {
                    // Log a message for debugging
                    Debug.Log("AutoSave");

                    active = false;

                    // Trigger the SaveData method in the GameManager
                    GameManager.Instance.SaveData();

                    GetComponent<Collider>().enabled = false;

                    Debug.Log("AutoSave: SaveData - condition met");
                }
                else
                {
                    // Log a message for debugging
                    Debug.Log("AutoSave: Condition not met");
                }
            }
        }   
    }

    #endregion
}