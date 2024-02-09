using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Mannequin : MonoBehaviour, ICustomUpdatable
{
    #region Serialized Fields
    [Header("Mannequin Settings")]
    [Tooltip("Reference to the head of the mannequin")]
    [SerializeField] Transform head;
    [Tooltip("Speed at which the head rotates to face the player")]
    [SerializeField] float headRotationSpeed = 1f;
    #endregion

    #region Private Fields
    Transform player;
    NavMeshAgent navMeshAgent;
    Animator animator;
    bool stopped = true; // Flag to track if the mannequin has stopped moving
    #endregion

    #region MonoBehaviour Callbacks
    void Start()
    {
        // Initialize references
        player = GameManager.Instance.player.transform;
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // Play audio and pause it instantly to use Pause/Unpause methods later
        AudioManager.Instance.PlayAudio(gameObject.GetInstanceID(), 1f, 1f, true); // Play audio for the mannequin
        AudioManager.Instance.PauseAudio(gameObject.GetInstanceID()); // Pause audio for the mannequin
    }

    public void CustomUpdate(float deltaTime)
    {
        // Update mannequin behaviors
        TurnHead(); // Rotate the mannequin's head to face the player
        MoveMannequin(); // Move the mannequin based on player's visibility
    }
    #endregion

    #region Behavior Methods
    private void TurnHead()
    {
        // Calculate direction to the player
        Vector3 directionToPlayer = (player.position - head.position).normalized;

        // Ignore vertical component (Y-axis)
        directionToPlayer.y = 0f;

        // Rotate the head towards the player
        if (directionToPlayer != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            head.rotation = Quaternion.Slerp(head.rotation, targetRotation, headRotationSpeed * Time.deltaTime);
        }
    }

    private void MoveMannequin()
    {
        // Calculate the angle between the player and the mannequin
        float angle = Vector3.Angle(player.position - head.position, player.forward);

        // Calculate the position of the player's head
        float yOffset = 1.5f;
        Vector3 playerHead = player.position + new Vector3(0f, yOffset, 0f);

        // Check if the player is visible by performing a linecast from the mannequin's head to the player's head
        // Ignore the "Characters" and "Enemies" layers
        int layerMask = ~((1 << LayerMask.NameToLayer("Characters")) | (1 << LayerMask.NameToLayer("Enemies")));
        bool isPlayerVisible = Physics.Linecast(head.position, playerHead, layerMask);

        // If the mannequin is outside the player's field of view or the player is not visible to the mannequin, 
        // the mannequin starts to move
        if (angle < 90f || isPlayerVisible)
        {
            ResumeAnimation(); // Resume the mannequin's movement
            Debug.DrawLine(head.position, playerHead, Color.green); // Draw a debug line to visualize player visibility
        }
        else
        {
            StopAnimation(); // Stop the mannequin's movement
            Debug.DrawLine(head.position, playerHead, Color.red); // Draw a debug line to visualize player visibility
        }
    }

    private void StopAnimation()
    {
        // Stop the mannequin from moving
        if (stopped) return;
        AudioManager.Instance.PauseAudio(gameObject.GetInstanceID()); // Pause audio for the mannequin
        navMeshAgent.SetDestination(transform.position); // Set destination for NavMeshAgent to current position
        animator.speed = 0; // Set animator speed to 0 to stop animation
        stopped = true; // Set stopped flag to true to indicate the mannequin has stopped
    }

    private void ResumeAnimation()
    {
        // Resume the mannequin's movement
        if (!stopped) return;
        animator.SetTrigger("Walk"); // Trigger the "Walk" animation in the animator
        AudioManager.Instance.UnpauseAudio(gameObject.GetInstanceID()); // Unpause audio for the mannequin
        navMeshAgent.SetDestination(player.position); // Set destination for NavMeshAgent to player's position
        animator.speed = 1; // Set animator speed to 1 to resume animation
        stopped = false; // Set stopped flag to false to indicate the mannequin has resumed movement
    }
    #endregion
}