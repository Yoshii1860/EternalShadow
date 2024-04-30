using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ammo : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] AmmoSlot[] _ammoSlots;

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
        public AmmoType AmmoType;
        public int AmmoAmount;
    }

    #endregion





    #region Ammo Management

    /// Gets the current ammo amount for the specified ammo type.
    public int GetCurrentAmmo(AmmoType ammoType)
    {
        var slot = GetAmmoSlot(ammoType);
        return slot?.AmmoAmount ?? 0;
    }


    /// Reduces the current ammo amount for the specified ammo type.
    public void ReduceCurrentAmmo(AmmoType ammoType)
    {
        if (ammoType != AmmoType.Infinite)
        {
            var slot = GetAmmoSlot(ammoType);
            if (slot != null)
            {
                slot.AmmoAmount--;
            }
        }
    }

    /// Increases the current ammo amount for the specified ammo type.
    public void IncreaseCurrentAmmo(AmmoType ammoType, int ammoAmount)
    {
        var slot = GetAmmoSlot(ammoType);
        if (slot != null)
        {
            slot.AmmoAmount += ammoAmount;
        }
    }


    /// Sets the ammo amount for the specified ammo type.
    public void SetAmmoOnLoad(AmmoType ammoType, int newAmmoAmount)
    {
        var slot = GetAmmoSlot(ammoType);
        if (slot != null)
        {
            slot.AmmoAmount = newAmmoAmount;
        }
    }


    /// Resets all ammo amounts to zero.
    public void ResetAmmo()
    {
        foreach (var slot in _ammoSlots)
        {
            slot.AmmoAmount = 0;
        }
    }

    #endregion





    #region Private Methods

    /// Gets the AmmoSlot for the specified ammo type.
    private AmmoSlot GetAmmoSlot(AmmoType ammoType)
    {
          foreach (AmmoSlot slot in _ammoSlots)
        {
            if (slot.AmmoType == ammoType)
            {
                return slot;
            }
        }
        return null;
    }

    #endregion
}