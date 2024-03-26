using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interaction : MonoBehaviour
{
    bool isInteractable = true;

    public void Interact()
    {
        if (isInteractable)
        {
            isInteractable = false;
            OnInteract();
        }
    }

    public virtual void OnInteract()
    {
        Debug.Log("Interaction.OnInteract()");
    }
}
