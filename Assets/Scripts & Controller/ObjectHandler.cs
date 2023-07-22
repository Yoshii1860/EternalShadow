using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectHandler : MonoBehaviour
{
    // [SerializeField] private InputActionReference input;
    [SerializeField] float pickupDistance = 5f;
    ItemActions action;
    

    void Start() 
    {
        action = GetComponent<ItemActions>();
    }

    public void Execute()
    {
        DetectObjects();
    }

    private void DetectObjects()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, pickupDistance))
        {
            Debug.Log("RaycastHit: " + hit.collider.gameObject.name);
            if (hit.collider.gameObject.GetComponent<ItemController>() != null)
            {
                // Use PickUp() method from ItemActions class
                action.PickUp(hit);
            }
            else if (hit.collider.gameObject.GetComponent<SaveObject>() != null)
            {
                hit.collider.gameObject.GetComponent<SaveObject>().Save();
            }
            else if (hit.collider.gameObject.tag == "Load")
            {
                GameManager.Instance.LoadData(InventoryManager.Instance.player.GetComponent<Player>());
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * pickupDistance);
    }
}
