using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashlightCode : InteractableObject
{
    // Override the base class method for specific implementation
    protected override void RunItemCode()
    {
        GameManager.Instance.player.lightAvail = true;
        GameManager.Instance.player.LightSwitch();

        // Implement the specific behavior for this object
        Destroy(gameObject);
    }
}
