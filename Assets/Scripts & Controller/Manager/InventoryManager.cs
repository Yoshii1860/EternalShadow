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
        public GameObject inventoryItem;
        [Tooltip("The prefab that will be used to display the inventory item display")]
        public GameObject itemDisplay;
        [Tooltip("The prefab that will be used to display the doll")]
        public GameObject dollDisplay;
        [Tooltip("The camera that will be used to inspect items")]
        public GameObject inspectorCamera;
        [Tooltip("The prefab that will be used to display the item inspector")]
        public GameObject inspector;
        [Tooltip("The transform that will be used to parent the item to inspect")]
        public Transform inspectItemTransform;
        [Tooltip("The canvas that will be used to display the status of the player")]
        public GameObject statsDisplay;
        [Tooltip("The fill amount of the health bar")]
        public Image healthBar;
        [Tooltip("The fill amount of the stamina bar")]
        public Image staminaBar;
        [Tooltip("The icons that will be used to display status effects")]
        public GameObject bleedingIcon;
        [Tooltip("The icons that will be used to display status effects")]
        public GameObject poisonedIcon;
        [Tooltip("The icons that will be used to display status effects")]
        public GameObject dizzyIcon;
        [Tooltip("The canvas that will be used to display messages")]
        public GameObject messageCanvas;
        [Tooltip("The text that will be used to display messages")]
        public TextMeshProUGUI messageText;
        [Tooltip("The text that will be used to display the amount of horror dolls")]
        public TextMeshProUGUI horrorDollCount;
        [Tooltip("The force that will be applied to the dropped item")]
        public float dropForce;
        [Tooltip("The maximum amount of items that can be stored in the inventory")]
        public int maxItems = 9;
        [Tooltip("The sensitivity of the zoom when inspecting items")]
        public float zoomSensitive = 0.01f;
        [Tooltip("The sensitivity of the look when inspecting items")]
        public float lookSensitivity = 0.5f;
        [Tooltip("The maximum zoom scale when inspecting items")]
        public float maxZoomScale = 4f;
        [Tooltip("The minimum zoom scale when inspecting items")]
        public float minZoomScale = 0.5f;


        // Additional fields
        Transform[] gridLayouts;
        public Transform itemContent;
        public Transform itemPreview;
        public GameObject weapons;
        GameObject player;
        Ammo ammo;
        GameObject inspectorItem;
        TextMeshProUGUI healthText;
        TextMeshProUGUI maxHealthText;
        TextMeshProUGUI staminaText;
        TextMeshProUGUI maxStaminaText;

        [Space(10)]
        [Header("Colors")]
        [Tooltip("The color of the selected item")]
        public Color selectedColor;
        [Tooltip("The color of the unselected item")]
        public Color unselectedColor;
        [Tooltip("The color of the highlighted item")]
        public Color highlightedColor;

        [Space(10)]
        [Header("Only for Debugging)")]
        public Item selectedItem;
        public int highlightNumber;
        public int selectedActionNumber;
        public int itemActionNumber;
        public bool itemActionsOpen;
        public int actionsChildCount;
        public bool isInspecting = false;

        // names of items need to meet the conditions to autosave the game before boss fight
        [SerializeField] AutoSave autoSave;
        public int uniqueAutoSaveID;
        public string[] autoSaveCondition;
        public int conditionCounter = 0;

        // The Image components of the item actions
        Image[] itemCanvasActions;

        #endregion

    #region Initialization

    void Start()
    {
        InitializeReferences();
    }

    void InitializeReferences()
    {
        gridLayouts = FindGridLayoutGroups();
        itemContent = gridLayouts[1];
        itemPreview = gridLayouts[2];
        healthText = healthBar.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        maxHealthText = healthBar.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        staminaText = staminaBar.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        maxStaminaText = staminaBar.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
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

    #endregion

    #region Update References

    public void UpdateReferences()
    {
        player = GameManager.Instance.player.gameObject;
        weapons = FindObjectOfType<WeaponSwitcher>().gameObject;
        ammo = player.GetComponent<Ammo>();
        if (autoSave == null)
        {
            GameObject autoSavePool = GameObject.FindWithTag("SaveGamePool");
            foreach (AutoSave autoSaveObj in autoSavePool.GetComponentsInChildren<AutoSave>())
            {
                if (autoSaveObj.GetComponent<UniqueIDComponent>().UniqueID == uniqueAutoSaveID.ToString())
                {
                    autoSave = autoSaveObj;
                    break;
                }
            }
        }

        GetComponent<ItemActions>().UpdateReferences();
        GetComponent<ObjectHandler>().UpdateReferences();
        GetComponent<Potion>().UpdateReferences();
    }

    #endregion

    #region AutoSaveCondition

    void AutoSaveCondition(Item item)
    {
        foreach (string condition in autoSaveCondition)
        {
            if (item.name == condition)
            {
                conditionCounter++;
                break;
            }
        }

        if (conditionCounter == autoSaveCondition.Length)
        {
            autoSave.conditionMet = true;
        }
    }

    #endregion

    #region Item Management

    public void AddItem(Item item)
    {
        AutoSaveCondition(item);

        if (item.type == ItemType.Ammo || item.type == ItemType.Potion)
        {
            Item existingItem = Items.Find(existingItem => existingItem.displayName == item.displayName);
            if (existingItem != null)
            {
                Debug.Log("InventoryManager - Add Existing Ammo/Potion (" + item.displayName + ")");
                // Increase quantity of existing item by quantity of added item;
                existingItem.quantity += item.quantity;

                if (item.type == ItemType.Ammo)
                {
                    // INCREASE AMMO SLOT
                    ammo.IncreaseCurrentAmmo(item.AmmoType, item.quantity);

                    // SET UI FOR BULLETS
                    foreach(Transform weapon in weapons.transform)
                    {
                        if (weapon.GetComponent<Weapon>().ammoType == existingItem.AmmoType &&
                            weapon.GetComponent<Weapon>().gameObject.activeSelf == true)
                        {
                            Weapon weaponScript = weapon.GetComponent<Weapon>();
                            player.GetComponent<Player>().SetBulletsUI(weaponScript.magazineCount, existingItem.quantity);
                            break;
                        }
                    }
                }

                return;
            }
            else
            {
                Debug.Log("InventoryManager - Add New Ammo/Potion (" + item.displayName + ")");

                Item newItem = Instantiate(item);
                Items.Add(newItem);

                // If the item is not unique, create a copy to not change the original
                if (item.type == ItemType.Ammo)
                {
                    // Increase the current ammo amount of the player
                    ammo.IncreaseCurrentAmmo(item.AmmoType, item.quantity);

                    // SET UI FOR BULLETS
                    foreach(Transform weapon in weapons.transform)
                    {
                        if (weapon.GetComponent<Weapon>().ammoType == item.AmmoType)
                        {
                            Weapon weaponScript = weapon.GetComponent<Weapon>();
                            int inventoryAmmoAmount = GetInventoryAmmo(weaponScript.ammoType);
                            player.GetComponent<Player>().SetBulletsUI(weaponScript.magazineCount, inventoryAmmoAmount);
                            break;
                        }
                    }

                }
                return;
            }
        }
        else if (item.type == ItemType.Weapon)
        {
            Debug.Log("InventoryManager - Add New Weapon (" + item.displayName + ")");

            Items.Add(item);
            foreach (Transform weapon in weapons.transform)
            {                    
                if (weapon.GetComponent<ItemController>() != null && weapon.GetComponent<ItemController>().item.displayName == item.displayName)
                {
                    Weapon weaponScript = weapon.GetComponent<Weapon>();
                    weaponScript.isAvailable = true;

                    // SET UI FOR BULLETS
                    int inventoryAmmo = GetInventoryAmmo(weaponScript.ammoType);
                    player.GetComponent<Player>().SetBulletsUI(weaponScript.magazineCount, inventoryAmmo);

                    ammo.IncreaseCurrentAmmo(weaponScript.ammoType, weaponScript.magazineCount);
                    while (weapon.gameObject.activeSelf == false)
                    {
                        weapons.GetComponent<WeaponSwitcher>().SwitchWeapon(true);
                    }
                    break;
                }
            }
        }
        else
        {
            Items.Add(item);
        }
    }

    public void RemoveItem(Item item)
    {
        if (Items.Contains(item))
        {
            if (Items[Items.IndexOf(item)].quantity > 1)
            {
                Items[Items.IndexOf(item)].quantity -= 1;
                BackToSelection();
                ListItems();
            }
            else
            {
                Items.Remove(item);
                BackToSelection();
                ListItems();
            }
        }
    }

    public void RemoveAmmo(Ammo.AmmoType ammotype, int amount)
    {
        Debug.Log("InventoryManager.RemoveAmmo(" + ammotype + ", " + amount + ")");
        foreach (Item item in Items)
        {
            if (item.AmmoType == ammotype)
            {
                if (item.quantity > amount)
                {
                    item.quantity -= amount;
                    break;
                }
                else if (item.quantity == amount)
                {
                    Items.Remove(item);
                    break;
                }
                else
                {
                    Debug.Log("InventoryManager.RemoveAmmo() - FAILED");
                }
            }
        }
    }

    public Item FindItem(string itemDisplayName)
    {
        foreach (Item inventoryItem in Items)
        {
            if (inventoryItem.displayName == itemDisplayName)
            {
                return inventoryItem;
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
                return item.quantity;
            }
        }
        return 0;
    }

    #endregion

    #region UI Handling

    public void ListItems()
    {
        // Clear list before showing items
        Debug.Log("InventoryManager.ListItems() - Clearing itemContent and itemPreview");
        foreach (Transform child in itemContent)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in itemPreview)
        {
            Destroy(child.gameObject);
        }

        // Show items from inventory list in UI
        Debug.Log("InventoryManager.ListItems() - Showing " + Items.Count + " items in itemContent");
        foreach (Item item in Items)
        {
            GameObject obj = Instantiate(inventoryItem, itemContent);
            Image[] itemCanvasIcons = obj.GetComponentsInChildren<Image>();
            TextMeshProUGUI itemCanvasText = obj.GetComponentInChildren<TextMeshProUGUI>();

            if (item.unique && item.type != ItemType.Weapon)
            {
                // If the item is unique, hide the quantity
                itemCanvasText.enabled = false;
            }
            else if (item.type == ItemType.Weapon)
            {
                // If item is weapon, show current ammo in magazine
                // loop through weapons to find the weapon with the same name as the item
                foreach (Transform weapon in weapons.transform)
                {
                    if (weapon.GetComponent<ItemController>().item.displayName == item.displayName)
                    {   
                        Weapon weaponScript = weapon.GetComponent<Weapon>();
                        if (weaponScript.ammoType == Ammo.AmmoType.Infinite)
                        {
                            itemCanvasText.enabled = false;
                            break;
                        }
                        
                        // If it is, get the weapon stats and display them
                        string weaponAmmo = weaponScript.magazineCount.ToString();
                        itemCanvasText.text = weaponAmmo;
                        break;
                    }
                }
            }
            else
            {
                // If the item is not unique, show the quantity
                itemCanvasText.text = item.quantity.ToString();
            }

            // Iterate through the Image components and find the desired one
            foreach (Image itemCanvasIcon in itemCanvasIcons)
            {
                if (itemCanvasIcon.gameObject != obj)
                {
                    itemCanvasIcon.sprite = item.icon;
                    break;
                }
            }
            
            obj.GetComponentInChildren<ItemController>().item = item;
        }

        // check if Items list is empty
        if (Items.Count > 0)
        {
            highlightNumber = 0;
            selectedItem = Items[highlightNumber];
            ChangeSelectedItemColor(true, true);
        }
    }

    public void ShowItemDisplay()
    {
        if (Items.Count == 0) return;
        selectedItem = Items[highlightNumber];
        Debug.Log("InventoryManager.ShowItemDisplay() for" + selectedItem.displayName);
        ChangeSelectedItemColor(false, false);

        itemActionNumber = 0;
        itemActionsOpen = true;

        GameObject obj;
        // Instantiate ItemDisplay UI if it doesn't exist
        if (itemPreview.childCount == 0)
        {
            obj = Instantiate(itemDisplay, itemPreview);
            Debug.Log("InventoryManager.ShowItemDisplay() - Instantiated ItemDisplay: " + obj.name);
        }
        else
        {
            // Clear the ItemDisplay UI
            foreach (Transform child in itemPreview)
            {
                Destroy(child.gameObject);
            }
            // Instantiate ItemDisplay UI
            obj = Instantiate(itemDisplay, itemPreview);
            Debug.Log("InventoryManager.ShowItemDisplay() - Instantiated ItemDisplay: " + obj.name);
        }

        // Change icon of obj to item.icon
        Image[] itemCanvasIcons = obj.GetComponentsInChildren<Image>();
        Debug.Log("InventoryManager.ShowItemDisplay() - Changing icon to " + selectedItem.icon.name);
        // Iterate through the Image components and find the desired one
        foreach (Image itemCanvasIcon in itemCanvasIcons)
        {
            if (itemCanvasIcon.gameObject != obj)
            {
                itemCanvasIcon.sprite = selectedItem.icon;
                break;
            }
        }

        // Get all TextMeshProUGUI components
        TextMeshProUGUI[] itemCanvasTexts = obj.GetComponentsInChildren<TextMeshProUGUI>();
        Debug.Log("InventoryManager.ShowItemDisplay() - Changing text to " + selectedItem.displayName);
        foreach (TextMeshProUGUI itemCanvasText in itemCanvasTexts)
        {
            // Change name and description of ItemDisplay
            if (itemCanvasText.gameObject.name == "ItemName")
            {
                itemCanvasText.text = selectedItem.displayName;
            }
            else if (itemCanvasText.gameObject.name == "ItemDescription")
            {
                itemCanvasText.text = selectedItem.description;
            }
            // If item is unique we hide the quantity display
            else if (itemCanvasText.gameObject.name == "ItemCountText" && selectedItem.unique)
            {
                itemCanvasText.gameObject.SetActive(false);
            }
            // If item is not unique, get its child object and change the quantity
            else if (itemCanvasText.gameObject.name == "ItemCountText" && !selectedItem.unique)
            {
                itemCanvasText.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = selectedItem.quantity.ToString();
            }
            // If item is not a weapon, hide the stats display
            if (itemCanvasText.gameObject.name == "ItemStatsText" && selectedItem.type != ItemType.Weapon)
            {
                itemCanvasText.gameObject.SetActive(false);
            }
            else if (itemCanvasText.gameObject.name == "ItemStatsText" && selectedItem.type == ItemType.Weapon)
            {
                // loop through all the childs of weapons, check if item is the same item as the one displayed
                foreach (Transform weapon in weapons.transform)
                {
                    if (weapon.GetComponent<ItemController>().item.displayName == selectedItem.displayName)
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
                itemCanvasActions = child.GetComponentsInChildren<Image>();
                actionsChildCount = child.childCount;
                ChangeSelectedActionColor(true);
            }
        }
    }

    public void ChangeSelectedItemColor(bool highlight, bool newSelected)
    {
        Debug.Log("InventoryManager.ChangeSelectedItemColor(" + highlight + ", " + newSelected + ")");
        selectedItem = Items[highlightNumber];

        ItemController itemController;
        Image itemImage;
        foreach (Transform item in itemContent)
        {
            itemController = item.GetComponentInChildren<ItemController>();
            if (itemController != null && itemController.item.displayName == selectedItem.displayName)
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
        Debug.Log("InventoryManager.ChangeSelectedActionColor(" + newHighlight + ")");
        itemCanvasActions[itemActionNumber].color = newHighlight ? highlightedColor : unselectedColor;
    }

    public void BackToSelection()
    {
        Debug.Log("Back To Selection - Destroying ItemDisplay UI");
        itemActionsOpen = false;
        if (Items.Count > 0) ChangeSelectedItemColor(true, true);
        foreach (Transform child in itemPreview)
        {
            Destroy(child.gameObject);
        }
    }

    public void Inspect(Item item)
    {
        Debug.Log("InventoryManager.Inspect(" + item.displayName + ") - Set content and display to false and inspector items to true");
        InstantiateItemToInspect();
        isInspecting = true;
        foreach (Transform child in itemContent)
        {
            child.gameObject.SetActive(false);
        }
        foreach (Transform child in itemPreview)
        {
            child.gameObject.SetActive(false);
        }
        dollDisplay.SetActive(false);
        statsDisplay.SetActive(false);
        inspectorCamera.SetActive(true);
        inspector.SetActive(true);
    }

    void InstantiateItemToInspect()
    {
        Debug.Log("InventoryManager.InstantiateItemToInspect() - Instantiating " + selectedItem.displayName + " to inspect");
        inspectorItem = Instantiate(selectedItem.prefab, inspectItemTransform);
        inspectorItem.transform.localPosition = new Vector3(0, 0, 0.8f);
        inspectorItem.transform.localRotation = Quaternion.identity;
        inspectorItem.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
    }

    public GameObject ReturnInspectorItem()
    {
        return inspectorItem;
    }

    public void RotateInspectorItem(float x, float y, float scale)
    {
        float zoom = scale * zoomSensitive;
        float xRot = -x * lookSensitivity;
        float yRot = -y * lookSensitivity;
        if (inspectorItem != null)
        {
            inspectorItem.transform.Rotate(Vector3.up, xRot);
            inspectorItem.transform.Rotate(Vector3.right, yRot);
            if (inspectorItem.transform.localScale.x + zoom < minZoomScale)
            {
                inspectorItem.transform.localScale = new Vector3(minZoomScale, minZoomScale, minZoomScale);
            }
            else if (inspectorItem.transform.localScale.x + zoom > maxZoomScale)
            {
                inspectorItem.transform.localScale = new Vector3(maxZoomScale, maxZoomScale, maxZoomScale);
            }
            else
            {
                inspectorItem.transform.localScale += new Vector3(zoom, zoom, zoom);
            }
        }
        else Debug.LogError("InventoryManager.RotateInspectorItem() - inspectorItem is null");
    }

    public void ResumeFromInspector()
    {
        Debug.Log("InventoryManager.ResumeFromInspector() - Set content and display to true and inspector items to false");
        inspectorCamera.SetActive(false);
        inspector.SetActive(false);
        dollDisplay.SetActive(true);
        statsDisplay.SetActive(true);
        Destroy(inspectorItem);
        inspectorItem = null;
        foreach (Transform child in itemContent)
        {
            child.gameObject.SetActive(true);
        }
        foreach (Transform child in itemPreview)
        {
            child.gameObject.SetActive(true);
        }
        isInspecting = false;
    }

    public void StatsUpdate()
    {
        healthBar.fillAmount = GameManager.Instance.player.health / 100f;
        healthText.text = GameManager.Instance.player.health.ToString();
        maxHealthText.text = GameManager.Instance.player.maxHealth.ToString();
        staminaBar.fillAmount = GameManager.Instance.player.stamina / 100f;
        staminaText.text = GameManager.Instance.player.stamina.ToString();
        maxStaminaText.text = GameManager.Instance.player.maxStamina.ToString();
        if (GameManager.Instance.player.isBleeding) bleedingIcon.SetActive(true);
        else bleedingIcon.SetActive(false);
        if (GameManager.Instance.player.isPoisoned) poisonedIcon.SetActive(true);
        else poisonedIcon.SetActive(false);
        if (GameManager.Instance.player.isDizzy) dizzyIcon.SetActive(true);
        else dizzyIcon.SetActive(false);
    }

    #endregion

    #region Message Management

    public void DisplayMessage(string message, float duration = 2f)
    {
        messageText.text = message;
        if (!messageCanvas.activeSelf) messageCanvas.SetActive(true);
        StartCoroutine(HideMessage(duration, message));
    }

    IEnumerator HideMessage(float duration, string message)
    {
        yield return new WaitForSeconds(duration);
        // string compare: if message is same as messageText
        if (string.Compare(message, messageText.text) == 0)
        {
            messageCanvas.SetActive(false);
        }
    }

    #endregion

    #region Item Actions

    public void DropItem(Item item)
    {
        Debug.Log("InventoryManager.DropItem(" + item.displayName + ")");
        GameObject obj = Instantiate(item.prefab, player.transform.position + player.transform.forward, Quaternion.identity, GameManager.Instance.interactableObjectPool);
        Rigidbody itemRigidbody = obj.AddComponent<Rigidbody>();
        itemRigidbody.AddForce(player.transform.forward * dropForce, ForceMode.Impulse);
        StartCoroutine(RemoveRigidbody(obj));

        if(item.type == ItemType.Weapon)
        {
            Weapon childWeapon = weapons.GetComponentInChildren<Weapon>();
            childWeapon.isAvailable = false;

            weapons.GetComponent<WeaponSwitcher>().SwitchWeapon(true);
        }
        else if(item.type == ItemType.Ammo)
        {
            for (int i = 0; i < item.quantity; i++)
            {
                ammo.ReduceCurrentAmmo(item.AmmoType);
            }
        }
        Items.Remove(item);
        ListItems();
        BackToSelection();
    }

    IEnumerator RemoveRigidbody(GameObject obj)
    {
        yield return new WaitForSeconds(1f);
        Destroy(obj.GetComponent<Rigidbody>());
    }   

    public int ItemCount()
    {
        if (Items.Count >= maxItems)
        {
            GameManager.Instance.DisplayMessage("Inventory is full", 2f);
            // play an audio
            return Items.Count;
        }
        return Items.Count;
    }

    #endregion
}