using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Weapon : MonoBehaviour
{
    [Header("Weapon Settings")]
    [Tooltip("The time between shots in seconds.")]
    [SerializeField] float timeBetweenShots = 0.5f;
    [Tooltip("The range of the weapon in meters.")]
    [SerializeField] float range = 100f;
    [Tooltip("The damage of the weapon.")]
    [SerializeField] float damage = 10f;
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

    public void Execute()
    {
        StartCoroutine(Shoot());
    }

    IEnumerator Shoot()
    {
        canShoot = false;
        if(ammoSlot.GetCurrentAmmo(ammoType) >= 1)
        {
            Debug.Log("Shooting");
            ProcessRaycast();
            ammoSlot.ReduceCurrentAmmo(ammoType);
        }
        else
        {
            Debug.Log("Out of ammo");
        }
        yield return new WaitForSeconds(timeBetweenShots);
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
