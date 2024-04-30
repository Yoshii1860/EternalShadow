using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashlightCode : InteractableObject
{
    // Override the base class method for specific implementation
    protected override void RunItemCode()
    {
        GameManager.Instance.Player.IsLightAvailable = true;
        GameManager.Instance.Player.LightSwitch();
    }
}
