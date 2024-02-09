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
    [Tooltip("Array of body parts of the mannequin for death animation")]
    [SerializeField] Transform[] bodyParts;
    [SerializeField] Transform[] deathParts;
    #endregion

    #region Private Fields
    Transform player;
    NavMeshAgent navMeshAgent;
    Animator animator;
    bool stopped = true; // Flag to track if the mannequin has stopped moving
    bool dead = false;
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
        if (dead) return;
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

    public void Hit(GameObject hitObject)
    {
        if (string.Compare(hitObject.name, "head") == 0)
        {
            dead = true;
            hitObject.GetComponent<Rigidbody>().AddForce(-transform.forward * 5f, ForceMode.Impulse);
            StartCoroutine(Death());
        }
        else
        {
            Debug.Log("Mannequin hit by player: " + hitObject.name);
        }
    }

    IEnumerator Death()
    {
        for (int i = 0; i < bodyParts.Length; i++)
        {
            for (int j = 0; j < deathParts.Length; j++)
            {
                if (string.Compare(bodyParts[i].name, deathParts[j].name) == 0)
                {
                    SwitchBodyPart(bodyParts[i], deathParts[j]);
                    if (string.Compare(deathParts[j].name, "head") == 0) yield return new WaitForSeconds(1f);
                    else yield return new WaitForSeconds(0.15f);
                    break;
                }
            }
        }
        AudioManager.Instance.StopAudio(gameObject.GetInstanceID());
        yield return null;
    }

    private void SwitchBodyPart(Transform bodyPart, Transform deathPart)
    {
        bodyPart.position = deathPart.position;
        bodyPart.rotation = deathPart.rotation;
        deathPart.gameObject.SetActive(true);
        bodyPart.gameObject.SetActive(false);
        if (string.Compare(deathPart.name, "head") == 0)
        {
            deathPart.GetComponent<Rigidbody>().AddForce(-transform.forward * 5f, ForceMode.Impulse);
        }
        else
        {
            // random number between .8 and 1.2
            float randomVolume = Random.Range(0.6f, 1.0f);
            float randomPitch = Random.Range(0.8f, 1.2f);
            AudioManager.Instance.PlaySoundOneShot(gameObject.GetInstanceID(), "wood fall", randomVolume, randomPitch);
        }
    }

    #endregion
}