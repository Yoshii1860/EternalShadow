using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Potion: MonoBehaviour
{
    [Tooltip("The player game object.")]
    [SerializeField] Player player;

    public enum PotionType
    {
        None,
        Antibiotics,
        Bandage,
        Painkillers
    }

    // Called by ItemActions.cs
    // If the player is poisoned/bleeding/dizzy, remove the poison
    // Remove the item from the inventory
    
    public void Antibiotics(Item item)
    {
        if (player.isPoisoned == true)
        {
            player.isPoisoned = false;
            InventoryManager.Instance.RemoveItem(item);
        }
        else
        {
            Debug.Log("Not poisoned");
        }
    }

    public void Bandage(Item item)
    {
        if (player.isBleeding == true)
        {
            player.isBleeding = false;
            InventoryManager.Instance.RemoveItem(item);
        }
        else
        {
            Debug.Log("Not bleeding");
        }
    }

    public void Painkillers(Item item)
    {
        if (player.isDizzy == true)
        {
            player.isDizzy = false;
            InventoryManager.Instance.RemoveItem(item);
        }
        else
        {
            Debug.Log("Not dizzy");
        }
    }
}
