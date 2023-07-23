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
public class WeaponData
{
    public int index;
    public int magazineCount;
    public bool isAvailable;
    public bool isEquipped;
}

[System.Serializable]
public class SaveData
{
    public int health;
    public float[] position;
    public List<ItemData> items;
    public List<WeaponData> weapons;

    public SaveData (Player player, Transform weaponPool)
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
            Debug.Log("SAVE: " + item.iconPath);
            itemData.prefabPath = item.prefabPath;

            items.Add(itemData);
        }

        ////////////////////////////
        // Save Weapons
        ////////////////////////////

        weapons = new List<WeaponData>();
        for (int i = 0; i < weaponPool.childCount; i++)
        {
            WeaponData weaponData = new WeaponData();
            weaponData.index = i;
            weaponData.magazineCount = weaponPool.GetChild(i).GetComponent<Weapon>().magazineCount;
            weaponData.isAvailable = weaponPool.GetChild(i).GetComponent<Weapon>().isAvailable;
            weaponData.isEquipped = weaponPool.GetChild(i).GetComponent<Weapon>().isEquipped;

            weapons.Add(weaponData);
        }
    }
}