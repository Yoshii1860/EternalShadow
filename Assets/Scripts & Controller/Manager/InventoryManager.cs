using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class InventoryManager : MonoBehaviour
{
    #region Singleton

    public static InventoryManager Instance;

    // Create a static reference to the instance
    private void Awake()
    {
        InitializeSingleton();
    }

    void InitializeSingleton()
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

    #endregion

    #region Fields

        public List<Item> Items = new List<Item>();

        [Space(10)]
        [Header("Inventory Settings")]
        [Tooltip("The prefab that will be used to display the inventory items")]
        public GameObject inventoryItem;
        [Tooltip("The prefab that will be used to display the inventory item display")]
        public GameObject itemDisplay;
        [Tooltip("The force that will be applied to the dropped item")]
        public float dropForce;

        // Additional fields
        Transform[] gridLayouts;
        public Transform itemContent;
        public Transform itemPreview;
        public GameObject weapons;
        GameObject player;
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

        #endregion

    #region Initialization

    void Start()
    {
        InitializeReferences();
    }

    void InitializeReferences()
    {
        gridLayouts = FindGridLayoutGroups();
        itemContent = gridLayouts[1];
        itemPreview = gridLayouts[2];
    }

    private Transform[] FindGridLayoutGroups(Transform parent = null)
    {
        if (parent == null)
        {
            parent = transform;
        }

        // Check if the current GameObject has a GridLayoutGroup component
        GridLayoutGroup gridLayoutGroup = parent.GetComponent<GridLayoutGroup>();
        if (gridLayoutGroup != null)
        {
            Transform viewport = gridLayoutGroup.transform.parent;

            // The target GridLayoutGroup is found!
            Transform[] gridLayoutGroups = viewport.GetComponentsInChildren<Transform>(true);

            return gridLayoutGroups;
        }
        else
        {
            Transform child = parent.GetChild(0);
            Transform[] result = FindGridLayoutGroups(child);
            if (result != null && result.Length > 0)
            {
                return result;
            }
        }

        // If no GridLayoutGroup was found, return null
        return null;
    }

    #endregion

    #region Update References

    public void UpdateReferences()
    {
        player = GameManager.Instance.player.gameObject;
        weapons = FindObjectOfType<WeaponSwitcher>().gameObject;
        ammo = player.GetComponent<Ammo>();

        GetComponent<ItemActions>().UpdateReferences();
        GetComponent<ObjectHandler>().UpdateReferences();
        GetComponent<Potion>().UpdateReferences();
    }

    #endregion

    #region Item Management

    public void AddItem(Item item)
    {
        if (item.type == ItemType.Ammo || item.type == ItemType.Potion)
        {
            Item existingItem = Items.Find(existingItem => existingItem.displayName == item.displayName);
            if (existingItem != null)
            {
                Debug.Log("InventoryManager - Add Existing Ammo/Potion (" + item.displayName + ")");
                // Increase quantity of existing item by quantity of added item;
                existingItem.quantity += item.quantity;

                if (item.type == ItemType.Ammo)
                {
                    // INCREASE AMMO SLOT
                    ammo.IncreaseCurrentAmmo(item.AmmoType, item.quantity);

                    // SET UI FOR BULLETS
                    foreach(Transform weapon in weapons.transform)
                    {
                        if (weapon.GetComponent<Weapon>().ammoType == existingItem.AmmoType &&
                            weapon.GetComponent<Weapon>().gameObject.activeSelf == true)
                        {
                            Weapon weaponScript = weapon.GetComponent<Weapon>();
                            player.GetComponent<Player>().SetBulletsUI(weaponScript.magazineCount, existingItem.quantity);
                            break;
                        }
                    }
                }

                return;
            }
            else
            {
                Debug.Log("InventoryManager - Add New Ammo/Potion (" + item.displayName + ")");

                Item newItem = Instantiate(item);
                Items.Add(newItem);

                // If the item is not unique, create a copy to not change the original
                if (item.type == ItemType.Ammo)
                {
                    // Increase the current ammo amount of the player
                    ammo.IncreaseCurrentAmmo(item.AmmoType, item.quantity);

                    // SET UI FOR BULLETS
                    foreach(Transform weapon in weapons.transform)
                    {
                        if (weapon.GetComponent<Weapon>().ammoType == item.AmmoType)
                        {
                            Weapon weaponScript = weapon.GetComponent<Weapon>();
                            int inventoryAmmoAmount = GetInventoryAmmo(weaponScript.ammoType);
                            player.GetComponent<Player>().SetBulletsUI(weaponScript.magazineCount, inventoryAmmoAmount);
                            break;
                        }
                    }

                }
                return;
            }
        }
        else if (item.type == ItemType.Weapon)
        {
            Debug.Log("InventoryManager - Add New Weapon (" + item.displayName + ")");

            Items.Add(item);
            foreach (Transform weapon in weapons.transform)
            {                    
                if (weapon.GetComponent<ItemController>() != null && weapon.GetComponent<ItemController>().item.displayName == item.displayName)
                {
                    Weapon weaponScript = weapon.GetComponent<Weapon>();
                    weaponScript.isAvailable = true;

                    // SET UI FOR BULLETS
                    int inventoryAmmo = GetInventoryAmmo(weaponScript.ammoType);
                    player.GetComponent<Player>().SetBulletsUI(weaponScript.magazineCount, inventoryAmmo);

                    ammo.IncreaseCurrentAmmo(weaponScript.ammoType, weaponScript.magazineCount);
                    while (weapon.gameObject.activeSelf == false)
                    {
                        weapons.GetComponent<WeaponSwitcher>().SwitchWeapon(true);
                    }
                    break;
                }
            }
        }
        else
        {
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

    public void RemoveAmmo(Ammo.AmmoType ammotype, int amount)
    {
        Debug.Log("InventoryManager.RemoveAmmo(" + ammotype + ", " + amount + ")");
        foreach (Item item in Items)
        {
            if (item.AmmoType == ammotype)
            {
                if (item.quantity > amount)
                {
                    item.quantity -= amount;
                    break;
                }
                else if (item.quantity == amount)
                {
                    Items.Remove(item);
                    break;
                }
                else
                {
                    Debug.Log("InventoryManager.RemoveAmmo() - FAILED");
                }
            }
        }
    }

    public Item FindItem(string itemDisplayName)
    {
        foreach (Item inventoryItem in Items)
        {
            if (inventoryItem.displayName == itemDisplayName)
            {
                return inventoryItem;
            }
        }
        return null;
    }

    public int GetInventoryAmmo(Ammo.AmmoType ammotype)
    {
        foreach (Item item in Items)
        {
            if (item.AmmoType == ammotype)
            {
                return item.quantity;
            }
        }
        return 0;
    }

    #endregion

    #region UI Handling

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

            if (item.unique && item.type != ItemType.Weapon)
            {
                // If the item is unique, hide the quantity
                itemCanvasText.enabled = false;
            }
            else if (item.type == ItemType.Weapon)
            {
                // If item is weapon, show current ammo in magazine
                // loop through weapons to find the weapon with the same name as the item
                foreach (Transform weapon in weapons.transform)
                {
                    if (weapon.GetComponent<ItemController>().item.displayName == item.displayName)
                    {   
                        Weapon weaponScript = weapon.GetComponent<Weapon>();
                        if (weaponScript.ammoType == Ammo.AmmoType.Infinite)
                        {
                            itemCanvasText.enabled = false;
                            break;
                        }
                        
                        // If it is, get the weapon stats and display them
                        string weaponAmmo = weaponScript.magazineCount.ToString();
                        itemCanvasText.text = weaponAmmo;
                        break;
                    }
                }
            }
            else
            {
                // If the item is not unique, show the quantity
                itemCanvasText.text = item.quantity.ToString();
            }

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

    #endregion

    #region Item Actions

    public void DropItem(Item item)
    {
        Debug.Log("InventoryManager.DropItem(" + item.displayName + ")");
        GameObject obj = Instantiate(item.prefab, player.transform.position + player.transform.forward, Quaternion.identity, GameManager.Instance.objectPool);
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

    #endregion
}