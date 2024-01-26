using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region Enum and CreateAssetMenu

// Enum defining the types of items
public enum ItemType
{
    Weapon,
    Ammo,
    Potion,
    Object
}

// CreateAssetMenu attribute to create new items from Unity Editor
[CreateAssetMenu(fileName = "New Item", menuName = "Item/Create New Item")]
#endregion

public class Item : ScriptableObject
{
    #region Serialized Fields

    [Header("General Properties")]
    [Tooltip("The display name of the item")]
    public string displayName;

    [Tooltip("The description of the item")]
    public string description;

    [Tooltip("The icon representing the item")]
    public Sprite icon;

    [Tooltip("Path to the icon resource")]
    public string iconPath;

    [Tooltip("Determines if the item is unique")]
    public bool unique;

    [Tooltip("The quantity of the item")]
    public int quantity;

    [Tooltip("The prefab associated with the item")]
    public GameObject prefab;

    [Tooltip("Path to the prefab resource")]
    public string prefabPath;

    [Tooltip("The type of the item")]
    public ItemType type;

    #endregion

    #region Additional Fields

    [Header("Additional Fields")]
    [Tooltip("Ammo type of the item (visible when ItemType is Ammo)")]
    [SerializeField] private Ammo.AmmoType ammoType;

    [Tooltip("Potion type of the item (visible when ItemType is Potion)")]
    [SerializeField] private Potion.PotionType potionType;

    #endregion

    #region Properties

    public Ammo.AmmoType AmmoType
    {
        get
        {
            return type == ItemType.Ammo ? ammoType : Ammo.AmmoType.None;
        }
        set
        {
            ammoType = value;
        }
    }

    public Potion.PotionType PotionType
    {
        get
        {
            return type == ItemType.Potion ? potionType : Potion.PotionType.None;
        }
        set
        {
            potionType = value;
        }
    }

    #endregion

    #region Unity Editor Callbacks

    // This method will be called when the Item asset is created or modified in the Unity Editor.
    private void OnValidate()
    {
        // Update the iconPath and prefabPath whenever the icon or prefab is set in the Inspector.
        if (icon != null) iconPath = GetResourcePath(icon);

        if (prefab != null) prefabPath = GetResourcePath(prefab);
    }

    #endregion

    #region Helper Methods

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

    #endregion
}