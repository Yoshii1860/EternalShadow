using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Reflection;
using System;

#region Save Data Classes

[System.Serializable]
public class ItemData
{
    public string DisplayName;
    public string Description;
    public bool IsUnique;
    public int Quantity;
    public string Type;
    public string AmmoType;
    public string PotionType;
    public string IconPath;
    public string PrefabPath;
}

[System.Serializable]
public class WeaponData
{
    public int Index;
    public int MagazineCount;
    public bool IsAvailable;
    public bool IsEquipped;
}

[System.Serializable]
public class InteractableObjectData
{
    public string UniqueID;
    public bool IsActive;
}

[System.Serializable]
public class EnemyData
{
    public string UniqueID;
    public int Health;
    public bool IsDead;
    public bool IsActive;
    public float[] Position;
    public float[] Rotation;
}

[System.Serializable]
public class InteractStaticObjectData
{
    public string UniqueID;
    public bool IsActive;
    public bool IsOpen;
    public float[] Position;
    public float[] Rotation;
}

[System.Serializable]
public class DoorObjectData
{
    public string UniqueID;
    public bool IsOpen;
    public bool IsLocked;
}

[System.Serializable]
public class TextObjectData
{
    public string UniqueID;
    public bool IsActive;
}

[System.Serializable]
public class SavedEventData
{
    public string EventName;
    public Type ScriptType;
    public bool Active;
}

[System.Serializable]
public class SummonObjectData
{
    public string UniqueID;
    public bool IsObjectPlaced;
}

[System.Serializable]
public class VariousData
{
    public string EnvironmentMusicClip;
    public bool IsFlashlightEnabled;
}

[System.Serializable]
public class AutoSaveData
{
    public bool IsActive;
    public string UniqueID;
}

#endregion

[System.Serializable]
public class SaveData
{
    #region Variables

    public string SceneName;
    public float Health;
    public float[] Position;
    public List<ItemData> Items;
    public List<WeaponData> Weapons;
    public List<EnemyData> Enemies;
    public List<InteractableObjectData> InteractableObjects;
    public List<InteractStaticObjectData> InteractStaticObjects;
    public List<DoorObjectData> Doors;
    public List<SavedEventData> Events;
    public List<TextObjectData> TextObjects;
    public List<SummonObjectData> SummonObjects;
    public VariousData VariousData;
    public List<AutoSaveData> AutoSaveData;

    #endregion

