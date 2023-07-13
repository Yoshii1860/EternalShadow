using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectHandler : MonoBehaviour
{
    [SerializeField] private InputActionReference input;
    [SerializeField] private float pickupDistance = 5f;

    void Update()
    {
        if (input.action.triggered)
        {
            DetectObjects();
        }
    }

    private void DetectObjects()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, pickupDistance))
        {
            PickupObject pickupObject = hit.collider.gameObject.GetComponent<PickupObject>();
            if (pickupObject != null)
            {
                // use PickUp() method from Item class
                pickupObject.PickUp();
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * pickupDistance);
    }
}
