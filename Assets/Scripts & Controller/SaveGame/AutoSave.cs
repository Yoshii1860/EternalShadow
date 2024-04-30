using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UniqueIDComponent))]
public class AutoSave : MonoBehaviour
{
    #region Variables

    public bool IsActive = true;
    public bool HasCondition = false;
    public bool ConditionMet = false;

    #endregion




    #region Unity Methods

    // This method is called when another Collider enters the trigger zone
    void OnTriggerEnter(Collider other) 
    {
        // Check if the entering collider has the "Player" tag
        if (other.gameObject.CompareTag("Player"))
        {
            if (!HasCondition)
            {
                // Log a message for debugging
                Debug.Log("AutoSave");

                IsActive = false;

                // Trigger the SaveData method in the GameManager
                GameManager.Instance.SaveData();

                GetComponent<Collider>().enabled = false;

                Debug.Log("AutoSave: SaveData - no condition");
            }
            else if (HasCondition)
            {
                if (ConditionMet)
                {
                    // Log a message for debugging
                    Debug.Log("AutoSave");

                    IsActive = false;

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