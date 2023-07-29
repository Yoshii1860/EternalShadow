using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UniqueIDComponent))]
public class InteractableObject : MonoBehaviour
{
    protected Animator anim;
    public bool active = false;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    public virtual void Interact()
    {
        Debug.Log("Interacting with the base interactable object.");
    }
}
