using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    public List<Item> Items = new List<Item>();

    [Space(10)]
    [Header("Inventory Settings")]
    [Tooltip("The transform that will be the parent of the inventory items")]
    public Transform itemContent;
    [Tooltip("The prefab that will be used to display the inventory items")]
    public GameObject inventoryItem;
    [Tooltip("The transform that will be the parent of the item preview")]
    public Transform itemPreview;
    [Tooltip("The prefab that will be used to display the inventory items")]
    public GameObject itemDisplay;
    [Tooltip("The transform that will be the parent of the weapons")]
    public GameObject weapons;
    [Tooltip("The player GameObject")]
    public GameObject player;
    [Tooltip("The object pool that will be used to drop items")]
    public Transform objectPool;
    [Tooltip("The force that will be applied to the dropped item")]
    public float dropForce;
    //
    Ammo ammo;

    [Space(10)]
    [Header("Colors")]
    [Tooltip("The color of the selected item")]
    public Color selectedColor;
    [Tooltip("The color of the unselected item")]
    public Color unselectedColor;
    [Tooltip("The color of the highlighted item")]
    public Color highlightedColor;

    [Space(10)]
    [Header("Only for Debugging)")]
    public Item selectedItem;
    public int highlightNumber;
    public int selectedActionNumber;
    public int itemActionNumber;
    public bool itemActionsOpen;
    public int actionsChildCount;

    // The Image components of the item actions
    Image[] itemCanvasActions;

    private void Awake()
    {
        // Singleton pattern to ensure only one instance exists
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Keep the InventoryManager between scene changes
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        ammo = player.GetComponent<Ammo>();
    }

    public void AddItem(Item item)
    {
        if (Items.Contains(item))
        {
            if (item.type == ItemType.Ammo)
            {
                // Increase the current ammo amount of the player
                ammo.IncreaseCurrentAmmo(item.AmmoType, item.quantity);
            }
            // Increase quantity of existing item by quantity of added item;
            Items[Items.IndexOf(item)].quantity += item.quantity;
        }
        else
        {
            if (item.type == ItemType.Ammo)
            {
                // Increase the current ammo amount of the player
                ammo.IncreaseCurrentAmmo(item.AmmoType, item.quantity);
            }
            else if (item.type == ItemType.Weapon)
            {
                foreach (Transform weapon in weapons.transform)
                {                    
                    if (weapon.GetComponent<ItemController>().item.displayName == item.displayName)
                    {
                        weapon.GetComponent<Weapon>().isAvailable = true;
                        while (weapon.gameObject.activeSelf == false)
                        {
                            weapons.GetComponent<WeaponSwitcher>().SwitchWeapon(true);
                        }
                    }
                }
            }
            Items.Add(item);
        }
    }

    public void RemoveItem(Item item)
    {
        if (Items.Contains(item))
        {
            if (Items[Items.IndexOf(item)].quantity > 1)
            {
                Items[Items.IndexOf(item)].quantity -= 1;
                BackToSelection();
                ListItems();
            }
            else
            {
                Items.Remove(item);
                BackToSelection();
                ListItems();
            }
        }
    }

    public void ListItems()
    {
        // Clear list before showing items
        foreach (Transform child in itemContent)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in itemPreview)
        {
            Destroy(child.gameObject);
        }

        // Show items from inventory list in UI
        foreach (Item item in Items)
        {
            GameObject obj = Instantiate(inventoryItem, itemContent);
            Image[] itemCanvasIcons = obj.GetComponentsInChildren<Image>();
            TextMeshProUGUI itemCanvasText = obj.GetComponentInChildren<TextMeshProUGUI>();

            if (item.unique)
            {
                // If the item is unique, hide the quantity
                itemCanvasText.enabled = false;
            }
            else
            {
                // If the item is not unique, show the quantity
                itemCanvasText.text = item.quantity.ToString();
            }
            itemCanvasText.text = item.quantity.ToString();
            // Iterate through the Image components and find the desired one
            foreach (Image itemCanvasIcon in itemCanvasIcons)
            {
                if (itemCanvasIcon.gameObject != obj)
                {
                    itemCanvasIcon.sprite = item.icon;
                    break;
                }
            }
            obj.GetComponentInChildren<ItemController>().item = item;
        }

        // check if Items list is empty
        if (Items.Count > 0)
        {
            highlightNumber = 0;
            selectedItem = Items[highlightNumber];
            ChangeSelectedItemColor(true, true);
        }
    }

    public void ShowItemDisplay()
    {
        selectedItem = Items[highlightNumber];
        ChangeSelectedItemColor(false, false);

        itemActionNumber = 0;
        itemActionsOpen = true;

        GameObject obj;
        // Instantiate ItemDisplay UI if it doesn't exist
        if (itemPreview.childCount == 0)
        {
            obj = Instantiate(itemDisplay, itemPreview);
        }
        else
        {
            // Clear the ItemDisplay UI
            foreach (Transform child in itemPreview)
            {
                Destroy(child.gameObject);
            }
            // Instantiate ItemDisplay UI
            obj = Instantiate(itemDisplay, itemPreview);
        }

        // Change icon of obj to item.icon
        Image[] itemCanvasIcons = obj.GetComponentsInChildren<Image>();
        // Iterate through the Image components and find the desired one
        foreach (Image itemCanvasIcon in itemCanvasIcons)
        {
            if (itemCanvasIcon.gameObject != obj)
            {
                itemCanvasIcon.sprite = selectedItem.icon;
                break;
            }
        }

        // Get all TextMeshProUGUI components
        TextMeshProUGUI[] itemCanvasTexts = obj.GetComponentsInChildren<TextMeshProUGUI>();
        foreach (TextMeshProUGUI itemCanvasText in itemCanvasTexts)
        {
            // Change name and description of ItemDisplay
            if (itemCanvasText.gameObject.name == "ItemName")
            {
                itemCanvasText.text = selectedItem.displayName;
            }
            else if (itemCanvasText.gameObject.name == "ItemDescription")
            {
                itemCanvasText.text = selectedItem.description;
            }
            // If item is unique we hide the quantity display
            else if (itemCanvasText.gameObject.name == "ItemCountText" && selectedItem.unique)
            {
                itemCanvasText.gameObject.SetActive(false);
            }
            // If item is not unique, get its child object and change the quantity
            else if (itemCanvasText.gameObject.name == "ItemCountText" && !selectedItem.unique)
            {
                itemCanvasText.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = selectedItem.quantity.ToString();
            }
            // If item is not a weapon, hide the stats display
            if (itemCanvasText.gameObject.name == "ItemStatsText" && selectedItem.type != ItemType.Weapon)
            {
                itemCanvasText.gameObject.SetActive(false);
            }
            else if (itemCanvasText.gameObject.name == "ItemStatsText" && selectedItem.type == ItemType.Weapon)
            {
                // loop through all the childs of weapons, check if item is the same item as the one displayed
                foreach (Transform weapon in weapons.transform)
                {
                    if (weapon.GetComponent<ItemController>().item.displayName == selectedItem.displayName)
                    {
                        // If it is, get the weapon stats and display them
                        string weaponStats = weapon.GetComponent<Weapon>().GetWeaponStats();
                        itemCanvasText.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = weaponStats;
                        break;
                    }
                }
            }
        }
        
        // Get all children objects of obj and find the one named ItemActions
        foreach (RectTransform child in obj.GetComponentsInChildren<RectTransform>())
        {
            if (child.gameObject.name == "ItemActions")
            {
                // Get all image children of child and add them to itemCanvasActions
                itemCanvasActions = child.GetComponentsInChildren<Image>();
                actionsChildCount = child.childCount;
                ChangeSelectedActionColor(true);
            }
        }
    }

    public void ChangeSelectedItemColor(bool highlight, bool newSelected)
    {
        selectedItem = Items[highlightNumber];

        ItemController itemController;
        Image itemImage;
        foreach (Transform item in itemContent)
        {
            itemController = item.GetComponentInChildren<ItemController>();
            if (itemController != null && itemController.item.displayName == selectedItem.displayName)
            {
                itemImage = item.GetComponentInChildren<Image>();
                if (itemImage != null)
                {
                    if (highlight) itemImage.color = newSelected ? highlightedColor : unselectedColor;
                    else itemImage.color = selectedColor;
                }
            }
        }
    }

    public void ChangeSelectedActionColor(bool newHighlight)
    {
        itemCanvasActions[itemActionNumber].color = newHighlight ? highlightedColor : unselectedColor;
    }

    public void BackToSelection()
    {
        itemActionsOpen = false;
        if (Items.Count > 0) ChangeSelectedItemColor(true, true);
        foreach (Transform child in itemPreview)
        {
            Destroy(child.gameObject);
        }
    }

    public void DropItem(Item item)
    {
        GameObject obj = Instantiate(item.prefab, player.transform.position + player.transform.forward, Quaternion.identity, objectPool);
        Rigidbody itemRigidbody = obj.AddComponent<Rigidbody>();
        itemRigidbody.AddForce(player.transform.forward * dropForce, ForceMode.Impulse);
        Destroy(itemRigidbody, 1.5f);

        if(item.type == ItemType.Weapon)
        {
            Weapon childWeapon = weapons.GetComponentInChildren<Weapon>();
            childWeapon.isAvailable = false;

            weapons.GetComponent<WeaponSwitcher>().SwitchWeapon(true);
        }
        else if(item.type == ItemType.Ammo)
        {
            for (int i = 0; i < item.quantity; i++)
            {
                ammo.ReduceCurrentAmmo(item.AmmoType);
            }
        }
        Items.Remove(item);
        ListItems();
        BackToSelection();
    }   
}