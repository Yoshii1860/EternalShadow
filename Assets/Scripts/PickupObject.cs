using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupObject : MonoBehaviour
{
    [Tooltip("The type of item this object is.")]
    public ItemType type;

    public enum ItemType
    {
        Weapon,
        Bullet,
        Potion,
        Object
    }

    public void PickUp()
    {
        // Logic for picking up the item
        Debug.Log("Picked up: " + gameObject.name);
    }

    public void Use()
    {
        if (type == ItemType.Potion)
        {
            // Logic for using the potion
        }
        else if (type == ItemType.Object)
        {
            // Logic for using the weapon
        }
    }

    public void Combine(PickupObject otherItem)
    {
        if (type == ItemType.Object)
        {
            // Logic for combining this item with another item
        }
    }

    public void Inspect()
    {
        if (type == ItemType.Object)
        {
            // Logic for inspecting the item (for riddles)
        }
    }

    public void ThrowAway()
    {
        // Logic for throwing away the item
    }
}
