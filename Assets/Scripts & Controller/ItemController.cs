using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemController : MonoBehaviour
{
    public Item item;
    public int uniqueID;
    public Vector3 originalPosition;
    bool pickedUpState;

    public bool isPickedUp
    {
        get => pickedUpState;
        set
        {
            pickedUpState = value;
            // Whenever isPickedUp is changed, update the transform.position
            if (pickedUpState)
            {
                // Move the object to a position far away
                transform.position = new Vector3(0, -1000f, 0);
            }
            else
            {
                // If it's not picked up, reset its position to its initial position.
                transform.position = originalPosition;
            }
        }
    }

    void Awake()
    {
        uniqueID = GetInstanceID();
    }

    void Start() 
    {
        pickedUpState = false;
        originalPosition = transform.position;
    }
}
