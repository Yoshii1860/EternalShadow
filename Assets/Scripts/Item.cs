using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    Weapon,
    Bullet,
    Potion,
    Object
}

[CreateAssetMenu(fileName = "New Item", menuName = "Item/Create New Item")]
public class Item : ScriptableObject 
{
    public string displayName;
    public string description;
    public Sprite icon;
    public int quantity;
    public ItemType type;
    public GameObject prefab;
}

