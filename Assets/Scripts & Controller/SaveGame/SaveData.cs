using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

#region Save Data Classes

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
    public string uniqueID;
    public bool active;
}

[System.Serializable]
public class EnemyData
{
    public string uniqueID;
    public int health;
    public bool isDead;
    public bool isActive;
    public float[] position;
    public float[] rotation;
    public string lastBehaviorState;
}

[System.Serializable]
public class InteractStaticObjectData
{
    public string uniqueID;
    public bool active;
    public bool open;
    public float[] position;
    public float[] rotation;
}

[System.Serializable]
public class DoorObjectData
{
    public string uniqueID;
    public bool open;
    public bool locked;
    public float[] rotation;
}

[System.Serializable]
public class SavedEventData
{
    public string eventName;
    public bool eventState;
}

#endregion

[System.Serializable]
public class SaveData
{
    #region Variables

    public string sceneName;
    public float health;
    public float[] position;
    public List<ItemData> items;
    public List<WeaponData> weapons;
    public List<EnemyData> enemies;
    public List<InteractableObjectData> interactableObjects;
    public List<InteractStaticObjectData> interactStaticObjects;
    public List<DoorObjectData> doors;
    public List<SavedEventData> events;

    #endregion

    public SaveData (Player player, Transform weaponPool, Transform enemyPool, Transform interactableObjectPool, Transform interactStaticObjectPool, Transform doorObjectPool, EventData eventData)
    {
        #region Save Scene

        sceneName = SceneManager.GetActiveScene().name;

        #endregion

        #region Save Player Data
        
        health = player.health;

        position = new float[3];
        position[0] = player.transform.position.x;
        position[1] = player.transform.position.y;
        position[2] = player.transform.position.z;

        #endregion

        #region Save Inventory Items

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

        #endregion

        #region Save Weapons

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

        #endregion

        #region Save Enemies

        enemies = new List<EnemyData>();
        foreach (Enemy enemy in enemyPool.GetComponentsInChildren<Enemy>())
        {
            EnemyData enemyData = new EnemyData();
            enemyData.uniqueID = enemy.GetComponent<UniqueIDComponent>().UniqueID;
            enemyData.health = enemy.health;
            enemyData.isDead = enemy.isDead;
            enemyData.isActive = enemy.gameObject.activeSelf;
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

        #endregion

        #region Save Interactable Objects

        interactableObjects = new List<InteractableObjectData>();
        foreach (InteractableObject pickupObject in interactableObjectPool.GetComponentsInChildren<InteractableObject>())
        {
            InteractableObjectData objectData = new InteractableObjectData();
            objectData.uniqueID = pickupObject.GetComponent<UniqueIDComponent>().UniqueID;
            objectData.active = pickupObject.active;

            interactableObjects.Add(objectData);
        }
        foreach (Duplicate duplicate in interactableObjectPool.GetComponentsInChildren<Duplicate>())
        {
            InteractableObjectData objectData = new InteractableObjectData();
            objectData.uniqueID = duplicate.duplicateID;
            objectData.active = duplicate.duplicateObject.GetComponent<InteractableObject>().active;

            interactableObjects.Add(objectData);
        }

        #endregion

        #region Save Interactable Static Objects

        interactStaticObjects = new List<InteractStaticObjectData>();
        foreach (Transform drawer in interactStaticObjectPool)
        {
            foreach (Drawer drawerScript in drawer.GetComponentsInChildren<Drawer>())
            {
                InteractStaticObjectData drawerData = new InteractStaticObjectData();
                drawerData.uniqueID = drawerScript.GetComponent<UniqueIDComponent>().UniqueID;
                drawerData.open = drawerScript.open;
                drawerData.position = new float[3];
                drawerData.position[0] = drawerScript.transform.position.x;
                drawerData.position[1] = drawerScript.transform.position.y;
                drawerData.position[2] = drawerScript.transform.position.z;
                drawerData.rotation = new float[3];
                drawerData.rotation[0] = drawerScript.transform.rotation.x;
                drawerData.rotation[1] = drawerScript.transform.rotation.y;
                drawerData.rotation[2] = drawerScript.transform.rotation.z;

                interactStaticObjects.Add(drawerData);
            }
        }

        #endregion

        #region Save Doors

        doors = new List<DoorObjectData>();
        foreach (Duplicate door in doorObjectPool.GetComponentsInChildren<Duplicate>())
        {
            Door doorScript = door.duplicateObject.GetComponent<Door>();
            if (doorScript == null) 
            {
                Debug.LogError("SaveData: Door script is null");
                continue;
            }
            DoorObjectData doorData = new DoorObjectData();
            doorData.uniqueID = doorScript.GetComponent<UniqueIDComponent>().UniqueID;
            doorData.open = doorScript.open;
            doorData.locked = doorScript.locked;
            doorData.rotation = new float[3];
            doorData.rotation[0] = doorScript.transform.rotation.x;
            doorData.rotation[1] = doorScript.transform.rotation.y;
            doorData.rotation[2] = doorScript.transform.rotation.z;

            doors.Add(doorData);
        }

        #endregion

        #region Save Events

        events = new List<SavedEventData>();

        foreach (string name in eventData.eventNames)
        {
            SavedEventData savedEventData = new SavedEventData();
            savedEventData.eventName = name;
            savedEventData.eventState = eventData.CheckEvent(name);

            events.Add(savedEventData);
        }

        #endregion
    }
}
