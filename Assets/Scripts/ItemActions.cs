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
        // Logic for using the potion
        // Logic for using the weapon
    }

    public void Combine(ItemActions otherItem, Item item)
    {
        // Logic for combining this item with another item
    }

    public void Inspect(Item item)
    {
         // Logic for inspecting the item (for riddles)
    }

    public void ThrowAway(Item item)
    {
        // Logic for throwing away the item
    }
}