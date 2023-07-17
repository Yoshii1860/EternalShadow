using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    public List<Item> Items = new List<Item>();

    [Tooltip("The transform that will be the parent of the inventory items")]
    public Transform itemContent;
    [Tooltip("The prefab that will be used to display the inventory items")]
    public GameObject inventoryItem;
    [Tooltip("The transform that will be the parent of the item preview")]
    public Transform itemPreview;
    [Tooltip("The prefab that will be used to display the inventory items")]
    public GameObject itemDisplay;

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

    public void AddItem(Item item)
    {
        if (Items.Contains(item))
        {
            // Increase quantity of existing item by quantity of added item;
            Items[Items.IndexOf(item)].quantity += item.quantity;
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
            Items.Remove(item);
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
    }

    public void ShowItemDisplay(Item item)
    {
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
                itemCanvasIcon.sprite = item.icon;
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
                itemCanvasText.text = item.displayName;
            }
            else if (itemCanvasText.gameObject.name == "ItemDescription")
            {
                itemCanvasText.text = item.description;
            }
            // If item is unique we hide the quantity display
            else if (itemCanvasText.gameObject.name == "ItemCountText" && item.unique)
            {
                itemCanvasText.gameObject.SetActive(false);
            }
            // If item is not unique, get its child object and change the quantity
            else if (itemCanvasText.gameObject.name == "ItemCountText" && !item.unique)
            {
                itemCanvasText.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = item.quantity.ToString();
            }
        }
        
    }
}