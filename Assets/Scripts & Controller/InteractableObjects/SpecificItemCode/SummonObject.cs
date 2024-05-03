using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummonObject : InteractableObject
{
    #region Variables

    [Space(10)]
    [Header("Components")]
    [Tooltip("The item required to summon the object.")]
    [SerializeField] private Item _summoningItem;
    [Tooltip("The red circle that indicates the object is summonable.")]
    [SerializeField] private GameObject _redCircle;
    [Tooltip("The summon event that this object is a part of.")]
    [SerializeField] private SummonEvent _summonEvent;

    [Space(10)]
    [Tooltip("Check if the object has been placed.")]
    public bool IsObjectPlaced = false;

    private GameObject _summonObject;

    #endregion




    #region Unity Methods

    protected override void Start()
    {
        _summonObject = transform.GetChild(0).gameObject;
    }

    #endregion




    #region Base Methods

    // Override the base class method for specific implementation
    protected override void RunItemCode()
    {
        if (IsObjectPlaced) return;

        // Check if the object is available
        Item item = InventoryManager.Instance.FindItem(_summoningItem.DisplayName);

        if (item != null)
        {
            // Set the object as placed
            IsObjectPlaced = true;
            _summonObject.SetActive(true);
            _redCircle.SetActive(false);
            InventoryManager.Instance.RemoveItem(item);
        }
        else
        {
            GameManager.Instance.DisplayMessage("You need \"" + _summoningItem.DisplayName + "\" to proceed with the summoning.", 2f);
        }

        _summonEvent.CheckItems();
    }

    #endregion




    #region Public Methods

    // Load the placed object
    public void LoadPlacedObject()
    {
        IsObjectPlaced = true;
        _summonObject.SetActive(true);
        _redCircle.SetActive(false);
    }

    #endregion
}
