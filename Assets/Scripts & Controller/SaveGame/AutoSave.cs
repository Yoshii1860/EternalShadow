using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoSave : MonoBehaviour
{
    void OnTriggerEnter(Collider other) 
    {
        if(other.gameObject.tag == "Player")
        {
            Debug.Log("AutoSave");
            GameManager.Instance.SaveData();
        }   
    }
}
