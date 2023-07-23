using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSwitcher : MonoBehaviour
{
    private int currentWeaponIndex = 0;

    public void Execute(float mouseScroll)
    {
        if (mouseScroll > 0)
        {
            // Switch to the next weapon
            SwitchWeapon(true);
        }
        else if (mouseScroll < 0)
        {
            // Switch to the previous weapon
            SwitchWeapon(false);
        }
    }

    public void SwitchWeapon(bool upOrDown)
    {
        int nextWeaponIndex = upOrDown ? currentWeaponIndex - 1 : currentWeaponIndex + 1;

        if (nextWeaponIndex < 0 || nextWeaponIndex >= transform.childCount)
        {
            // Wrap around to the first or last weapon if out of bounds
            nextWeaponIndex = upOrDown ? transform.childCount - 1 : 0;
        }

        Transform nextWeapon = transform.GetChild(nextWeaponIndex);
        Transform currentWeapon = transform.GetChild(currentWeaponIndex);
        Weapon nextWeaponComponent = nextWeapon.GetComponent<Weapon>();

        // Check if the next weapon is available
        while (!nextWeaponComponent.isAvailable)
        {
            nextWeaponIndex = upOrDown ? nextWeaponIndex - 1 : nextWeaponIndex + 1;

            if (nextWeaponIndex < 0 || nextWeaponIndex >= transform.childCount)
            {
                // Wrap around to the first or last weapon if out of bounds
                nextWeaponIndex = upOrDown ? transform.childCount - 1 : 0;
            }

            nextWeapon = transform.GetChild(nextWeaponIndex);
            nextWeaponComponent = nextWeapon.GetComponent<Weapon>();
        }

        // Deactivate the current weapon
        currentWeapon.gameObject.SetActive(false);

        // Activate the next weapon
        nextWeapon.gameObject.SetActive(true);

        // Get ammo from inventory
        int inventoryAmmo = InventoryManager.Instance.GetInventoryAmmo(nextWeapon.GetComponent<Weapon>().ammoType);
        // Set the UI to magazine Count + inventory ammo
        transform.parent.parent.GetComponent<Player>().SetBulletsUI(nextWeapon.GetComponent<Weapon>().magazineCount, inventoryAmmo);

        // Update the current weapon index
        currentWeaponIndex = nextWeaponIndex;
    }
}
