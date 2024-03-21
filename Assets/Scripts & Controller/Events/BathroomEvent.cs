using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BathroomEvent : MonoBehaviour
{
    [SerializeField] Animator girl;

    void OnTriggerEnter(Collider other) 
    {
        if (other.CompareTag("Player"))
        {
            girl.SetTrigger("GetOut");
            Invoke("StartGirl", 7f);
        }
    }

    void StartGirl()
    {
        girl.GetComponent<AISensor>().hidden = false;
        girl.GetComponent<EnemyBT>().enabled = true;
        girl.GetComponentInChildren<Collider>().enabled = true;
    }
}

