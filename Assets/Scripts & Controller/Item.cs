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
    public string DisplayName;
    [Tooltip("The description of the item")]
    public string Description;
    [Tooltip("The icon representing the item")]
    public Sprite Icon;
    [Tooltip("Path to the icon resource")]
    public string IconPath;
    [Tooltip("Determines if the item is unique")]
    public bool IsUnique;
    [Tooltip("The quantity of the item")]
    public int Quantity;
    [Tooltip("The prefab associated with the item")]
    public GameObject Prefab;
    [Tooltip("Path to the prefab resource")]
    public string PrefabPath;
    [Tooltip("The type of the item")]
    public ItemType Type;

    #endregion




    #region Additional Fields

    [Space(10)]
    [Header("Additional Fields")]
    [Tooltip("Ammo type of the item (visible when ItemType is Ammo)")]
    [SerializeField] private Ammo.AmmoType ammoType;
    [Tooltip("Potion type of the item (visible when ItemType is Potion)")]
    [SerializeField] private Potion.PotionType potionType;

    #endregion




    #region Properties

    public Ammo.AmmoType AmmoType 
    {
        get => Type == ItemType.Ammo ? ammoType : Ammo.AmmoType.None;
        set 
        {
            if (Type == ItemType.Ammo) ammoType = value;

        }
    }

    public Potion.PotionType PotionType
    {
        get => Type == ItemType.Potion ? potionType : Potion.PotionType.None;
        set
        {
            if (Type == ItemType.Potion) potionType = value;
        }
    }

    #endregion




    #region Unity Editor Callbacks

    // This method will be called when the Item asset is created or modified in the Unity Editor.
    private void OnValidate()
    {
        // Update the iconPath and prefabPath whenever the icon or prefab is set in the Inspector.
        if (Icon != null) IconPath = GetResourcePath(Icon);

        if (Prefab != null) PrefabPath = GetResourcePath(Prefab);
    }

    #endregion




    #region Helper Methods

    // Helper method to get the resource path of an object.
    private string GetResourcePath(Object obj)
    {
        string assetPath = UnityEditor.AssetDatabase.GetAssetPath(obj);

        // Check if the asset path is in the Resources folder.
        if (assetPath.StartsWith("Assets/Resources/"))
        {
            return assetPath.Substring("Assets/Resources/".Length, assetPath.LastIndexOf(".") - "Assets/Resources/".Length);
        }
        else
        {
            Debug.LogWarning("Object must be in the Resources folder to get the resource path.");
            return null;
        }
    }

    #endregion
}