using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HorrorDollCode : InteractableObject
{
    // Can be found on the Canvas for the inventory
    [SerializeField] TextMeshProUGUI itemCount;

    // Override the base class method for specific implementation
    protected override void RunItemCode()
    {
        // Convert the text to an integer, add 1, then convert back to string
        itemCount.text = (int.Parse(itemCount.text) + 1).ToString();
    }
}
