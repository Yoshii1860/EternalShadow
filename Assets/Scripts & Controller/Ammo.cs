using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ammo : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] AmmoSlot[] ammoSlots;

    #endregion

    #region Enums

    public enum AmmoType
    {
        None,
        Bullets,
        Shells,
        Infinite
    }

    #endregion

    #region AmmoSlot Class

    [System.Serializable]
    private class AmmoSlot
    {
        public AmmoType ammoType;
        public int ammoAmount;
    }

    #endregion

    #region Ammo Management

    /// <summary>
    /// Gets the current ammo amount for the specified ammo type.
    /// </summary>
    public int GetCurrentAmmo(AmmoType ammoType)
    {
        return GetAmmoSlot(ammoType).ammoAmount;
    }

    /// <summary>
    /// Reduces the current ammo amount for the specified ammo type.
    /// </summary>
    public void ReduceCurrentAmmo(AmmoType ammoType)
    {
        if (ammoType != AmmoType.Infinite)
        {
            GetAmmoSlot(ammoType).ammoAmount--;
        }
    }

    /// <summary>
    /// Increases the current ammo amount for the specified ammo type.
    /// </summary>
    public void IncreaseCurrentAmmo(AmmoType ammoType, int ammoAmount)
    {
        GetAmmoSlot(ammoType).ammoAmount += ammoAmount;
    }

    /// <summary>
    /// Sets the ammo amount for the specified ammo type.
    /// </summary>
    public void SetAmmoOnLoad(AmmoType ammoType, int newAmmoAmount)
    {
        GetAmmoSlot(ammoType).ammoAmount = newAmmoAmount;
    }

    /// <summary>
    /// Resets all ammo amounts to zero.
    /// </summary>
    public void ResetAmmo()
    {
        foreach (AmmoSlot slot in ammoSlots)
        {
            slot.ammoAmount = 0;
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Gets the AmmoSlot for the specified ammo type.
    /// </summary>
    private AmmoSlot GetAmmoSlot(AmmoType ammoType)
    {
        foreach (AmmoSlot slot in ammoSlots)
        {
            if (slot.ammoType == ammoType)
            {
                return slot;
            }
        }
        return null;
    }

    #endregion
}