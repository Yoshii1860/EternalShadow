using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MannequinEvent : MonoBehaviour
{
    [SerializeField] Mannequin firstMannequin;
    [SerializeField] Mannequin secondMannequin;
    bool firstEnter = false;

    void OnTriggerExit(Collider other) 
    {
        if (other.tag == "Player")
        {
            if (!firstEnter)
            {
                firstEnter = true;
                firstMannequin.started = true;
                secondMannequin.started = true;
            }
            else
            {
                firstMannequin.move = true;
                secondMannequin.move = true;
                Destroy(gameObject);
            }
        }
    }
}
