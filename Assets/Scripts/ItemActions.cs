using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemActions : MonoBehaviour
{
    public void PickUp(RaycastHit item)
    {
        Item itemData = item.collider.gameObject.GetComponent<ItemController>().item;
        Debug.Log("Picked up: " + itemData.name);

        // Add the Item instance to the inventory
        InventoryManager.Instance.AddItem(itemData);
        Destroy(item.collider.gameObject);
    }

    public void Use(Item item)
    {
        Debug.Log("Used: " + item.name);
    }

    public void Combine(Item item)
    {
        Debug.Log("Combined: " + item.name);
    }

    public void Inspect(Item item)
    {
        Debug.Log("Inspected: " + item.name);
    }

    public void ThrowAway(Item item)
    {
        Debug.Log("Threw away: " + item.name);
    }
}