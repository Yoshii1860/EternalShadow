using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockerCode : InteractableObject
{
    [Space(10)]
    [Header("RUN ITEM CODE")]
    [SerializeField] Transform door;
    bool insideCollider = false;
    bool open = false;
    bool locked = false;
    // Override the base class method for specific implementation
    protected override void RunItemCode()
    {
        if (!locked) 
        {
            if (!open) StartCoroutine(Open());
            else if (open) StartCoroutine(Close());
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            insideCollider = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            insideCollider = false;
        }
    }

    IEnumerator Open()
    {
        locked = true;

        // slowly open door of the locker
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime;
            door.localEulerAngles = Vector3.Lerp(Vector3.zero, new Vector3(0, -100, 0), t);
            yield return null;
        }

        if (insideCollider)
        {
            foreach (AISensor sensor in GameManager.Instance.enemyPool.GetComponentsInChildren<AISensor>())
            {
                sensor.hidden = false;
            }
        }

        locked = false;
        open = true;
    }

    IEnumerator Close()
    {
        if (insideCollider)
        {
            foreach (AISensor sensor in GameManager.Instance.enemyPool.GetComponentsInChildren<AISensor>())
            {
                Debug.Log("Hiding!!!");
                sensor.hidden = true;
            }
            
        }
        locked = true;
        
        // close the door of the locker
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime;
            door.localRotation = Quaternion.Lerp(door.localRotation, Quaternion.Euler(0, 0, 0), t);
            yield return null;
        }

        locked = false;
        open = false;
    }
}