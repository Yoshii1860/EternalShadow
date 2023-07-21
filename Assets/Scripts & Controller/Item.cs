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
    public bool unique;
    public int quantity;
    public GameObject prefab;
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
}

