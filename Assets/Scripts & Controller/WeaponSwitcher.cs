using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSwitcher : MonoBehaviour
{
    #region Fields

    // The index of the current weapon
    private int _currentWeaponIndex = 0;
    // Avoid switching weapons too fast
    private bool _canSwitch = false;
    // The _pistolWeaponScript weapon
    [Tooltip("The _pistolWeaponScript weapon in the weapon object of the player.")]
    [SerializeField] Weapon _pistolWeaponScript;

    #endregion




    #region Public Methods

    // Execute the weapon switch based on the mouse scroll input
    // Usually called from the PlayerController
    public void Execute(float mouseScroll)
    {
        if (_canSwitch) return;

        // Check if the pistol weapon is available - currently just one weapon available
        if (_pistolWeaponScript.IsAvailable)
        {
            if (mouseScroll > 0)
            {
                // Scroll up to switch to the next weapon
                SwitchWeapon(true);
            }
            else if (mouseScroll < 0)
            {
                // Scroll down to switch to the previous weapon
                SwitchWeapon(false);
            }
        }
    }

    // Select a weapon by its item
    public void SelectWeapon(Item item)
    {
        foreach (ItemController weapon in transform.gameObject.GetComponentsInChildren<ItemController>(true))
        {
            if (weapon.Item.DisplayName == item.DisplayName)
            {
                // Deactivate the current weapon
                transform.GetChild(_currentWeaponIndex).gameObject.SetActive(false);

                // Activate the selected weapon
                weapon.gameObject.SetActive(true);

                // Set the UI to magazine Count + inventory ammo
                UpdateAmmoUI(weapon.GetComponent<Weapon>());

                // Update the current weapon index
                _currentWeaponIndex = weapon.transform.GetSiblingIndex();
            }
        }
    }

    // Switch to the next or previous weapon
    public void SwitchWeapon(bool upOrDown)
    {
        _canSwitch = true;

        int nextWeaponIndex = CalculateNextWeaponIndex(upOrDown);

        Transform nextWeapon = transform.GetChild(nextWeaponIndex);
        Transform currentWeapon = transform.GetChild(_currentWeaponIndex);
        Weapon nextWeaponComponent = nextWeapon.GetComponent<Weapon>();

        // Check if the next weapon is available
        while (!nextWeaponComponent.IsAvailable)
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

        // Set the UI to magazine Count + inventory ammo
        UpdateAmmoUI(nextWeaponComponent);

        // Update the current weapon index
        _currentWeaponIndex = nextWeaponIndex;

        // Send information to Anim Controller
        StartCoroutine(SwitchWeaponAnimation(currentWeapon, nextWeapon));
    }

    #endregion




    #region Private Methods

    // Calculate the index of the next weapon
    private int CalculateNextWeaponIndex(bool isScrollUp)
    {
        int nextWeaponIndex = isScrollUp ? _currentWeaponIndex - 1 : _currentWeaponIndex + 1;

        if (nextWeaponIndex < 0 || nextWeaponIndex >= transform.childCount)
        {
            // Wrap around to the first or last weapon if out of bounds
            nextWeaponIndex = isScrollUp ? transform.childCount - 1 : 0;
        }

        return nextWeaponIndex;
    }

    // Update the ammo UI
    private void UpdateAmmoUI(Weapon weapon)
    {
        int inventoryAmmo = InventoryManager.Instance.GetInventoryAmmo(weapon.AmmoType);
        GameManager.Instance.Player.SetBulletsUI(weapon.CurrentAmmoInClip, inventoryAmmo);
    }

    #endregion




    #region Coroutines

    // Switch weapon animation
    IEnumerator SwitchWeaponAnimation(Transform currentWeapon, Transform nextWeapon)
    {
        yield return new WaitUntil(() => GameManager.Instance.CurrentSubGameState == GameManager.SubGameState.DEFAULT);
        GameManager.Instance.PlayerAnimManager.SetWeapon(currentWeapon, nextWeapon);
        yield return new WaitForSeconds(0.25f);
        _canSwitch = false;
    }

    #endregion
}