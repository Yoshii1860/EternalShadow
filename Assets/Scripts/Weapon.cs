using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Weapon : MonoBehaviour
{
    [Header("Weapon Settings")]
    [Tooltip("The time between shots in seconds.")]
    [SerializeField] float timeBetweenShots = 0.5f;
    [Tooltip("The time it takes to reload the weapon in seconds.")]
    [SerializeField] float timeOfReload = 1f;
    [Tooltip("The range of the weapon in meters.")]
    [SerializeField] float range = 100f;
    [Tooltip("The damage of the weapon.")]
    [SerializeField] float damage = 10f;
    [Tooltip("The amount of ammo in the magazine.")]
    [SerializeField] int magazine = 10;
    [Tooltip("The amount of ammo a magazine currently has.")]
    [SerializeField] int magazineCount = 0;
    [Space(10)]

    [Header("Ammo Settings")]
    [Tooltip("The type of ammo this weapon uses.")]
    [SerializeField] Ammo.AmmoType ammoType;
    [Tooltip("The ammo slot this weapon uses. Should be on the player.")]
    [SerializeField] Ammo ammoSlot;

    bool canShoot = true;

    void OnEnable()
    {
        canShoot = true;
    }

    void Start()
    {
        magazineCount = magazine;
    }

    public void Execute()
    {
        if(canShoot) StartCoroutine(Shoot());
    }

    public void ExecuteReload()
    {
        if(canShoot) StartCoroutine(Reload());
    }

    IEnumerator Shoot()
    {
        canShoot = false;
        if(ammoSlot.GetCurrentAmmo(ammoType) >= 1 && magazineCount >= 1)
        {
            Debug.Log("Shooting");
            magazineCount--;
            ProcessRaycast();
            ammoSlot.ReduceCurrentAmmo(ammoType);
        }
        else
        {
            Debug.Log("Shoot: Out of ammo");
        }
        yield return new WaitForSeconds(timeBetweenShots);
        canShoot = true;
    }

    IEnumerator Reload()
    {
        canShoot = false;
        int currentAmmo = ammoSlot.GetCurrentAmmo(ammoType);
        if(magazineCount < magazine && currentAmmo - magazineCount >= 1)
        {
            Debug.Log("Reloading");
            if(currentAmmo >= magazine)
            {
                magazineCount = magazine;
            }
            else
            {
                magazineCount = currentAmmo;
            }
        }
        else
        {
            Debug.Log("Reload: Out of ammo");
        }
        yield return new WaitForSeconds(timeOfReload);
        canShoot = true;
    }

    void ProcessRaycast()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, range))
        {
            Debug.Log(hit.transform.gameObject.name);
        }
        else
        {
            return;
        }
    }
}
