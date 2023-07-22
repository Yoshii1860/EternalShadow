using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemData
{
    public string displayName;
    public string description;
    public bool unique;
    public int quantity;
    public string type;
    public string ammoType;
    public string potionType;
    public string iconPath;
    public string prefabPath;
}

[System.Serializable]
public class SaveData
{
    public int health;
    public float[] position;
    public List<ItemData> items;

    public SaveData (Player player)
    {
        ////////////////////////////
        // Save Player Stats
        ////////////////////////////
        
        health = player.health;

        position = new float[3];
        position[0] = player.transform.position.x;
        position[1] = player.transform.position.y;
        position[2] = player.transform.position.z;

        ////////////////////////////
        // Save Inventory
        ////////////////////////////

        items = new List<ItemData>();
        foreach (Item item in InventoryManager.Instance.Items)
        {
            ItemData itemData = new ItemData();
            itemData.displayName = item.displayName;
            itemData.description = item.description;
            itemData.unique = item.unique;
            itemData.quantity = item.quantity;
            itemData.type = item.type.ToString();
            itemData.ammoType = item.AmmoType.ToString();
            itemData.potionType = item.PotionType.ToString();
            itemData.iconPath = item.iconPath;
            itemData.prefabPath = item.prefabPath;

            items.Add(itemData);
        }
    }
}