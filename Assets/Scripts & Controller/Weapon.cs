using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Weapon : MonoBehaviour
{
    [Header("Weapon Settings")]
    [Tooltip("If the weapon can be used.")]
    public bool isAvailable = false;
    [Tooltip("The time between shots in seconds.")]
    [SerializeField] float timeBetweenShots = 0.5f;
    [Tooltip("The time it takes to reload the weapon in seconds.")]
    [SerializeField] float timeOfReload = 1f;
    [Tooltip("The range of the weapon in meters.")]
    [SerializeField] float range = 100f;
    [Tooltip("The damage of the weapon.")]
    [SerializeField] int damage = 10;
    [Tooltip("The amount of ammo in the magazine.")]
    [SerializeField] int magazine = 10;
    [Tooltip("The amount of ammo a magazine currently has.")]
    public int magazineCount = 5;
    [Tooltip("If the weapon is currently equipped.")]
    public bool isEquipped = false;

    [Space(10)]
    [Header("Ammo Settings")]
    [Tooltip("The type of ammo this weapon uses.")]
    public Ammo.AmmoType ammoType;
    [Tooltip("The ammo slot this weapon uses. Should be on the player.")]
    [SerializeField] Ammo ammoSlot;

    bool canShoot = true;
    bool canReload = true;

    void OnEnable()
    {
        canShoot = true;
        canReload = true;
        isEquipped = true;
    }

    void OnDisable()
    {
        isEquipped = false;
    }

    public void Execute()
    {
        if(canShoot) StartCoroutine(Shoot());
    }

    public void ExecuteReload()
    {
        if(canReload) StartCoroutine(Reload());
    }
    
    public string GetWeaponStats()
    {
        // Order - Firepower: Reload Speed: Shoot Delay: Range: Capacity: (separated with \n)
        // Used for InventoryManager
        return damage.ToString() + "\n" + timeOfReload.ToString() + "\n" + timeBetweenShots.ToString() + "\n" + range.ToString() + "\n" + magazine.ToString();
    }

    IEnumerator Shoot()
    {
        canShoot = false;

        if(ammoSlot.GetCurrentAmmo(ammoType) >= 1 && magazineCount >= 1)
        {
            Debug.Log("Shooting");
            
            // if weapon has a NoiseController, make noise
            NoiseController noiseController = GetComponent<NoiseController>();
            if (noiseController != null)
            {
                noiseController.ShootNoise();
            }

            GameObject target = ProcessRaycast();
            if (target != null)
            {
                if (ammoType != Ammo.AmmoType.Infinite)
                {
                    magazineCount--;

                    // SET UI FOR BULLETS
                    int inventoryAmmo = InventoryManager.Instance.GetInventoryAmmo(ammoType);
                    GameManager.Instance.player.SetBulletsUI(magazineCount, inventoryAmmo);

                    ammoSlot.ReduceCurrentAmmo(ammoType);
                    // Hit animation
                }

                if (target.GetComponent<Enemy>() != null)
                {
                    target.GetComponent<Enemy>().TakeDamage(damage);
                    // Hit animation
                }
            }
            else
            {
                if (ammoType != Ammo.AmmoType.Infinite)
                {
                    magazineCount--;
                    
                    // SET UI FOR BULLETS
                    int inventoryAmmo = InventoryManager.Instance.GetInventoryAmmo(ammoType);
                    GameManager.Instance.player.SetBulletsUI(magazineCount, inventoryAmmo);

                    ammoSlot.ReduceCurrentAmmo(ammoType);
                }
            }
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
        canReload = false;
        int currentInventoryAmmo = InventoryManager.Instance.GetInventoryAmmo(ammoType);
        if(currentInventoryAmmo >= 1 && magazineCount < magazine)
        {
            Debug.Log("Reloading");
            if(currentInventoryAmmo <= magazine - magazineCount)
            {
                magazineCount += currentInventoryAmmo;
                InventoryManager.Instance.RemoveAmmo(ammoType, currentInventoryAmmo);

                Debug.Log("Reloading Ammount: " + currentInventoryAmmo);
            }
            else
            {
                int ammoNeeded = magazine - magazineCount;
                magazineCount += ammoNeeded;
                InventoryManager.Instance.RemoveAmmo(ammoType, ammoNeeded);

                Debug.Log("Reloading Ammount: " + ammoNeeded);
            }

            // SET UI FOR BULLETS
            int inventoryAmmo = InventoryManager.Instance.GetInventoryAmmo(ammoType);
            Debug.Log("Inventory Ammo: " + inventoryAmmo);
           GameManager.Instance.player.SetBulletsUI(magazineCount, inventoryAmmo);

        }
        else
        {
            Debug.Log("Reload failed.");
        }
        yield return new WaitForSeconds(timeOfReload);
        canShoot = true;
        canReload = true;
    }

    GameObject ProcessRaycast()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, range))
        {

            return hit.transform.gameObject;
        }
        else
        {
            return null;
        }
    }
}