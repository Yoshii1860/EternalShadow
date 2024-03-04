using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BehaviorTree;

public class S_ActionDoor : Node
{
    /*
    #region Fields

    // References to components
    private Animator animator;
    private Transform transform;
    private NavMeshAgent agent;
    private float timer;

    private enum DoorState { None, Crouching, Walking, Uncrouching, Continue };
    private DoorState doorState;

    #endregion

    #region Constructors

    // Constructor to initialize references
    public S_ActionDoor(Transform transform, NavMeshAgent agent)
    {
        this.transform = transform;
        this.agent = agent;
        animator = transform.GetComponent<Animator>();
    }

    #endregion

    #region Public Methods

    // Evaluate method to determine the state of the node
    public override NodeState Evaluate()
    {
        // Check if the game is paused
        if (GameManager.Instance.isPaused)
        {
            // Return FAILURE to indicate that the action cannot be executed
            return NodeState.FAILURE;
        }

        if (GetData("doorState") == null) 
        {
            doorState = DoorState.None;

            parent.parent.SetData("destination", agent.destination);
            parent.parent.SetData("speed", agent.speed);
            agent.isStopped = true;
            agent.speed = 0f;
            agent.ResetPath();
            agent.SetDestination(transform.position);
            agent.enabled = false;
            animator.SetTrigger("crouch");
            // wait for one second in realtime
            timer = 1;
            while (timer > 0)
            {
                timer -= Time.deltaTime;
            }
        }
        else 
        {
            doorState = (DoorState)GetData("doorState");
        }

        switch (doorState)
        {
            case DoorState.None:
                // Store initial destination and speed
                parent.parent.SetData("doorState", DoorState.Walking);
                break;

            case DoorState.Walking:
                animator.SetTrigger("crouch walk");
                agent.enabled = true;
                agent.isStopped = false;
                agent.speed = SlenderBT.crouchSpeed;
                agent.SetDestination((Vector3)GetData("destination"));
                
                if (transform.GetComponent<Enemy>().enteredDoor == false)
                {
                    agent.isStopped = true;
                    agent.speed = 0f;
                    agent.ResetPath();
                    agent.SetDestination(transform.position);
                    agent.enabled = false;
                    parent.parent.SetData("doorState", DoorState.Uncrouching);
                    animator.SetTrigger("uncrouch");
                }
                else Debug.Log("WAITING FOR enteredDoor TO BE FALSE!");
                break;

            case DoorState.Uncrouching:
                // wait for one second in realtime
                timer = 1;
                while (timer > 0)
                {
                    timer -= Time.deltaTime;
                }
                parent.parent.SetData("doorState", DoorState.Continue);
                break;

            case DoorState.Continue:
                agent.enabled = true;
                agent.isStopped = false;
                float speed = (float)GetData("speed");
                agent.speed = speed;
                agent.SetDestination((Vector3)GetData("destination"));
                ClearData("destination");
                ClearData("speed");
                ClearData("doorState");
                ClearData("enteredDoor");
                return NodeState.SUCCESS;
        }

        return NodeState.RUNNING;


        /*parent.parent.SetData("destination", agent.destination);
        parent.parent.SetData("speed", agent.speed);
        agent.isStopped = true;
        agent.ResetPath();
        agent.SetDestination(transform.position);
        animator.SetTrigger("crouch");

        timer = 5;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
        }

        agent.speed = SlenderBT.crouchSpeed;
        agent.isStopped = false;
        agent.ResetPath();
        agent.SetDestination((Vector3)GetData("destination"));

        timer = 2;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
        }

        agent.isStopped = true;
        agent.SetDestination(transform.position);
        animator.SetTrigger("uncrouch");

        timer = 0.5f;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
        }

        float speed = (float)GetData("speed");
        agent.speed = speed;

        agent.isStopped = false;
        agent.SetDestination((Vector3)GetData("destination"));
        ClearData("destination");
        ClearData("speed");
        ClearData("enteredDoor");
        return NodeState.SUCCESS;
    }

    #endregion */
}