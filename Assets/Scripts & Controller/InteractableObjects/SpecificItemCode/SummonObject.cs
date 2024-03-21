using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummonObject : InteractableObject
{
    [Space(10)]
    [Header("RUN ITEM CODE")]
    [SerializeField] Item summoningItem;
    [SerializeField] SummonEvent summonEvent;

    // Override the base class method for specific implementation
    protected override void RunItemCode()
    {
        Item item = InventoryManager.Instance.FindItem(summoningItem.displayName);

        if (item != null)
        {
            transform.GetChild(0).gameObject.SetActive(true);
            InventoryManager.Instance.RemoveItem(item);
        }       

        summonEvent.CheckItems();
    }
}
