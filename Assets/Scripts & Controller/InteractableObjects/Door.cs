using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : InteractableObject
{
    public override void Interact()
    {
        base.Interact();
        active = !active;
        anim.SetBool("Open", active);
    }
}