    public SaveData (
        Player player, 
        Transform weaponPool, 
        Transform enemyPool, 
        Transform interactableObjectPool, 
        Transform interactStaticObjectPool, 
        Transform doorObjectPool, 
        Transform textObjectPool, 
        Transform summonObjectPool, 
        Transform autoSavePool)
    {
        #region Save Scene

        SceneName = SceneManager.GetActiveScene().name;

        #endregion

        #region Save Player Data
        
        Health = player.Health;

        Position = new float[3];
        Position[0] = player.transform.position.x;
        Position[1] = player.transform.position.y;
        Position[2] = player.transform.position.z;

        #endregion

        #region Save Inventory Items

        Items = new List<ItemData>();
        foreach (Item item in InventoryManager.Instance.Items)
        {
            ItemData itemData = new ItemData();
            itemData.DisplayName = item.DisplayName;
            itemData.Description = item.Description;
            itemData.IsUnique = item.IsUnique;
            itemData.Quantity = item.Quantity;
            itemData.Type = item.Type.ToString();
            itemData.AmmoType = item.AmmoType.ToString();
            itemData.PotionType = item.PotionType.ToString();
            itemData.IconPath = item.IconPath;
            itemData.PrefabPath = item.PrefabPath;

            Items.Add(itemData);
        }

        #endregion

        #region Save Weapons

        Weapons = new List<WeaponData>();
        for (int i = 0; i < weaponPool.childCount; i++)
        {
            WeaponData weaponData = new WeaponData();
            weaponData.Index = i;
            weaponData.MagazineCount = weaponPool.GetChild(i).GetComponent<Weapon>().CurrentAmmoInClip;
            weaponData.IsAvailable = weaponPool.GetChild(i).GetComponent<Weapon>().IsAvailable;
            weaponData.IsEquipped = weaponPool.GetChild(i).GetComponent<Weapon>().IsEquipped;

            Weapons.Add(weaponData);
        }

        #endregion

        #region Save Enemies

        Enemies = new List<EnemyData>();
        foreach (Enemy enemy in enemyPool.GetComponentsInChildren<Enemy>(true))
        {
            EnemyData enemyData = new EnemyData();
            enemyData.UniqueID = enemy.GetComponent<UniqueIDComponent>().UniqueID;
            enemyData.Health = enemy.HealthPoints;
            enemyData.IsDead = enemy.IsDead;
            enemyData.IsActive = enemy.gameObject.activeSelf;
            enemyData.Position = new float[3];
            enemyData.Position[0] = enemy.transform.position.x;
            enemyData.Position[1] = enemy.transform.position.y;
            enemyData.Position[2] = enemy.transform.position.z;
            enemyData.Rotation = new float[3];
            enemyData.Rotation[0] = enemy.transform.rotation.x;
            enemyData.Rotation[1] = enemy.transform.rotation.y;
            enemyData.Rotation[2] = enemy.transform.rotation.z;

            Enemies.Add(enemyData);
        }

        foreach (Mannequin mannequin in enemyPool.GetComponentsInChildren<Mannequin>(true))
        {
            EnemyData enemyData = new EnemyData();
            enemyData.UniqueID = mannequin.GetComponent<UniqueIDComponent>().UniqueID;
            enemyData.Health = 0;
            enemyData.IsDead = mannequin.IsDead;
            enemyData.IsActive = mannequin.gameObject.activeSelf;
            enemyData.Position = new float[3];
            enemyData.Position[0] = mannequin.transform.position.x;
            enemyData.Position[1] = mannequin.transform.position.y;
            enemyData.Position[2] = mannequin.transform.position.z;
            enemyData.Rotation = new float[3];
            enemyData.Rotation[0] = mannequin.transform.rotation.x;
            enemyData.Rotation[1] = mannequin.transform.rotation.y;
            enemyData.Rotation[2] = mannequin.transform.rotation.z;

            Enemies.Add(enemyData);
        }

        #endregion

        #region Save Interactable Objects

        InteractableObjects = new List<InteractableObjectData>();
        foreach (InteractableObject pickupObject in interactableObjectPool.GetComponentsInChildren<InteractableObject>(true))
        {
            InteractableObjectData objectData = new InteractableObjectData();
            objectData.UniqueID = pickupObject.GetComponent<UniqueIDComponent>().UniqueID;
            objectData.IsActive = pickupObject.IsActive;

            InteractableObjects.Add(objectData);
        }
        foreach (Duplicate duplicate in interactableObjectPool.GetComponentsInChildren<Duplicate>())
        {
            InteractableObjectData objectData = new InteractableObjectData();
            objectData.UniqueID = duplicate.DuplicateID;
            objectData.IsActive = duplicate.DuplicateObject.GetComponent<InteractableObject>().IsActive;

            InteractableObjects.Add(objectData);
        }

        #endregion

        #region Save Interactable Static Objects

        InteractStaticObjects = new List<InteractStaticObjectData>();
        foreach (Transform drawer in interactStaticObjectPool)
        {
            foreach (Drawer drawerScript in drawer.GetComponentsInChildren<Drawer>())
            {
                InteractStaticObjectData drawerData = new InteractStaticObjectData();
                drawerData.UniqueID = drawerScript.GetComponent<UniqueIDComponent>().UniqueID;
                drawerData.IsOpen = drawerScript.IsOpen;
                drawerData.Position = new float[3];
                drawerData.Position[0] = drawerScript.transform.position.x;
                drawerData.Position[1] = drawerScript.transform.position.y;
                drawerData.Position[2] = drawerScript.transform.position.z;
                drawerData.Rotation = new float[3];
                drawerData.Rotation[0] = drawerScript.transform.rotation.x;
                drawerData.Rotation[1] = drawerScript.transform.rotation.y;
                drawerData.Rotation[2] = drawerScript.transform.rotation.z;

                InteractStaticObjects.Add(drawerData);
            }
        }

        #endregion

        #region Save Doors

        Doors = new List<DoorObjectData>();
        foreach (Duplicate door in doorObjectPool.GetComponentsInChildren<Duplicate>())
        {
            Door doorScript = door.DuplicateObject.GetComponent<Door>();
            if (doorScript == null) 
            {
                Debug.LogError("SaveData: Door script is null");
                continue;
            }
            DoorObjectData doorData = new DoorObjectData();
            doorData.UniqueID = doorScript.GetComponent<UniqueIDComponent>().UniqueID;
            doorData.IsOpen = doorScript.IsOpen;
            doorData.IsLocked = doorScript.IsLocked;

            Doors.Add(doorData);
        }

        #endregion

        #region Save Text Objects

        TextObjects = new List<TextObjectData>();
        foreach (TextCode textCode in textObjectPool.GetComponentsInChildren<TextCode>())
        {
            TextObjectData textData = new TextObjectData();
            textData.UniqueID = textCode.GetComponent<UniqueIDComponent>().UniqueID;
            textData.IsActive = textCode.IsAudioActive;

            TextObjects.Add(textData);
        }

        foreach (Duplicate duplicate in textObjectPool.GetComponentsInChildren<Duplicate>())
        {
            TextObjectData textData = new TextObjectData();
            TextCode textCode = duplicate.DuplicateObject.GetComponent<TextCode>();
            textData.UniqueID = duplicate.DuplicateID;
            textData.IsActive = textCode.IsAudioActive;

            TextObjects.Add(textData);
        }

        #endregion

        #region Save Events

        Events = new List<SavedEventData>();

        foreach (EventDataEntry eventDataEntry in GameManager.Instance.EventData.eventDataEntries)
        {
            SavedEventData savedEventData = new SavedEventData();
            savedEventData.EventName = eventDataEntry.EventName;
            savedEventData.ScriptType = eventDataEntry.ScriptType;
            savedEventData.Active = eventDataEntry.Active;

            Events.Add(savedEventData);
        }

        #endregion

        #region Save Summon Objects

        SummonObjects = new List<SummonObjectData>();
        foreach (SummonObject summonObject in summonObjectPool.GetComponentsInChildren<SummonObject>())
        {
            SummonObjectData summonObjectData = new SummonObjectData();
            summonObjectData.UniqueID = summonObject.GetComponent<UniqueIDComponent>().UniqueID;
            summonObjectData.IsObjectPlaced = summonObject.IsObjectPlaced;

            SummonObjects.Add(summonObjectData);
        }

        #endregion

        #region Save Various Data

        VariousData = new VariousData();
        AudioSource environmentAudio = AudioManager.Instance.GetAudioSource(AudioManager.Instance.Environment);
        VariousData.EnvironmentMusicClip = environmentAudio.clip.name;
        VariousData.IsFlashlightEnabled = GameManager.Instance.Player.IsLightAvailable;

        #endregion

        #region Save AutoSave Data

        AutoSaveData = new List<AutoSaveData>();

        foreach (AutoSave autoSave in autoSavePool.GetComponentsInChildren<AutoSave>())
        {
            AutoSaveData autoSaveEntry = new AutoSaveData();
            autoSaveEntry.IsActive = autoSave.IsActive;
            autoSaveEntry.UniqueID = autoSave.GetComponent<UniqueIDComponent>().UniqueID;

            AutoSaveData.Add(autoSaveEntry);
        }

        #endregion
    }
}
