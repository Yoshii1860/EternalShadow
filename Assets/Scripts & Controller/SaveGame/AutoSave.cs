using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoSave : MonoBehaviour
{
    #region Unity Events

    // This method is called when another Collider enters the trigger zone
    void OnTriggerEnter(Collider other) 
    {
        // Check if the entering collider has the "Player" tag
        if (other.gameObject.CompareTag("Player"))
        {
            // Log a message for debugging
            Debug.Log("AutoSave");

            // Trigger the SaveData method in the GameManager
            GameManager.Instance.SaveData();
        }   
    }

    #endregion
}