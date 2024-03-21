using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpareKey : MonoBehaviour
{
    void OnEnable()
    {
        Item item = GetComponent<ItemController>().item;
        item = InventoryManager.Instance.FindItem(item.displayName);
        if (item != null)
        {
            Destroy(gameObject);
        }
    }
}
