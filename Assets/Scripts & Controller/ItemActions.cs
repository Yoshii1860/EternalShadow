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
        ItemController itemController = item.collider.gameObject.GetComponent<ItemController>();
        Item itemData = itemController.item;
        Debug.Log("ItemActions.PickUp(" + itemData.name + ")");

        // Add the Item instance to the inventory
        InventoryManager.Instance.AddItem(itemData);
        itemController.isPickedUp = true;
    }

    public void Use(Item item)
    {
        Debug.Log("ItemActions.Use(" + item.name + ")");
        if (item.PotionType == Potion.PotionType.Antibiotics)       potion.Antibiotics(item);
        else if (item.PotionType == Potion.PotionType.Bandage)      potion.Bandage(item);
        else if (item.PotionType == Potion.PotionType.Painkillers)  potion.Painkillers(item);
        else if (item.type == ItemType.Weapon)                      InventoryManager.Instance.weapons.GetComponent<WeaponSwitcher>().SelectWeapon(item);
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
        Debug.Log("ItemActions.ThrowAway(" + item.name + ")");
        InventoryManager.Instance.DropItem(item);
    }
}