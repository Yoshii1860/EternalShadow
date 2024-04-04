using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum BossState { Chase, Attack, Stunned, Teleporting, Half, Die };

public class Boss : MonoBehaviour, ICustomUpdatable
{
    #region Fields
    
    public BossState currentState;

    [Header("Boss Attributes")]
    [Tooltip("Health of the boss")]
    public int health = 100;
    [Tooltip("Damage dealt to the player")]
    public float damage = 25f;
    [Space(10)]

    [Header("Light Components")]
    [Tooltip("Sun object in the scene")]
    public GameObject sun;
    [Tooltip("Reflection probes object in the scene")]
    public GameObject reflectionProbes;
    [Tooltip("Reflection probes object in the scene when half health is reached")]
    public GameObject reflectionProbesDark;
    [Tooltip("Lights container containing all the lights in the scene")]
    public Transform lightsContainer;
    [Space(10)]

    [Header("Teleport Points")]
    [Tooltip("Teleport points for the boss to move around the scene upstairs and downstairs")]
    public Transform[] teleportPointsUpstairs;
    public Transform[] teleportPointsDownstairs;

    [Header("Particles")]
    [Tooltip("Dissolve particle system")]
    public ParticleSystem dissolveParticle;
    [Tooltip("Explosion particle system")]
    public ParticleSystem explosionParticle;
    [Tooltip("Steam particle system")]
    public ParticleSystem steamParticle;
    [Tooltip("Teleport particle system prefab")]
    public GameObject particlePrefab;
    [Tooltip("Steam particle system prefab")]
    public GameObject steamPrefab;
    [Tooltip("Fire particle system prefab")]
    public GameObject firePrefab;
    [Space(10)]

    [Header("Other Components")]
    [Tooltip("Mesh of the boss")]
    public GameObject priestMesh;
    [Tooltip("Player object")]
    public Transform player;
    [Space(10)]

    [Header("Audio Components")]
    [Tooltip("Footsteps audio source")]
    public AudioSource footsteps;
    [Tooltip("Speaker audio source")]
    public AudioSource speaker;
    [Tooltip("Breath audio source")]
    private AudioSource breath;
    [Space(10)]

    [Header("Debug")]
    [Tooltip("Debug mode")]
    public bool debugMode = false;

    // Other Boss components
    private Animator animator;
    private NavMeshAgent agent;
    private Collider[] colliders;

    // Variables for calculations of teleport points
    private float distanceToPlayer = 100f;
    private int teleportPoint = 0;

    // Booleans for states
    private bool hitted = false;
    private bool teleporting = false;
    private bool halfHealth = false;
    private bool isDead = false;
    private bool halfEventPlayed = false;
    private bool halfEventStarted = false;


