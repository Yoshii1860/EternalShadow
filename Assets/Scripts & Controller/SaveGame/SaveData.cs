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
public class InteractableObjectData
{
    public int uniqueID;
    public bool isPickedUp;
}

[System.Serializable]
public class EnemyData
{
    public int uniqueID;
    public int health;
    public bool isDead;
    public float[] position;
    public float[] rotation;
    public string lastBehaviorState;
}

[System.Serializable]
public class SaveData
{
    public int health;
    public float[] position;
    public List<ItemData> items;
    public List<WeaponData> weapons;
    public List<InteractableObjectData> interactableObjects;
    public List<EnemyData> enemies;

    public SaveData (Player player, Transform weaponPool, Transform objectPool, Transform enemyPool)
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

        ////////////////////////////
        // Save Interactable Objects
        ////////////////////////////

        interactableObjects = new List<InteractableObjectData>();
        foreach (ItemController interactableObject in objectPool.GetComponentsInChildren<ItemController>())
        {
            InteractableObjectData objectData = new InteractableObjectData();
            objectData.uniqueID = interactableObject.uniqueID;
            objectData.isPickedUp = interactableObject.isPickedUp;

            interactableObjects.Add(objectData);
        }

        ////////////////////////////
        // Save Enemies
        ////////////////////////////

        enemies = new List<EnemyData>();
        foreach (Enemy enemy in enemyPool.GetComponentsInChildren<Enemy>())
        {
            EnemyData enemyData = new EnemyData();
            enemyData.uniqueID = enemy.uniqueID;
            enemyData.health = enemy.health;
            enemyData.isDead = enemy.isDead;
            Debug.Log("SAVE Enemy is Dead? " + enemy.isDead);
            enemyData.position = new float[3];
            enemyData.position[0] = enemy.transform.position.x;
            enemyData.position[1] = enemy.transform.position.y;
            enemyData.position[2] = enemy.transform.position.z;
            enemyData.rotation = new float[3];
            enemyData.rotation[0] = enemy.transform.rotation.x;
            enemyData.rotation[1] = enemy.transform.rotation.y;
            enemyData.rotation[2] = enemy.transform.rotation.z;

            enemies.Add(enemyData);
        }
    }
}