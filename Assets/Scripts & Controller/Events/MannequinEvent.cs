using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MannequinEvent : MonoBehaviour
{
    [SerializeField] Mannequin firstMannequin;
    [SerializeField] Mannequin secondMannequin;
    [SerializeField] Weapon weapon;
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
                if (weapon.isAvailable)
                {
                    GameManager.Instance.eventData.SetEvent("Mannequin");
                    firstMannequin.move = true;
                    secondMannequin.move = true;
                    Destroy(gameObject);
                }
            }
        }
    }

    public void EventLoad()
    {
        if (firstMannequin.gameObject.activeSelf)
        {
            firstMannequin.started = true;
            firstMannequin.move = true;
        }

        if (secondMannequin.gameObject.activeSelf)
        {
            secondMannequin.started = true;
            secondMannequin.move = true;
        }
    }
}
