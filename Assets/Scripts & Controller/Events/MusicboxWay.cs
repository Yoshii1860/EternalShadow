using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicboxWay : MonoBehaviour
{
    #region Variables

    public bool Exited = false;

    #endregion




    #region Collider

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Exited");
            Exited = true;
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Entered");
            Exited = false;
        }
    }

    #endregion
}
