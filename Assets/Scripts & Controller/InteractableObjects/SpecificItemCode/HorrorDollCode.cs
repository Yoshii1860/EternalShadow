using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HorrorDollCode : InteractableObject
{
    // Override the base class method for specific implementation
    protected override void RunItemCode()
    {
        // Convert the text to an integer, add 1, then convert back to string
        InventoryManager.Instance.horrorDollCount.text = (int.Parse(InventoryManager.Instance.horrorDollCount.text) + 1).ToString();
        Debug.Log("Horror Doll picked up");
    }
}
