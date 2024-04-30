using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Weapon : MonoBehaviour
{
    #region Fields

    [Header("Weapon Settings")]
    [Tooltip("If the weapon can be used.")]
    public bool IsAvailable = false;
    [Tooltip("The time between shots in seconds.")]
    [SerializeField] private float _timeBetweenShots = 0.5f;
    [Tooltip("The time it takes to reload the weapon in seconds.")]
    [SerializeField] private float _timeOfReload = 1f;
    [Tooltip("The fire range of the weapon in meters.")]
    [SerializeField] private float _fireRange = 100f;
    [Tooltip("The damage of the weapon.")]
    [SerializeField] private int _damage = 10;
    [Tooltip("The amount of ammo in the magazine.")]
    [SerializeField] private int _magazineCapacity = 10;
    [Tooltip("The amount of ammo a magazine currently has.")]
    public int CurrentAmmoInClip = 5;
    [Tooltip("If the weapon is currently equipped.")]
    public bool IsEquipped = false;
    [Tooltip("The blood particle effect for hit impact.")]
    [SerializeField] private GameObject _bloodEffect;

    [Space(10)]
    [Header("Ammo Settings")]
    [Tooltip("The type of ammo this weapon uses.")]
    public Ammo.AmmoType AmmoType;
    [Tooltip("The ammo slot this weapon uses. Should be on the player.")]
    [SerializeField] private Ammo _ammoSlot;

    // Cooldowns
    private bool _canShoot = true;
    private bool _canReload = true;

    // Layer mask for raycasting
    private LayerMask _allLayersExceptCharacter = ~(1 << 7);
    private LayerMask _enemyLayer = 1 << 8;

    #endregion




    #region Unity Callbacks

    // Set the weapon as equipped when enabled for save data
    void OnEnable()
    {
        _canShoot = true;
        _canReload = true;
        IsEquipped = true;
    }

    // Set the weapon as unequipped when disabled for save data
    void OnDisable()
    {
        IsEquipped = false;
    }

    #endregion




    #region Public Methods

    // Execute the weapon's fire action from the PlayerController
    public void Execute(Weapon weapon)
    {
        if (weapon == null || weapon.AmmoType == Ammo.AmmoType.None) return;
        else if (_canShoot) Fire(weapon);
    }

    // Execute the weapon's reload action from the PlayerController
    public void ExecuteReload()
    {
        if (_canReload) Reload();
    }

    // Get the weapon's stats for the InventoryManager
    public string GetWeaponStats()
    {
        // Order - Firepower: Reload Speed: Fire Delay: Range: Capacity: (separated with \n)
        return $"{_damage}\n{_timeOfReload}\n{_timeBetweenShots}\n{_fireRange}\n{_magazineCapacity}";
    }

    #endregion




    #region Private Methods

    // Fire the weapon
    private void Fire(Weapon weapon)
    {
        // Check if the weapon can shoot
        if (!_canShoot) return;
        else _canShoot = false;

        // Check if the weapon has enough ammo
        if (!HasEnoughAmmo())
        {
            Debug.Log("Fire: Out of ammo");
            AudioManager.Instance.PlayClipOneShot(AudioManager.Instance.PlayerSpeaker2, "empty shot", 1f, 1f);
            return;
        }

        // Play the shoot animation and noise and reduce ammo
        ReduceAmmo();
        GameManager.Instance.PlayerAnimManager.ShootAnimation();
        GameManager.Instance.Player.GetComponent<NoiseController>().TriggerShootNoise();

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, _fireRange, _allLayersExceptCharacter))
        {
            // Check if the raycast hit an object
            if (hit.transform != null) HandleHit(hit);
            else StartCoroutine(ResetShootCooldown());
        }
    }

    private void HandleHit(RaycastHit hit)
    {
        Debug.Log("Hit: " + hit.transform.name);

        // Blood effect for hit impact of enemies
        if (hit.transform.gameObject.layer == _enemyLayer)
        {
            Debug.Log("Hit: Blood effect");
            Instantiate(_bloodEffect, hit.point, Quaternion.LookRotation(hit.normal), hit.transform);
            Destroy(_bloodEffect, 0.6f); // Destroy blood effect after a short duration
        }

        // Damage enemies
        if (hit.transform.parent.GetComponent<Enemy>() != null)
        {
            Debug.Log("Hit: Enemy hit");
            Enemy enemy = hit.transform.parent.GetComponent<Enemy>();
            enemy.TakeDamage(_damage);
            PlayHitSound(enemy.gameObject);
        }
        // Damage bosses
        else if (hit.transform.GetComponent<Boss>() != null)
        {
            Debug.Log("Hit: Boss hit");
            Boss boss = hit.transform.GetComponent<Boss>();
            boss.GetHit();
            PlayHitSound(boss.gameObject);
        }
        // Damage mannequins or non-enemy objects
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
                Debug.Log("Hit: Mannequin hit");
                currentParent.GetComponent<Mannequin>().GetHit(hit.transform.gameObject);
                PlayHitSound(currentParent.gameObject);
            }
            else
            {
                Debug.Log("No target hit");
            }
        }

        // Reset the shoot cooldown
        StartCoroutine(ResetShootCooldown());
    }

    private void Reload()
    {
        // Check if the weapon can reload
        if (_canReload)
        {
            _canReload = false;
            _canShoot = false;
        }
        else return;

        // Check if the weapon has enough ammo to reload
        int ammoToReload = Mathf.Clamp(_magazineCapacity - CurrentAmmoInClip, 0, InventoryManager.Instance.GetInventoryAmmo(AmmoType));
        
        // Reload the weapon
        if (ammoToReload > 0)
        {
            InventoryManager.Instance.RemoveAmmo(AmmoType, ammoToReload);
            CurrentAmmoInClip += ammoToReload;
            UpdateAmmoUI();
            GameManager.Instance.PlayerAnimManager.ReloadAnimation();
        }
        // Display error message if no ammo to reload
        else
        {
            Debug.Log("Reload failed: No ammo to reload");
            AudioManager.Instance.PlayClipOneShot(AudioManager.Instance.PlayerSpeaker2, "error", 0.6f, 1f);
        }

        StartCoroutine(ResetReloadCooldown());
    }

    // Check if the weapon has enough ammo to shoot
    private bool HasEnoughAmmo()
    {
        return CurrentAmmoInClip > 0 || AmmoType == Ammo.AmmoType.Infinite;
    }

    // Reduce the ammo in the weapon
    private void ReduceAmmo()
    {
        if (AmmoType != Ammo.AmmoType.Infinite)
        {
            CurrentAmmoInClip--;
            _ammoSlot.ReduceCurrentAmmo(AmmoType);

            // SET UI FOR BULLETS
            UpdateAmmoUI();
        }
    }

    // Play the hit sound for the object hit
    private void PlayHitSound(GameObject obj)
    {
        AudioManager.Instance.PlayClipOneShot(obj.GetInstanceID(), "bullet hit", 0.6f, 1f);
    }

    // Update the ammo UI
    private void UpdateAmmoUI()
    {
        int inventoryAmmo = InventoryManager.Instance.GetInventoryAmmo(AmmoType);
        GameManager.Instance.Player.SetBulletsUI(CurrentAmmoInClip, inventoryAmmo);
    }

    #endregion




    #region Coroutines

    // Reset the shoot cooldown
    private IEnumerator ResetShootCooldown()
    {
        Debug.Log("Fire: Waiting for next shot");
        yield return new WaitForSeconds(_timeBetweenShots);
        _canShoot = true;
    }

    // Reset the reload cooldown
    private IEnumerator ResetReloadCooldown()
    {
        Debug.Log("Reload: Waiting for reload");
        yield return new WaitForSeconds(_timeOfReload);
        _canReload = true;
        _canShoot = true;
    }

    #endregion
}