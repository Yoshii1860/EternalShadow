using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(UniqueIDComponent))]
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
    [Tooltip("Maximum distance at which the mannequin can follow the player")]
    [SerializeField] float maxDistance = 12f;
    #endregion

    #region Private Fields
    Transform player;
    NavMeshAgent navMeshAgent;
    Animator animator;
    bool stopped = true; // Flag to track if the mannequin has stopped moving
    public bool isDead = false;
    public bool move = false;
    public bool started = false;
    bool firstAudio = false;
    float waitTimer = 0f;
    bool stuckCheckActive = false;
    #endregion

    #region MonoBehaviour Callbacks
    void Start()
    {
        // Initialize references
        player = GameObject.FindGameObjectWithTag("Player").transform;
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    public void CustomUpdate(float deltaTime)
    {
        if (GameManager.Instance.isPaused) 
        {
            StopAnimation();
            return;
        }
        if (isDead) return;
        // Update mannequin behaviors
        if (!started) return;
        TurnHead(); // Rotate the mannequin's head to face the player
        if (!move) return;
        MoveMannequin(); // Move the mannequin based on player's visibility
        CheckDoor();
        if (waitTimer > 0f)
        {
            waitTimer -= deltaTime;
            if (waitTimer <= 0f)
            {
                waitTimer = 0f;
            }
        }
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
        float distance = Vector3.Distance(player.position, head.position);

        // If the mannequin is outside the player's field of view or the player is not visible to the mannequin, 
        // the mannequin starts to move
        if (angle < 90f && distance < maxDistance || isPlayerVisible && distance < maxDistance)
        {
            ResumeAnimation(); // Resume the mannequin's movement
            if (waitTimer == 0) Attack(); // Attack the player if close enough
            Debug.DrawLine(head.position, playerHead, Color.green); // Draw a debug line to visualize player visibility
            if (!stuckCheckActive) StartCoroutine(StuckCheck());
        }
        else
        {
            StopAnimation(); // Stop the mannequin's movement
            Debug.DrawLine(head.position, playerHead, Color.red); // Draw a debug line to visualize player visibility
            if (stuckCheckActive) 
            {
                StopCoroutine(StuckCheck());
                stuckCheckActive = false;
            }
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
        if (!firstAudio) 
        {
            AudioManager.Instance.PlayAudio(gameObject.GetInstanceID(), 1f, 1f, true); // Play audio for the mannequin
            firstAudio = true;
        }
        else AudioManager.Instance.UnpauseAudio(gameObject.GetInstanceID()); // Unpause audio for the mannequin
        navMeshAgent.SetDestination(player.position); // Set destination for NavMeshAgent to player's position
        animator.speed = 1; // Set animator speed to 1 to resume animation
        stopped = false; // Set stopped flag to false to indicate the mannequin has resumed movement
    }

    private void CheckDoor()
    {
        // Check with a sphere if the mannequin is close to a door
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 2f, LayerMask.GetMask("Interact"));
        // make it visible for debugging
        if (hitColliders.Length > 0)
        {
            for (int i = 0; i < hitColliders.Length; i++)
            {
                Door door = hitColliders[i].GetComponent<Door>();
                if (door != null)
                {
                    if (!door.DoorState() && !door.locked)
                    {
                        door.OpenDoor();
                    }
                }
            }
        }
    }

    private void Attack()
    {
        // Check if mannequin is close enough to attack
        float distance = Vector3.Distance(player.position, head.position);

        if (distance < 2f)
        {
            waitTimer = 2f;
            StartCoroutine(AttackHit());
        }
    }

    public void Hit(GameObject hitObject)
    {
        if (string.Compare(hitObject.name, "head") == 0)
        {
            isDead = true;
            hitObject.GetComponent<Rigidbody>().AddForce(-transform.forward * 5f, ForceMode.Impulse);
            AudioManager.Instance.SetAudioClip(gameObject.GetInstanceID(), "mannequin death", 1f, 1f, false);

            StartCoroutine(Death());
        }
        else
        {
            Debug.Log("Mannequin hit by player: " + hitObject.name);
        }
    }

    IEnumerator StuckCheck()
    {
        stuckCheckActive = true;
        if (navMeshAgent.velocity.magnitude < 0.1f)
        {
            float timer = 3f;
            while (timer > 0)
            {
                timer -= Time.deltaTime;

                if (navMeshAgent.velocity.magnitude > 0.1f)
                {
                    stuckCheckActive = false;
                    yield break;
                }

                yield return null;
            }
            Debug.Log("Mannequin is stuck!");
            StopAnimation();
        }
        stuckCheckActive = false;
    }

    IEnumerator AttackHit()
    {
        AudioManager.Instance.PlaySoundOneShot(AudioManager.Instance.playerSpeaker2, "mannequin hit", 1f); // Play attack sound
        float randomizer = Random.Range(15, 33f);
        GameManager.Instance.player.TakeDamage(randomizer);
        randomizer = Random.Range(0,100);
        if (randomizer <= 15)
        {
            if (!GameManager.Instance.player.isBleeding) GameManager.Instance.player.Bleeding();
        }
        yield return new WaitForSeconds(0.5f);
        int randomNum = Random.Range(1, 5);
        float randomVolume = Random.Range(0.6f, 1.0f);
        string painSound = "pain" + randomNum.ToString();
        AudioManager.Instance.PlaySoundOneShot(AudioManager.Instance.playerSpeaker2, painSound, randomVolume); // Play pain sound
    }

    IEnumerator Death()
    {
        AudioManager.Instance.PlayAudio(gameObject.GetInstanceID());

        for (int i = 0; i < bodyParts.Length; i++)
        {
            for (int j = 0; j < deathParts.Length; j++)
            {
                if (string.Compare(bodyParts[i].name, deathParts[j].name) == 0)
                {
                    SwitchBodyPart(bodyParts[i], deathParts[j]);
                    if (string.Compare(deathParts[j].name, "head") == 0) yield return new WaitForSeconds(2f);
                    break;
                }
            }
        }

        yield return new WaitUntil (() => !AudioManager.Instance.IsPlaying(gameObject.GetInstanceID()));
        AudioManager.Instance.StopAudio(gameObject.GetInstanceID());
        GameManager.Instance.customUpdateManager.RemoveCustomUpdatable(this);
        AudioManager.Instance.RemoveAudioSource(gameObject.GetInstanceID());
        yield return new WaitForSeconds(30f);
        
        gameObject.SetActive(false);
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
        }
    }

    #endregion
}