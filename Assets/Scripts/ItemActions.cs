using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemActions : MonoBehaviour
{
    [Tooltip("The player game object.")]
    [SerializeField] Player player;
    Potion potion;

    void Start() 
    {
        potion = GetComponent<Potion>();
    }

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
        if (item.PotionType == Potion.PotionType.Antibiotics)       potion.Antibiotics(item);
        else if (item.PotionType == Potion.PotionType.Bandage)      potion.Bandage(item);
        else if (item.PotionType == Potion.PotionType.Painkillers)  potion.Painkillers(item);
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
        InventoryManager.Instance.DropItem(item);
    }
}