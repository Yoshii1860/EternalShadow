using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class S_DecisionDoorSensing : Node
{
    /*
    #region Fields

    // References to components
    Transform transform;
    Animator animator;

    #endregion

    #region Constructors

    // Constructor to initialize references to components
    public S_DecisionDoorSensing(Transform transform)
    {
        this.transform = transform;
        animator = transform.GetComponent<Animator>();
    }

    #endregion

    #region Public Methods

    // Evaluate method to determine the state of the node
    public override NodeState Evaluate()
    {
        
        if (GetData("enteredDoor") == null)
        {
            parent.parent.SetData("enteredDoor", false);
        }
        else if (transform.GetComponent<Enemy>().enteredDoor == true)
        {
            if ((bool)GetData("enteredDoor") == false)
            {
                parent.parent.SetData("enteredDoor", transform.GetComponent<Enemy>().enteredDoor);
            }
        }

        if ((bool)GetData("enteredDoor"))
        {
            // Set state to SUCCESS and return
            state = NodeState.SUCCESS;
            return state;
        }
        else
        {
            // Set state to FAILURE and return
            state = NodeState.FAILURE;
            return state;
        }

        // Check if there's a door nearby (replace "Door" with your actual tag/layer)
        Collider[] colliders = Physics.OverlapSphere(transform.position, 1.0f, LayerMask.GetMask("Interact"));
        for (int i = 0; i < colliders.Length; i++)
        {
            // Check if the door is locked
            Door door = colliders[i].GetComponent<Door>();
            if (door != null)
            {
                Debug.Log("DecisionDoorSensing: Door Found");
                // Set state to SUCCESS and return
                state = NodeState.SUCCESS;
                return state;
            }
        }
        return NodeState.FAILURE;
    }

    #endregion 
    */
}