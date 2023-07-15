using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSwitcher : MonoBehaviour
{
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

    private void SwitchWeapon(bool upOrDown)
    {
        foreach (Transform weapon in transform)
        {
            // If the weapon is active, deactivate it
            if (weapon.gameObject.activeSelf)
            {
                weapon.gameObject.SetActive(false);
                if (upOrDown)
                {
                    // Set the next weapon to active
                    if (weapon.GetSiblingIndex() - 1 >= 0)
                    {
                        transform.GetChild(weapon.GetSiblingIndex() - 1).gameObject.SetActive(true);
                        break;
                    }
                    else
                    {
                        // If the object is the last child, set the first child to active
                        transform.GetChild(transform.childCount - 1).gameObject.SetActive(true);
                        break;
                    }
                }
                else if (!upOrDown)
                {
                    // Set the previous weapon to active
                    if (weapon.GetSiblingIndex() + 1 < transform.childCount)
                    {
                        transform.GetChild(weapon.GetSiblingIndex() + 1).gameObject.SetActive(true);
                        break;
                    }
                    else
                    {
                        // If the object is the first child, set the last child to active
                        transform.GetChild(0).gameObject.SetActive(true);
                        break;
                    }
                }
            }
        }
    }
}
