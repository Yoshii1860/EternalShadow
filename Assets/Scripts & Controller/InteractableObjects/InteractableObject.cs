using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    protected Animator anim;
    public bool active = false;
    public int uniqueID;

    void Awake()
    {
        uniqueID = GetInstanceID();
    }

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    public virtual void Interact()
    {
        Debug.Log("Interacting with the base interactable object.");
    }
}
