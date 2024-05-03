using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class InventoryManager : MonoBehaviour
{
    #region Singleton

    public static InventoryManager Instance;

    // Create a static reference to the instance
    private void Awake()
    {
        InitializeSingleton();
    }

    void InitializeSingleton()
    {
        // Singleton pattern to ensure only one instance exists
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Keep the InventoryManager between scene changes
        DontDestroyOnLoad(gameObject);
    }

    #endregion




    #region Fields

        public List<Item> Items = new List<Item>();

        [Space(10)]
        [Header("Inventory Settings")]
        [Tooltip("The prefab that will be used to display the inventory items")]
        [SerializeField] private GameObject _inventoryItemPrefab;
        [Tooltip("The prefab that will be used to display the inventory item display")]
        [SerializeField] private GameObject _itemDisplayPrefab;
        [Tooltip("The canvas (child of viewport) that will be used to display the items in the inventory")]
        [SerializeField] private Transform _itemContentCanvas;
        [Tooltip("The canvas (child of viewport) that will be used to display the item display")]
        [SerializeField] private Transform _itemPreviewCanvas;
        [Tooltip("The child of inventory that will be used to display the doll")]
        [SerializeField] private GameObject _dollDisplayUI;
        [Tooltip("The text (child of doll dipsplay) that will be used to display the amount of horror dolls")]
        public TextMeshProUGUI HorrorDollCount;
        [Tooltip("The camera (child of inventory) that will be used to inspect items")]
        [SerializeField] private GameObject _inspectorCamera;
        [Tooltip("The object (child of inventory viewport) that will be used to display the item inspector")]
        [SerializeField] private GameObject _inspector;
        [Tooltip("The transform (child of camera) that will be used to parent the item to inspect")]
        [SerializeField] private Transform _inspectItemTransform;
        [Tooltip("The object (child of viewport) that will be used to display the status of the player")]
        [SerializeField] private GameObject _statsDisplay;
        [Tooltip("The image (child of stats) that will be used to display the health bar")]
        [SerializeField] private Image _healthBar;
        [Tooltip("The image (child of stats) that will be used to display the stamina bar")]
        [SerializeField] private Image _staminaBar;
        [Tooltip("The icon (child of stats) that will be used to display status effects")]
        [SerializeField] private GameObject _bleedingIcon;
        [Tooltip("The icon (child of stats) that will be used to display status effects")]
        [SerializeField] private GameObject _poisonedIcon;
        [Tooltip("The icon (child of stats) that will be used to display status effects")]
        [SerializeField] private GameObject _dizzyIcon;
        [Tooltip("The canvas (child of inventory) that will be used to display messages")]
        [SerializeField] private GameObject _messageCanvas;
        [Tooltip("The text (child of message canvas) that will be used to display messages")]
        [SerializeField] private TextMeshProUGUI _messageText;
        [Tooltip("The force that will be applied to the dropped item")]
        [SerializeField] private float _dropForce;
        [Tooltip("The maximum amount of items that can be stored in the inventory")]
        [SerializeField] private int _maxItems = 12;
        [Space(10)]

        [Tooltip("The GameObject (child of player) that contains the weapons")]
        public GameObject Weapons;
        [Space(10)]

        [Header("Inspector Settings")]
        [Tooltip("The sensitivity of the zoom when inspecting items")]
        [SerializeField] private float _zoomSensitivity = 0.01f;
        [Tooltip("The sensitivity of the look when inspecting items")]
        [SerializeField] private float _lookSensitivity = 0.5f;
        [Tooltip("The maximum zoom scale when inspecting items")]
        [SerializeField] private float _maxZoomScale = 4f;
        [Tooltip("The minimum zoom scale when inspecting items")]
        [SerializeField] private float _minZoomScale = 0.5f;
        [Space(10)]


        // Additional fields
        private Transform[] _gridLayouts;
        private Player _player;
        private Ammo _ammo;
        private GameObject _inspectorItem;
        private TextMeshProUGUI _healthText;
        private TextMeshProUGUI _maxHealthText;
        private TextMeshProUGUI _staminaText;
        private TextMeshProUGUI _maxStaminaText;
        // The Image components of the item actions
        Image[] _itemCanvasActions;

        [Header("Colors")]
        [Tooltip("The color of the selected item")]
        [SerializeField] private Color selectedColor;
        [Tooltip("The color of the unselected item")]
        [SerializeField] private Color unselectedColor;
        [Tooltip("The color of the highlighted item")]
        [SerializeField] private Color highlightedColor;
        [Space(10)]

        [Header("Only for Debugging)")]
        public Item SelectedItem;
        public int HighlightNumber;
        public int ItemActionNumber;
        public bool IsItemActionsOpen;
        public int ActionsChildCount;
        public bool IsInspecting = false;
        [Space(10)]

        [Header("AutoSave Condition")]
        [Tooltip("The AutoSave object that will be used to save the game before the boss fight")]
        [SerializeField] private AutoSave _autoSaveWithCondition;
        [SerializeField] private int _uniqueAutoSaveID;
        [SerializeField] private string[] _autoSaveConditionNames;
        [SerializeField] private int _conditionCounter = 0;
        [Space(10)]

        [SerializeField] private bool _debugMode;

    #endregion




    #region Initialization

    void Start()
    {
        InitializeReferences();
    }

    void InitializeReferences()
    {
        _gridLayouts = FindGridLayoutGroups();
        _itemContentCanvas = _gridLayouts[1];
        _itemPreviewCanvas = _gridLayouts[2];
        _healthText = _healthBar.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        _maxHealthText = _healthBar.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        _staminaText = _staminaBar.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        _maxStaminaText = _staminaBar.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
    }

    private Transform[] FindGridLayoutGroups(Transform parent = null)
    {
        if (parent == null)
        {
            parent = transform;
        }

        // Check if the current GameObject has a GridLayoutGroup component
        GridLayoutGroup gridLayoutGroup = parent.GetComponent<GridLayoutGroup>();
        if (gridLayoutGroup != null)
        {
            Transform viewport = gridLayoutGroup.transform.parent;

            // The target GridLayoutGroup is found!
            Transform[] gridLayoutGroups = viewport.GetComponentsInChildren<Transform>(true);

            return gridLayoutGroups;
        }
        else
        {
            Transform child = parent.GetChild(0);
            Transform[] result = FindGridLayoutGroups(child);
            if (result != null && result.Length > 0)
            {
                return result;
            }
        }

        // If no GridLayoutGroup was found, return null
        return null;
    }

    public void UpdateReferences()
    {
        _player = GameManager.Instance.Player.gameObject.GetComponent<Player>();
        Weapons = FindObjectOfType<WeaponSwitcher>().gameObject;
        _ammo = _player.GetComponent<Ammo>();
        if (_autoSaveWithCondition == null)
        {
            GameObject autoSavePool = GameObject.FindWithTag("SaveGamePool");
            foreach (AutoSave autoSaveObj in autoSavePool.GetComponentsInChildren<AutoSave>())
            {
                if (autoSaveObj.GetComponent<UniqueIDComponent>().UniqueID == _uniqueAutoSaveID.ToString())
                {
                    _autoSaveWithCondition = autoSaveObj;
                    break;
                }
            }
        }

        GetComponent<ItemActions>().UpdateReferences();
        GetComponent<ObjectHandler>().UpdateReferences();
        GetComponent<Potion>().UpdateReferences();
    }

    #endregion




    #region Auto Save Condition

    // Check if the item is one of the conditions for the AutoSave
    private void CheckAutoSaveCondition(Item item)
    {
        foreach (string condition in _autoSaveConditionNames)
        {
            if (item.name == condition)
            {
                _conditionCounter++;
                break;
            }
        }

        // if all items are in the inventory, set the condition to true
        if (_conditionCounter == _autoSaveConditionNames.Length)
        {
            _autoSaveWithCondition.ConditionMet = true;
        }
    }

    #endregion




    #region Item Management


    public void AddItem(Item item)
    {
        switch (item.Type)
        {
            case ItemType.Ammo:
                AddAmmo(item);
                break;
            case ItemType.Potion:
                AddPotion(item);
                break;
            case ItemType.Weapon:
                AddWeapon(item);
                break;
            default:
                Items.Add(item);
                CheckAutoSaveCondition(item);
                if (_debugMode) if (_debugMode) Debug.Log("InventoryManager - Add New Item (" + item.DisplayName + ")");
                break;
        }
    }

    private void AddAmmo(Item item)
    {
        Item existingItem = Items.Find(existingItem => existingItem.DisplayName == item.DisplayName);
        if (existingItem != null)
        {
            existingItem.Quantity += item.Quantity;
            _ammo.IncreaseCurrentAmmo(item.AmmoType, item.Quantity);
            SetUIForBullets(item);
            if (_debugMode) if (_debugMode) Debug.Log("InventoryManager - Add Existing Ammo (" + item.DisplayName + ")");
        }
        else
        {
            Item newItem = Instantiate(item);
            Items.Add(newItem);
            _ammo.IncreaseCurrentAmmo(item.AmmoType, item.Quantity);
            SetUIForBullets(item);
            if (_debugMode) if (_debugMode) Debug.Log("InventoryManager - Add New Ammo (" + item.DisplayName + ")");
        }
    }

    private void SetUIForBullets(Item item)
    {
        foreach(Transform weapon in Weapons.transform)
        {
            if (weapon.GetComponent<Weapon>().AmmoType == item.AmmoType &&
                weapon.gameObject.activeSelf == true)
            {
                Weapon weaponScript = weapon.GetComponent<Weapon>();
                int inventoryAmmoAmount = GetInventoryAmmo(weaponScript.AmmoType);
                _player.SetBulletsUI(weaponScript.CurrentAmmoInClip, inventoryAmmoAmount);
                break;
            }
        }
    }

    private void AddPotion(Item item)
    {
        Item existingItem = Items.Find(existingItem => existingItem.DisplayName == item.DisplayName);
        if (existingItem != null)
        {
            existingItem.Quantity++;
            if (_debugMode) if (_debugMode) Debug.Log("InventoryManager - Add Existing Potion (" + item.DisplayName + ")");
        }
        else
        {
            Items.Add(item);
            if (_debugMode) if (_debugMode) Debug.Log("InventoryManager - Add New Potion (" + item.DisplayName + ")");
        }
    }

    private void AddWeapon(Item item)
    {
        Items.Add(item);
        foreach (Transform weapon in Weapons.transform)
        {                    
            if (weapon.GetComponent<ItemController>() != null && weapon.GetComponent<ItemController>().Item.DisplayName == item.DisplayName)
            {
                Weapon weaponScript = weapon.GetComponent<Weapon>();
                weaponScript.IsAvailable = true;
                int inventoryAmmoAmount = GetInventoryAmmo(weaponScript.AmmoType);
                _player.SetBulletsUI(weaponScript.CurrentAmmoInClip, inventoryAmmoAmount);
                _ammo.IncreaseCurrentAmmo(weaponScript.AmmoType, weaponScript.CurrentAmmoInClip);
                while (weapon.gameObject.activeSelf == false)
                {
                    Weapons.GetComponent<WeaponSwitcher>().SwitchWeapon(true);
                }
                if (_debugMode) if (_debugMode) Debug.Log("InventoryManager - Add New Weapon (" + item.DisplayName + ")");
            }
        }
    }

    public void RemoveItem(Item item)
    {
        if (Items.Contains(item))
        {
            if (Items[Items.IndexOf(item)].Quantity > 1)
            {
                Items[Items.IndexOf(item)].Quantity -= 1;
                if (GameManager.Instance.InventoryCanvas.activeSelf)
                {
                    BackToSelection();
                    ListItems();
                }
            }
            else
            {
                Items.Remove(item);
                if (GameManager.Instance.InventoryCanvas.activeSelf)
                {
                    BackToSelection();
                    ListItems();
                }
            }
        }
    }

    public void RemoveAmmo(Ammo.AmmoType ammotype, int amount)
    {
        if (_debugMode) if (_debugMode) Debug.Log("InventoryManager.RemoveAmmo(" + ammotype + ", " + amount + ")");
        foreach (Item item in Items)
        {
            if (item.AmmoType == ammotype)
            {
                if (item.Quantity > amount)
                {
                    item.Quantity -= amount;
                    break;
                }
                else if (item.Quantity == amount)
                {
                    Items.Remove(item);
                    break;
                }
                else
                {
                    if (_debugMode) Debug.Log("InventoryManager.RemoveAmmo() - FAILED");
                }
            }
        }
    }

    public Item FindItem(string itemDisplayName)
    {
        foreach (Item _inventoryItemPrefab in Items)
        {
            if (_inventoryItemPrefab.DisplayName == itemDisplayName)
            {
                return _inventoryItemPrefab;
            }
        }
        return null;
    }

    public int GetInventoryAmmo(Ammo.AmmoType ammotype)
    {
        foreach (Item item in Items)
        {
            if (item.AmmoType == ammotype)
            {
                return item.Quantity;
            }
        }
        return 0;
    }

    public bool InventorySpaceCheck()
    {
        if (Items.Count >= _maxItems)
        {
            return true;
        }
        return false;
    }

    #endregion




    #region UI Handling

    public void ListItems()
    {
        // Clear list before showing items
        if (_debugMode) Debug.Log("InventoryManager.ListItems() - Clearing itemContent and itemPreview");
        foreach (Transform child in _itemContentCanvas)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in _itemPreviewCanvas)
        {
            Destroy(child.gameObject);
        }

        // Show items from inventory list in UI
        if (_debugMode) Debug.Log("InventoryManager.ListItems() - Showing " + Items.Count + " items in itemContent");
        foreach (Item item in Items)
        {
            GameObject obj = Instantiate(_inventoryItemPrefab, _itemContentCanvas);
            Image[] itemCanvasIcons = obj.GetComponentsInChildren<Image>();
            TextMeshProUGUI itemCanvasText = obj.GetComponentInChildren<TextMeshProUGUI>();

            if (item.IsUnique && item.Type != ItemType.Weapon)
            {
                // If the item is unique, hide the quantity
                itemCanvasText.enabled = false;
            }
            else if (item.Type == ItemType.Weapon)
            {
                // If item is weapon, show current ammo in magazine
                // loop through weapons to find the weapon with the same name as the item
                foreach (Transform weapon in Weapons.transform)
                {
                    if (weapon.GetComponent<ItemController>().Item.DisplayName == item.DisplayName)
                    {   
                        Weapon weaponScript = weapon.GetComponent<Weapon>();
                        if (weaponScript.AmmoType == Ammo.AmmoType.Infinite)
                        {
                            itemCanvasText.enabled = false;
                            break;
                        }
                        
                        // If it is, get the weapon stats and display them
                        string weaponAmmo = weaponScript.CurrentAmmoInClip.ToString();
                        itemCanvasText.text = weaponAmmo;
                        break;
                    }
                }
            }
            else
            {
                // If the item is not unique, show the quantity
                itemCanvasText.text = item.Quantity.ToString();
            }

            // Iterate through the Image components and find the desired one
            foreach (Image itemCanvasIcon in itemCanvasIcons)
            {
                if (itemCanvasIcon.gameObject != obj)
                {
                    itemCanvasIcon.sprite = item.Icon;
                    break;
                }
            }
            
            obj.GetComponentInChildren<ItemController>().Item = item;
        }

        // check if Items list is empty
        if (Items.Count > 0)
        {
            HighlightNumber = 0;
            SelectedItem = Items[HighlightNumber];
            ChangeSelectedItemColor(true, true);
        }
    }

    public void ShowItemDisplay()
    {
        if (Items.Count == 0) return;
        SelectedItem = Items[HighlightNumber];
        if (_debugMode) Debug.Log("InventoryManager.ShowItemDisplay() for" + SelectedItem.DisplayName);
        ChangeSelectedItemColor(false, false);

        ItemActionNumber = 0;
        IsItemActionsOpen = true;

        GameObject obj;
        // Instantiate ItemDisplay UI if it doesn't exist
        if (_itemPreviewCanvas.childCount == 0)
        {
            obj = Instantiate(_itemDisplayPrefab, _itemPreviewCanvas);
            if (_debugMode) Debug.Log("InventoryManager.ShowItemDisplay() - Instantiated ItemDisplay: " + obj.name);
        }
        else
        {
            // Clear the ItemDisplay UI
            foreach (Transform child in _itemPreviewCanvas)
            {
                Destroy(child.gameObject);
            }
            // Instantiate ItemDisplay UI
            obj = Instantiate(_itemDisplayPrefab, _itemPreviewCanvas);
            if (_debugMode) Debug.Log("InventoryManager.ShowItemDisplay() - Instantiated ItemDisplay: " + obj.name);
        }

        // Change icon of obj to item.icon
        Image[] itemCanvasIcons = obj.GetComponentsInChildren<Image>();
        if (_debugMode) Debug.Log("InventoryManager.ShowItemDisplay() - Changing icon to " + SelectedItem.Icon.name);
        // Iterate through the Image components and find the desired one
        foreach (Image itemCanvasIcon in itemCanvasIcons)
        {
            if (itemCanvasIcon.gameObject != obj)
            {
                itemCanvasIcon.sprite = SelectedItem.Icon;
                break;
            }
        }

        // Get all TextMeshProUGUI components
        TextMeshProUGUI[] itemCanvasTexts = obj.GetComponentsInChildren<TextMeshProUGUI>();
        if (_debugMode) Debug.Log("InventoryManager.ShowItemDisplay() - Changing text to " + SelectedItem.DisplayName);
        foreach (TextMeshProUGUI itemCanvasText in itemCanvasTexts)
        {
            // Change name and description of ItemDisplay
            if (itemCanvasText.gameObject.name == "ItemName")
            {
                itemCanvasText.text = SelectedItem.DisplayName;
            }
            else if (itemCanvasText.gameObject.name == "ItemDescription")
            {
                itemCanvasText.text = SelectedItem.Description;
            }
            // If item is unique we hide the quantity display
            else if (itemCanvasText.gameObject.name == "ItemCountText" && SelectedItem.IsUnique)
            {
                itemCanvasText.gameObject.SetActive(false);
            }
            // If item is not unique, get its child object and change the quantity
            else if (itemCanvasText.gameObject.name == "ItemCountText" && !SelectedItem.IsUnique)
            {
                itemCanvasText.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = SelectedItem.Quantity.ToString();
            }
            // If item is not a weapon, hide the stats display
            if (itemCanvasText.gameObject.name == "ItemStatsText" && SelectedItem.Type != ItemType.Weapon)
            {
                itemCanvasText.gameObject.SetActive(false);
            }
            else if (itemCanvasText.gameObject.name == "ItemStatsText" && SelectedItem.Type == ItemType.Weapon)
            {
                // loop through all the childs of weapons, check if item is the same item as the one displayed
                foreach (Transform weapon in Weapons.transform)
                {
                    if (weapon.GetComponent<ItemController>().Item.DisplayName == SelectedItem.DisplayName)
                    {
                        // If it is, get the weapon stats and display them
                        string weaponStats = weapon.GetComponent<Weapon>().GetWeaponStats();
                        itemCanvasText.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = weaponStats;
                        break;
                    }
                }
            }
        }
        
        // Get all children objects of obj and find the one named ItemActions
        foreach (RectTransform child in obj.GetComponentsInChildren<RectTransform>())
        {
            if (child.gameObject.name == "ItemActions")
            {
                // Get all image children of child and add them to itemCanvasActions
                _itemCanvasActions = child.GetComponentsInChildren<Image>();
                ActionsChildCount = child.childCount;
                ChangeSelectedActionColor(true);
            }
        }
    }

    public void ChangeSelectedItemColor(bool highlight, bool newSelected)
    {
        if (_debugMode) Debug.Log("InventoryManager.ChangeSelectedItemColor(" + highlight + ", " + newSelected + ")");
        SelectedItem = Items[HighlightNumber];

        ItemController itemController;
        Image itemImage;
        foreach (Transform item in _itemContentCanvas)
        {
            itemController = item.GetComponentInChildren<ItemController>();
            if (itemController != null && itemController.Item.DisplayName == SelectedItem.DisplayName)
            {
                itemImage = item.GetComponentInChildren<Image>();
                if (itemImage != null)
                {
                    if (highlight) itemImage.color = newSelected ? highlightedColor : unselectedColor;
                    else itemImage.color = selectedColor;
                }
            }
        }
    }

    public void ChangeSelectedActionColor(bool newHighlight)
    {
        if (_debugMode) Debug.Log("InventoryManager.ChangeSelectedActionColor(" + newHighlight + ")");
        _itemCanvasActions[ItemActionNumber].color = newHighlight ? highlightedColor : unselectedColor;
    }

    public void BackToSelection()
    {
        if (_debugMode) Debug.Log("Back To Selection - Destroying ItemDisplay UI");
        IsItemActionsOpen = false;
        if (Items.Count > 0) ChangeSelectedItemColor(true, true);
        foreach (Transform child in _itemPreviewCanvas)
        {
            Destroy(child.gameObject);
        }
    }

    public void StatsUpdate()
    {
        _healthBar.fillAmount = _player.Health / 100f;
        _healthText.text = _player.Health.ToString();
        _maxHealthText.text = _player.MaxHealth.ToString();
        _staminaBar.fillAmount = _player.Stamina / 100f;
        _staminaText.text = _player.Stamina.ToString();
        _maxStaminaText.text = _player.MaxStamina.ToString();
        if (_player.IsBleeding) _bleedingIcon.SetActive(true);
        else _bleedingIcon.SetActive(false);
        if (_player.IsPoisoned) _poisonedIcon.SetActive(true);
        else _poisonedIcon.SetActive(false);
        if (_player.IsDizzy) _dizzyIcon.SetActive(true);
        else _dizzyIcon.SetActive(false);
    }

    #endregion




    #region Inspect Item

    public void Inspect(Item item)
    {
        if (_debugMode) Debug.Log("InventoryManager.Inspect(" + item.DisplayName + ") - Set content and display to false and inspector items to true");
        InstantiateItemToInspect();
        IsInspecting = true;
        foreach (Transform child in _itemContentCanvas)
        {
            child.gameObject.SetActive(false);
        }
        foreach (Transform child in _itemPreviewCanvas)
        {
            child.gameObject.SetActive(false);
        }
        _dollDisplayUI.SetActive(false);
        _statsDisplay.SetActive(false);
        _inspectorCamera.SetActive(true);
        _inspector.SetActive(true);
    }

    private void InstantiateItemToInspect()
    {
        if (_debugMode) Debug.Log("InventoryManager.InstantiateItemToInspect() - Instantiating " + SelectedItem.DisplayName + " to inspect");
        _inspectorItem = Instantiate(SelectedItem.Prefab, _inspectItemTransform);
        _inspectorItem.transform.localPosition = new Vector3(0, 0, 0.8f);
        _inspectorItem.transform.localRotation = Quaternion.identity;
        _inspectorItem.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
    }

    public GameObject ReturnInspectorItem()
    {
        return _inspectorItem;
    }

    public void RotateInspectorItem(float x, float y, float scale)
    {
        float zoom = scale * _zoomSensitivity;
        float xRot = -x * _lookSensitivity;
        float yRot = -y * _lookSensitivity;
        if (_inspectorItem != null)
        {
            _inspectorItem.transform.Rotate(Vector3.up, xRot);
            _inspectorItem.transform.Rotate(Vector3.right, yRot);
            if (_inspectorItem.transform.localScale.x + zoom < _minZoomScale)
            {
                _inspectorItem.transform.localScale = new Vector3(_minZoomScale, _minZoomScale, _minZoomScale);
            }
            else if (_inspectorItem.transform.localScale.x + zoom > _maxZoomScale)
            {
                _inspectorItem.transform.localScale = new Vector3(_maxZoomScale, _maxZoomScale, _maxZoomScale);
            }
            else
            {
                _inspectorItem.transform.localScale += new Vector3(zoom, zoom, zoom);
            }
        }
        else Debug.LogError("InventoryManager.RotateInspectorItem() - inspectorItem is null");
    }

    public void ResumeFromInspector()
    {
        if (_debugMode) Debug.Log("InventoryManager.ResumeFromInspector() - Set content and display to true and inspector items to false");
        _inspectorCamera.SetActive(false);
        _inspector.SetActive(false);
        _dollDisplayUI.SetActive(true);
        _statsDisplay.SetActive(true);
        Destroy(_inspectorItem);
        _inspectorItem = null;
        foreach (Transform child in _itemContentCanvas)
        {
            child.gameObject.SetActive(true);
        }
        foreach (Transform child in _itemPreviewCanvas)
        {
            child.gameObject.SetActive(true);
        }
        IsInspecting = false;
    }

    #endregion




    #region Message Management

    public void DisplayMessage(string message, float duration = 2f)
    {
        _messageText.text = message;
        if (!_messageCanvas.activeSelf) _messageCanvas.SetActive(true);
        StartCoroutine(HideMessage(duration, message));
    }

    private IEnumerator HideMessage(float duration, string message)
    {
        yield return new WaitForSeconds(duration);
        // string compare: if message is same as _messageText
        if (string.Compare(message, _messageText.text) == 0)
        {
            _messageCanvas.SetActive(false);
        }
    }

    #endregion




    #region Item Actions

    public void DropItem(Item item)
    {
        if (_debugMode) Debug.Log("InventoryManager.DropItem(" + item.DisplayName + ")");
        GameObject obj = Instantiate(item.Prefab, _player.transform.position + _player.transform.forward, Quaternion.identity, GameManager.Instance.InteractableObjectPool);
        Rigidbody itemRigidbody = obj.AddComponent<Rigidbody>();
        itemRigidbody.AddForce(_player.transform.forward * _dropForce, ForceMode.Impulse);
        StartCoroutine(RemoveRigidbody(obj));

        if(item.Type == ItemType.Weapon)
        {
            Weapon childWeapon = Weapons.GetComponentInChildren<Weapon>();
            childWeapon.IsAvailable = false;

            Weapons.GetComponent<WeaponSwitcher>().SwitchWeapon(true);
        }
        else if(item.Type == ItemType.Ammo)
        {
            for (int i = 0; i < item.Quantity; i++)
            {
                _ammo.ReduceCurrentAmmo(item.AmmoType);
            }
        }
        Items.Remove(item);
        ListItems();
        BackToSelection();
    }

    private IEnumerator RemoveRigidbody(GameObject obj)
    {
        yield return new WaitForSeconds(1f);
        Destroy(obj.GetComponent<Rigidbody>());
    }   

    #endregion
}