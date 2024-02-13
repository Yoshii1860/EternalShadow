using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicboxWay : MonoBehaviour
{
    public bool exited = false;


    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Exited");
            exited = true;
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Entered");
            exited = false;
        }
    }
}
