using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    Weapon,
    Ammo,
    Potion,
    Object
}

[CreateAssetMenu(fileName = "New Item", menuName = "Item/Create New Item")]
public class Item : ScriptableObject 
{
    public string displayName;
    public string description;
    public Sprite icon;
    public string iconPath;
    public bool unique;
    public int quantity;
    public GameObject prefab;
    public string prefabPath;
    public ItemType type;

    // Additional field for AmmoType, only visible when ItemType is Ammo
    [SerializeField]
    private Ammo.AmmoType ammoType;

    public Ammo.AmmoType AmmoType
    {
        get
        {
            if (type == ItemType.Ammo)
                return ammoType;
            else
                return Ammo.AmmoType.None; // Return a default value for non-Bullet items
        }
        set
        {
            ammoType = value;
        }
    }

    // Additional field for PotionType, only visible when ItemType is Potion
    [SerializeField]
    private Potion.PotionType potionType;

    public Potion.PotionType PotionType
    {
        get
        {
            if (type == ItemType.Potion)
                return potionType;
            else
                return Potion.PotionType.None; // Return a default value for non-Potion items
        }
        set
        {
            potionType = value;
        }
    }

    // This method will be called when the Item asset is created or modified in the Unity Editor.
    private void OnValidate()
    {
        // Update the iconPath and prefabPath whenever the icon or prefab is set in the Inspector.
        if (icon != null) iconPath = GetResourcePath(icon);

        if (prefab != null) prefabPath = GetResourcePath(prefab);
    }

    // Helper method to get the resource path of an object.
    private string GetResourcePath(Object obj)
    {
        string assetPath = UnityEditor.AssetDatabase.GetAssetPath(obj);
        string resourcePath = "Assets/Resources/";

        // Check if the asset path is in the Resources folder.
        if (assetPath.StartsWith(resourcePath))
        {
            int startIndex = resourcePath.Length;
            int endIndex = assetPath.LastIndexOf(".");
            if (endIndex == -1)
                endIndex = assetPath.Length;
            int length = endIndex - startIndex;
            return assetPath.Substring(startIndex, length);
        }
        else
        {
            Debug.LogWarning("Object must be in the Resources folder to get the resource path.");
            return null;
        }
    }
}

