using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemActions : MonoBehaviour
{
    #region Fields

    private Player player;
    private Potion potion;

    #endregion

    #region Unity Lifecycle Methods

    void Start()
    {
        // Initialize references on Start
        UpdateReferences();
    }

    #endregion

    #region Public Methods

    /// Update references to other components.
    public void UpdateReferences()
    {
        potion = GetComponent<Potion>();
        player = GameManager.Instance.player;
    }

    /// Pick up the item and add it to the inventory.
    /// <param name="item">The RaycastHit containing information about the item.</param>
    public void PickUp(RaycastHit item)
    {
        ItemController itemController = item.collider.gameObject.GetComponent<ItemController>();
        Item itemData = itemController.item;
        Debug.Log("ItemActions.PickUp(" + itemData.name + ")");

        // Add the Item instance to the inventory
        InventoryManager.Instance.AddItem(itemData);
        itemController.isPickedUp = true;
    }

    /// Use the specified item.
    /// <param name="item">The item to be used.</param>
    public void Use(Item item)
    {
        Debug.Log("ItemActions.Use(" + item.name + ")");

        if (item.PotionType == Potion.PotionType.Antibiotics)
            potion.Antibiotics(item);
        else if (item.PotionType == Potion.PotionType.Bandage)
            potion.Bandage(item);
        else if (item.PotionType == Potion.PotionType.Painkillers)
            potion.Painkillers(item);
        else if (item.type == ItemType.Weapon)
            InventoryManager.Instance.weapons.GetComponent<WeaponSwitcher>().SelectWeapon(item);
        else InventoryManager.Instance.DisplayMessage("You cannot use this right now.");
    }

    /*
    /// Combine the specified item.
    /// <param name="item">The item to be combined.</param>
    public void Combine(Item item)
    {
        Debug.Log("Combined: " + item.name);
    }
    */

    /// Inspect the specified item.
    /// <param name="item">The item to be inspected.</param>
    public void Inspect(Item item)
    {
        InventoryManager.Instance.Inspect(item);
    }

    /// Throw away the specified item.
    /// <param name="item">The item to be thrown away.</param>
    public void ThrowAway(Item item)
    {
        if (item.type == ItemType.Object || item.type == ItemType.Weapon)
        {
            InventoryManager.Instance.DisplayMessage("You cannot throw this away.");
            return;
        }
        Debug.Log("ItemActions.ThrowAway(" + item.name + ")");
        InventoryManager.Instance.DropItem(item);
    }

    #endregion
}