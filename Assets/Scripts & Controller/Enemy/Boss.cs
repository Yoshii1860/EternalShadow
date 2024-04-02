using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Boss : MonoBehaviour, ICustomUpdatable
{
    #region Fields

    public int health = 100;
    public bool isDead = false;
    public float damage = 25f;
    public Transform[] teleportPointsUpstairs;
    public Transform[] teleportPointsDownstairs;
    public ParticleSystem particleEffect;
    public GameObject particlePrefab;
    public GameObject priestMesh;
    public bool chaseDEBUG = false;
    public Transform player;
    public AudioSource footsteps;
    public AudioSource speaker;
    private AudioSource breath;

    private Animator animator;
    private NavMeshAgent agent;

    private float scanFrequency = 1f;
    private float timer = 0f;
    private bool teleporting = false;
    private bool started = false;

    private float distanceToPlayer = 100f;
    private int teleportPoint = 0;

    private bool hitted = false;

    #endregion

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        breath = GetComponent<AudioSource>();
        GameManager.Instance.customUpdateManager.AddCustomUpdatable(this);
    }

    public void CustomUpdate(float deltaTime)
    {
        if (timer >= scanFrequency)
        {
            timer = 0f;
            if (!teleporting && started) Attack();
        }
        else
        {
            timer += deltaTime;
        }

        if (chaseDEBUG) FollowPlayer();

        if (started) agent.SetDestination(player.position);
    }

    void Attack()
    {
        if (Vector3.Distance(transform.position, player.position) < 3f)
        {
            // play animation
            animator.SetTrigger("attack");
        }
        else if (Vector3.Distance(transform.position, player.position) > 10f)
        {
            // teleport
            Teleport();
        }
        else
        {
            animator.SetTrigger("walk");
        }
    }

    void AttackAnim()
    {
        // deal damage to player
        GameManager.Instance.player.TakeDamage(damage);
        // play a sound
        AudioManager.Instance.PlaySoundOneShot(gameObject.GetInstanceID(), "boss attack", 0.6f, 1f);
    }

    public void GetHit()
    {
        if (!hitted)
        {
            footsteps.Pause();
            hitted = true;
            agent.isStopped = true;
            teleporting = true;
            animator.SetTrigger("hit");
            StartCoroutine(ResetMovement());
        }
        health -= 10;
    }

    IEnumerator ResetMovement()
    {
        yield return new WaitForSeconds(2f);
        breath.Pause();
        speaker.Play();
        animator.SetTrigger("walk");
        agent.isStopped = true;
        footsteps.UnPause();
        teleporting = false;
        yield return new WaitUntil (() => speaker.isPlaying == false);
        breath.UnPause();
        hitted = false;
    }

    void Teleport()
    {
        if (!teleporting) StartCoroutine(TeleportCoroutine());
    }

    IEnumerator TeleportCoroutine()
    {
        teleporting = true;
        // Stop the priest
        footsteps.Pause();
        agent.isStopped = true;
        animator.SetTrigger("idle");
        yield return new WaitForSeconds(0.5f);
        // play particle effect
        particleEffect.Play();
        AudioManager.Instance.PlaySoundOneShot(gameObject.GetInstanceID(), "teleport forth", 1f, 1f);
        // when particle effect is done, disable priest
        yield return new WaitUntil(() => particleEffect.isStopped);
        priestMesh.SetActive(false);
        // move priest to another location
        Transform teleporter = ScanPositions();
        yield return new WaitForSeconds(0.25f);
        // instantiate particle prefab
        GameObject obj = Instantiate(particlePrefab, teleporter.position, Quaternion.identity);
        obj.GetComponent<ParticleSystem>().Play();
        AudioManager.Instance.PlaySoundOneShot(gameObject.GetInstanceID(), "teleport back", 1f, 1f);
        transform.position = teleporter.position;
        // after 1-2 seconds, enable priest
        yield return new WaitForSeconds(1.5f);
        priestMesh.SetActive(true);
        // follow player
        yield return new WaitForSeconds(0.5f);
        footsteps.UnPause();
        animator.SetTrigger("walk");
        agent.isStopped = false;
        // destroy particle prefab
        yield return new WaitForSeconds(2f);
        Destroy(obj);
        teleporting = false;
    }

    Transform ScanPositions()
    {
        if (player.position.y < -3.76f)
        {
            for (int i = 0; i < teleportPointsDownstairs.Length; i++)
            {
                float newDistance = Vector3.Distance(teleportPointsDownstairs[i].position, player.position);
                if (newDistance < distanceToPlayer)
                {
                    distanceToPlayer = newDistance;
                    teleportPoint = i;
                }
            }
            StartCoroutine(ResetDistance());
            return teleportPointsDownstairs[teleportPoint];
        }
        else
        {
            for (int i = 0; i < teleportPointsUpstairs.Length; i++)
            {
                float newDistance = Vector3.Distance(teleportPointsUpstairs[i].position, player.position);
                if (newDistance < distanceToPlayer)
                {
                    distanceToPlayer = newDistance;
                    teleportPoint = i;
                }
            }
            StartCoroutine(ResetDistance());
            return teleportPointsUpstairs[teleportPoint];
        }
    }

    IEnumerator ResetDistance()
    {
        yield return new WaitForSeconds(1f);
        distanceToPlayer = 100f;
    }

    public void FollowPlayer()
    {
        started = true;
        animator.SetTrigger("walk");
        agent.SetDestination(player.position);
        Debug.Log("START TO FOLLOW PLAYER");
        footsteps.Play();
    }

    void Die()
    {
        
    }


}