    #endregion

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        breath = GetComponent<AudioSource>();
        colliders = GetComponents<Collider>();
    }

    //////////////////////////////////////////////
    // STATE MACHINE
    //////////////////////////////////////////////

    public void CustomUpdate(float deltaTime)
    {
        switch (currentState)
        {
            case BossState.Chase:
                if (debugMode) Debug.Log("Chase");

                if (Vector3.Distance(transform.position, player.position) < 3f)
                {
                    currentState = BossState.Attack;
                }
                else if (Vector3.Distance(transform.position, player.position) > 10f)
                {
                    animator.SetTrigger("idle");
                    currentState = BossState.Teleporting;
                }
                Chase();
                break;
    //////////////////////////////////////////////
            case BossState.Attack:
                if (debugMode) Debug.Log("Attack");
                if (Vector3.Distance(transform.position, player.position) > 3f && Vector3.Distance(transform.position, player.position) < 10f)
                {
                    animator.SetTrigger("walk");
                    currentState = BossState.Chase;
                }
                else if (Vector3.Distance(transform.position, player.position) > 10f)
                {
                    animator.SetTrigger("idle");
                    currentState = BossState.Teleporting;
                }
                else animator.SetTrigger("attack");
                break;
    //////////////////////////////////////////////
            case BossState.Stunned:
                if (debugMode) Debug.Log("Stunned");
                Stunned();
                break;
    //////////////////////////////////////////////
            case BossState.Teleporting:
                if (debugMode) Debug.Log("Teleporting");
                if (!teleporting) StartCoroutine(TeleportCoroutine());
                break;
    //////////////////////////////////////////////
            case BossState.Half:
                if (debugMode) Debug.Log("Half");
                if (!halfEventStarted) StartCoroutine(HalfEventRoutine());
                break;
    //////////////////////////////////////////////
            case BossState.Die:
                if (debugMode) Debug.Log("Die");
                if (!isDead) Die();
                break;
        }

        if (health <= 50 && !halfHealth)
        {
            halfHealth = true;
            currentState = BossState.Half;
        }
        else if (health <= 0 && !isDead)
        {
            currentState = BossState.Die;
        }
    }

    //////////////////////////////////////////////
    // STATE MACHINE END
    //////////////////////////////////////////////



    //////////////////////////////////////////////
    //  Chase START
    //////////////////////////////////////////////

    void Chase()
    {
        agent.isStopped = false;
        agent.SetDestination(player.position);
        animator.SetTrigger("walk");
    }

    //////////////////////////////////////////////
    //  Chase END
    //////////////////////////////////////////////



    //////////////////////////////////////////////
    //  Stunned START
    //////////////////////////////////////////////

    void Stunned()
    {
        if (!hitted) hitted = true;
        else return;
        AudioManager.Instance.PlaySoundOneShot(gameObject.GetInstanceID(), "priest hit", 1f, 1f);
        animator.SetTrigger("hit");
        footsteps.Pause();
        agent.isStopped = true;
        StartCoroutine(ResetMovement());
    }

    IEnumerator ResetMovement()
    {
        yield return new WaitForSeconds(2f);
        breath.Pause();
        speaker.Play();
        footsteps.UnPause();
        currentState = BossState.Chase;
        yield return new WaitUntil (() => speaker.isPlaying == false);
        breath.UnPause();
        yield return new WaitForSeconds(1f);
        hitted = false;
    }

    //////////////////////////////////////////////
    //  Stunned END
    //////////////////////////////////////////////



    //////////////////////////////////////////////
    //  TELEPORT START
    //////////////////////////////////////////////

    IEnumerator TeleportCoroutine()
    {
        teleporting = true;
        foreach (Collider col in colliders) col.enabled = false;
        // Stop the priest
        dissolveParticle.Play();
        yield return new WaitForSeconds(3f);
        animator.SetTrigger("idle");
        footsteps.Pause();
        agent.isStopped = true;
        AudioManager.Instance.PlaySoundOneShot(gameObject.GetInstanceID(), "explosion", 1f, 1f);
        explosionParticle.Play();
        GameObject firePS = Instantiate(firePrefab, transform.position, Quaternion.identity);
        steamParticle.Play();
        yield return new WaitForSeconds(0.3f);
        // play particle effect
        priestMesh.SetActive(false);
        AudioManager.Instance.PlaySoundOneShot(gameObject.GetInstanceID(), "teleport forth", 1f, 1f);
        // when particle effect is done, disable priest
        yield return new WaitForSeconds(1f);
        // move priest to another location
        dissolveParticle.Stop();
        explosionParticle.Stop();
        steamParticle.Stop();
        Transform[] teleporters = ScanPositions();
        yield return new WaitForSeconds(0.25f);
        // instantiate particle prefab
        GameObject obj = Instantiate(particlePrefab, teleporters[0].position, Quaternion.identity);
        GameObject obj2 = null;
        if (halfHealth) obj2 = Instantiate(particlePrefab, teleporters[1].position, Quaternion.identity);

        if (halfHealth && !halfEventPlayed)
        {
            StartCoroutine(HalfEventRoutine());
            yield return new WaitUntil(() => halfEventPlayed == true);
        }

        obj.GetComponent<ParticleSystem>().Play();
        GameObject steamPS = Instantiate(steamPrefab, teleporters[0].position, Quaternion.identity);
        GameObject steamPS2 = null;
        if (halfHealth) 
        {
            obj2.GetComponent<ParticleSystem>().Play();
            steamPS2 = Instantiate(steamPrefab, teleporters[1].position, Quaternion.identity);
        }
        yield return new WaitForSeconds(2f);
        AudioManager.Instance.PlaySoundOneShot(gameObject.GetInstanceID(), "teleport back", 1f, 1f);
        // random number between 0 and 1   
        int random = 0;
        if (halfHealth) random = Random.Range(0, 2);
        transform.position = teleporters[random].position;
        priestMesh.SetActive(true);
        hitted = true;
        Destroy(firePS);
        foreach (Collider col in colliders) col.enabled = true;
        // follow player
        yield return new WaitForSeconds(0.5f);
        footsteps.UnPause();
        agent.isStopped = false;
        animator.SetTrigger("walk");
        currentState = BossState.Chase;
        // destroy particle prefab
        yield return new WaitForSeconds(1.5f);
        hitted = false;
        yield return new WaitForSeconds(0.5f);
        Destroy(obj);
        Destroy(steamPS);
        if (halfHealth) 
        {
            Destroy(obj2);
            Destroy(steamPS2);
        }
        teleporting = false;

    }

    Transform[] ScanPositions()
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

            Transform[] teleportPoints = new Transform[2];
            teleportPoints[0] = teleportPointsDownstairs[teleportPoint];
            if (teleportPoint + 1 != teleportPointsDownstairs.Length) 
                teleportPoints[1] = teleportPointsDownstairs[teleportPoint + 1];
            else if (teleportPoint - 1 != -1)
                teleportPoints[1] = teleportPointsDownstairs[teleportPoint - 1];
            return teleportPoints;
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

            Transform[] teleportPoints = new Transform[2];
            teleportPoints[0] = teleportPointsUpstairs[teleportPoint];
            if (teleportPoint + 1 != teleportPointsUpstairs.Length) 
                teleportPoints[1] = teleportPointsUpstairs[teleportPoint + 1];
            else if (teleportPoint - 1 != -1)
                teleportPoints[1] = teleportPointsUpstairs[teleportPoint - 1];
            return teleportPoints;
        }
    }

    IEnumerator ResetDistance()
    {
        yield return new WaitForSeconds(1f);
        distanceToPlayer = 100f;
    }

    //////////////////////////////////////////////
    //  TELEPORT END
    //////////////////////////////////////////////



    //////////////////////////////////////////////
    //  Half Event START
    //////////////////////////////////////////////

    IEnumerator HalfEventRoutine()
    {
        halfEventStarted = true;
        yield return new WaitUntil(() => teleporting == false);

        LightsOff(lightsContainer);
        yield return new WaitForSeconds(0.5f);
        reflectionProbes.SetActive(false);
        reflectionProbesDark.SetActive(true);
        AudioManager.Instance.PlaySoundOneShot(AudioManager.Instance.playerSpeaker2, "power down", 0.5f, 1f);
        yield return new WaitForSeconds(5f);
        agent.speed = 5f;
        halfEventPlayed = true;
    }

    void LightsOff(Transform lights)
    {
        for (int i = 0; i < lights.childCount; i++)
        {
            Transform lightChild = lights.GetChild(i);
            Light light = lightChild.GetComponent<Light>();
            if (light != null)
            {
                light.enabled = false;
            }
            else if (lightChild.childCount > 0)
            {
                LightsOff(lightChild);
            }
        }
        sun.SetActive(false);
    }

    //////////////////////////////////////////////
    //  Half Event END
    //////////////////////////////////////////////



    //////////////////////////////////////////////
    //  DIE START
    //////////////////////////////////////////////

    void Die()
    {
        isDead = true;
        agent.isStopped = true;
        speaker.Stop();
        footsteps.Stop();
        AudioManager.Instance.FadeOut(breath.gameObject.GetInstanceID(), 1f);
        AudioManager.Instance.FadeOut(AudioManager.Instance.environment, 5f);
        dissolveParticle.Stop();
        explosionParticle.Stop();
        steamParticle.Stop();
        animator.SetTrigger("die");
        GameManager.Instance.customUpdateManager.RemoveCustomUpdatable(this);
    }

    //////////////////////////////////////////////
    //  DIE END
    //////////////////////////////////////////////



    //////////////////////////////////////////////
    //  PUBLIC AND EVENT METHODS
    //////////////////////////////////////////////

    // Used to start the event
    public void FollowPlayer()
    {
        GameManager.Instance.customUpdateManager.AddCustomUpdatable(this);
        currentState = BossState.Chase;
        footsteps.Play();
        Debug.Log("START TO FOLLOW PLAYER");
    }

    // called by weapon script
    public void GetHit()
    {
        health -= 10;
        if (health <= 50 && !halfHealth) return;
        if (currentState != BossState.Stunned && health > 0 && !hitted) currentState = BossState.Stunned;
        else if (health <= 0) currentState = BossState.Die;
    }

    // called by animation event
    void AttackAnim()
    {
        // deal damage to player
        GameManager.Instance.player.TakeDamage(damage);
        // play a sound
        AudioManager.Instance.PlaySoundOneShot(gameObject.GetInstanceID(), "mannequin hit", 0.6f, 1f);
    }

    //////////////////////////////////////////////
    //  PUBLIC AND EVENT METHODS END
    //////////////////////////////////////////////
}
