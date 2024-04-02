using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Weapon : MonoBehaviour
{
    #region Fields

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
    [Tooltip("The blood particle effect.")]
    [SerializeField] GameObject bloodEffect;

    [Space(10)]
    [Header("Ammo Settings")]
    [Tooltip("The type of ammo this weapon uses.")]
    public Ammo.AmmoType ammoType;
    [Tooltip("The ammo slot this weapon uses. Should be on the player.")]
    [SerializeField] Ammo ammoSlot;

    bool canShoot = true;
    bool canReload = true;
    LayerMask allLayersExceptCharacter = ~(1 << 7);

    #endregion

    #region Unity Callbacks

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

    #endregion

    #region Public Methods

    public void Execute(Weapon weapon)
    {
        if (weapon == null) return;
        else if (canShoot) StartCoroutine(Shoot(weapon));
    }

    public void ExecuteReload()
    {
        if (canReload) StartCoroutine(Reload());
    }

    public string GetWeaponStats()
    {
        // Order - Firepower: Reload Speed: Shoot Delay: Range: Capacity: (separated with \n)
        // Used for InventoryManager
        return $"{damage}\n{timeOfReload}\n{timeBetweenShots}\n{range}\n{magazine}";
    }

    #endregion

    #region Coroutines

    IEnumerator Shoot(Weapon weapon)
    {
        if (string.Compare(weapon.transform.name, "Pistol") != 0) yield break;
        canShoot = false;

        if (ammoSlot.GetCurrentAmmo(ammoType) >= 1 && magazineCount >= 1)
        {
            Debug.Log("Shooting");
            GameManager.Instance.playerAnimController.ShootAnimation();

            // if weapon has a NoiseController, make noise
            NoiseController noiseController = GameManager.Instance.player.GetComponent<NoiseController>();
            if (noiseController != null)
            {
                noiseController.ShootNoise();
            }

            RaycastHit hit;
            Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, range, allLayersExceptCharacter);

            if (hit.transform != null)
            {
                Debug.Log("Target Hit: " + hit.transform.gameObject.name);
                if (ammoType != Ammo.AmmoType.Infinite)
                {
                    magazineCount--;

                    // SET UI FOR BULLETS
                    int inventoryAmmo = InventoryManager.Instance.GetInventoryAmmo(ammoType);
                    GameManager.Instance.player.SetBulletsUI(magazineCount, inventoryAmmo);

                    ammoSlot.ReduceCurrentAmmo(ammoType);
                    // Hit animation
                }

                // Check if target is an enemy by Layer "Enemies"
                if (hit.transform.gameObject.layer == 8)
                {
                    GameObject obj = Instantiate(bloodEffect, hit.point, Quaternion.LookRotation(hit.normal), hit.transform);
                    Destroy(obj, 0.6f);

                    Boss boss = hit.transform.GetComponent<Boss>();
                    // Slender & Girl have script on parent
                    Enemy parentEnemy = hit.transform.parent.GetComponent<Enemy>();
                    if (boss != null)
                    {
                        boss.GetHit();
                    }
                    else if (parentEnemy != null)
                    {
                        parentEnemy.TakeDamage(damage);
                        // Hit animation
                    }
                    else
                    {
                        // Recursive hit for Mannequin as it has several children objects for hit detection
                        Transform currentParent = hit.transform;
                        while (currentParent != null && currentParent.GetComponent<Mannequin>() == null)
                        {
                            currentParent = currentParent.parent;
                        }

                        if (currentParent != null)
                        {
                            currentParent.GetComponent<Mannequin>().Hit(hit.transform.gameObject);
                        }
                    }
                }
            }
            else
            {
                Debug.Log("No target hit");
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
            AudioManager.Instance.PlaySoundOneShot(AudioManager.Instance.playerSpeaker2, "empty shot", 1f, 1f);
        }

        yield return new WaitForSeconds(timeBetweenShots);
        canShoot = true;
    }

    IEnumerator Reload()
    {
        canShoot = false;
        canReload = false;

        int currentInventoryAmmo = InventoryManager.Instance.GetInventoryAmmo(ammoType);

        if (currentInventoryAmmo >= 1 && magazineCount < magazine)
        {
            Debug.Log("Reloading");

            if (currentInventoryAmmo <= magazine - magazineCount)
            {
                magazineCount += currentInventoryAmmo;
                InventoryManager.Instance.RemoveAmmo(ammoType, currentInventoryAmmo);

                Debug.Log("Reloading Amount: " + currentInventoryAmmo);
            }
            else
            {
                int ammoNeeded = magazine - magazineCount;
                magazineCount += ammoNeeded;
                InventoryManager.Instance.RemoveAmmo(ammoType, ammoNeeded);

                Debug.Log("Reloading Amount: " + ammoNeeded);
            }

            // SET UI FOR BULLETS
            int inventoryAmmo = InventoryManager.Instance.GetInventoryAmmo(ammoType);
            Debug.Log("Inventory Ammo: " + inventoryAmmo);
            GameManager.Instance.player.SetBulletsUI(magazineCount, inventoryAmmo);

            GameManager.Instance.playerAnimController.ReloadAnimation();
        }
        else
        {
            Debug.Log("Reload failed.");
            AudioManager.Instance.PlaySoundOneShot(AudioManager.Instance.playerSpeaker2, "error", 0.6f, 1f);
        }

        yield return new WaitForSeconds(timeOfReload);
        canShoot = true;
        canReload = true;
    }

    #endregion
}