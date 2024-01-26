using UnityEngine;

/// <summary>
/// Contains various enums used in the game.
/// </summary>
public static class Enums
{
    /// <summary>
    /// Types of ammunition.
    /// </summary>
    public enum AmmoType
    {
        None,
        Bullets,
        Shells,
        Infinite
    }

    /// <summary>
    /// Types of healing potions.
    /// </summary>
    public enum PotionType
    {
        None,
        Antibiotics,
        Bandage,
        Painkillers
    }
}