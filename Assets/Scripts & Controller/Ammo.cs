using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ammo : MonoBehaviour
{
    [SerializeField] AmmoSlot[] ammoSlots;

    public enum AmmoType
    {
        None,
        Bullets,
        Shells,
        Infinite
    }

    [System.Serializable]
    private class AmmoSlot
    {
        public AmmoType ammoType;
        public int ammoAmount;
    }

    public int GetCurrentAmmo(AmmoType ammoType)
    {
        return GetAmmoSlot(ammoType).ammoAmount;
    }

    public void ReduceCurrentAmmo(AmmoType ammoType)
    {
        if (ammoType != AmmoType.Infinite)
        {
            GetAmmoSlot(ammoType).ammoAmount--;
        }
    }

    public void IncreaseCurrentAmmo(AmmoType ammoType, int ammoAmount)
    {
        GetAmmoSlot(ammoType).ammoAmount += ammoAmount;
    }

    public void SetAmmoOnLoad(AmmoType ammoType, int newAmmoAmount)
    {
        GetAmmoSlot(ammoType).ammoAmount = newAmmoAmount;
    }

    public void ResetAmmo()
    {
        foreach (AmmoSlot slot in ammoSlots)
        {
            slot.ammoAmount = 0;
        }
    }

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
}
