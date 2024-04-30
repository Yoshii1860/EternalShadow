using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemActions : MonoBehaviour
{
    #region Fields

    // Reference to the potion component
    private Potion _potion;

    #endregion




    #region Unity Lifecycle Methods

    void Start()
    {
        // Initialize references on Start
        UpdateReferences();
    }

    #endregion




    #region Public Methods

    /// Update references to other components. To be called when the GameManager is initialized.
    public void UpdateReferences()
    {
        _potion = GetComponent<Potion>();
    }

    /// Pick up the item and add it to the inventory.
    /// <param name="item">The RaycastHit containing information about the item.</param>
    public void PickUp(RaycastHit item)
    {
        ItemController itemController = item.collider.gameObject.GetComponent<ItemController>();
        if (itemController != null)
        {
            Item itemData = itemController.Item;
            // Add the Item instance to the inventory
            InventoryManager.Instance.AddItem(itemData);
            itemController.IsPickedUp = true;
            Debug.Log("ItemActions.PickUp(" + itemData.name + ")");
        }
        else Debug.LogError("ItemController component not found on " + item.collider.gameObject.name);
    }

    /// Use the specified item.
    /// <param name="item">The item to be used.</param>
    public void Use(Item item)
    {
        switch (item.Type)
        {
            case ItemType.Potion:
                UsePotion(item);
                break;
            case ItemType.Weapon:
                InventoryManager.Instance.Weapons.GetComponent<WeaponSwitcher>().SelectWeapon(item);
                break;
            default:
                InventoryManager.Instance.DisplayMessage("You cannot use this right now.");
                AudioManager.Instance.PlayClipOneShot(AudioManager.Instance.PlayerSpeaker2, "error", 0.6f, 1f);
                break;
        }
    }

    /// Use the potion.
    /// <param name="item">The potion to be used.</param>
    private void UsePotion(Item item)
    {
        switch (item.PotionType)
        {
            case Potion.PotionType.Antibiotics:
                _potion.Antibiotics(item);
                break;
            case Potion.PotionType.Bandage:
                _potion.Bandage(item);
                break;
            case Potion.PotionType.Painkillers:
                _potion.Painkillers(item);
                break;
            default:
                Debug.LogError("Unhandled potion type: " + item.PotionType);
                break;
        }
    }

    /// Inspect the specified item.
    /// <param name="item">The item to be inspected.</param>
    public void Inspect(Item item)
    {
        InventoryManager.Instance.Inspect(item);
        AudioManager.Instance.PlayClipOneShot(AudioManager.Instance.PlayerSpeaker2, "interact", 0.6f, 1f);
    }

    /// Throw away the specified item.
    /// <param name="item">The item to be thrown away.</param>
    public void ThrowAway(Item item)
    {
        if (item.Type == ItemType.Object || item.Type == ItemType.Weapon)
        {
            InventoryManager.Instance.DisplayMessage("You cannot throw this away.");
            AudioManager.Instance.PlayClipOneShot(AudioManager.Instance.PlayerSpeaker2, "error", 0.6f, 1f);
            return;
        }

        Debug.Log("ItemActions.ThrowAway(" + item.name + ")");
        InventoryManager.Instance.DropItem(item);
        AudioManager.Instance.PlayClipOneShot(AudioManager.Instance.PlayerSpeaker2, "interact", 0.6f, 1f);
    }

    #endregion
}