using System.Collections;
using UnityEngine;

[RequireComponent(typeof(UniqueIDComponent))]
[RequireComponent(typeof(AISensor))]
public class Enemy : MonoBehaviour
{
    #region Fields

    public int health = 100;
    public bool isDead = false;
    public bool isShot = false;
    public float damage = 10f;
    [Tooltip("The time the enemy stays down after being shot.")]
    [SerializeField] float downTimer = 30f;
    [Tooltip("The time the enemy starts with get up animation after being shot.")]
    [SerializeField] float reducedTimer = 25f;
    [SerializeField] string deathClip = "";
    [SerializeField] string fallClip = "";
    [SerializeField] float deathToFallTime = 1f;
    [SerializeField] float fallVolume = 1f;
    
    // Audio Variables
    bool slenderStep = false;
    bool girlStep = false;
    public bool slender = false;
    int slenderStepID;
    int girlStepID;
    bool girlShout = false;
    bool girlSpeak = false;
    bool slenderShout = false; 
    bool slenderSpeak = false;

    #endregion

    #region Public Methods

    /// Inflict damage to the enemy.
    public void TakeDamage(int damage)
    {
        health -= damage;
        
        // Check if the enemy has no health left
        if (health <= 0)
        {
            isDead = true;
            Die();
        }

        // Trigger the enemy shot coroutine
        StartCoroutine(EnemyShot());
    }

    #endregion

    #region Private Methods

    private void Start()
    {
        if (slender) slenderStepID = transform.GetChild(0).gameObject.GetInstanceID();
        else girlStepID = transform.GetChild(0).gameObject.GetInstanceID();
    }

    /// Handles the death of the enemy.
    private void Die()
    {
        // Deactivate the enemy game object
        gameObject.SetActive(false);
    }

    /// Coroutine to handle the enemy being shot and the chase timer.
    private IEnumerator EnemyShot()
    {
        // Set the enemy as shot
        isShot = true;

        transform.GetComponent<Animator>().SetTrigger("die");

        AudioManager.Instance.StopAudio(gameObject.GetInstanceID());

        yield return new WaitForSeconds(0.2f);

        AudioManager.Instance.PlaySoundOneShot(gameObject.GetInstanceID(), deathClip);

        yield return new WaitForSeconds(deathToFallTime);

        AudioManager.Instance.PlaySoundOneShot(gameObject.GetInstanceID(), fallClip, fallVolume);

        StartCoroutine(EnemyGetUp());

        // Wait for the specified chase timer duration
        yield return new WaitForSeconds(downTimer);

        // Reset the shot state
        isShot = false;
    }

    IEnumerator EnemyGetUp()
    {
        yield return new WaitForSeconds(reducedTimer);
        transform.GetComponent<Animator>().SetTrigger("getup");
    }

    private void Hit() 
    {
        AudioManager.Instance.PlaySoundOneShot(transform.gameObject.GetInstanceID(), "slender punch", 0.6f);
        GameManager.Instance.player.TakeDamage(damage);
    }

    #endregion

    #region Anim Events

    public void SlenderWalk()
    {
        float volume = AudioManager.Instance.slenderVolume;

        if (!slenderStep)
        {
            AudioManager.Instance.PlaySoundOneShot(slenderStepID, "slender walk 1", volume);
            slenderStep = true;
        }
        else
        {
            AudioManager.Instance.PlaySoundOneShot(slenderStepID, "slender walk 2", volume);
            slenderStep = false;
        }
    }

    public void SlenderRun()
    {
        float volume = AudioManager.Instance.slenderVolume;

        AudioManager.Instance.PlaySoundOneShot(slenderStepID, "slender run", volume);
    }

    public void SlenderShout()
    {
        if (slenderShout) return;
        if (slenderSpeak) 
        {
            AudioManager.Instance.StopAudioWithDelay(gameObject.GetInstanceID(), 0.5f);
        }

        float volume = AudioManager.Instance.slenderVolume;

        AudioManager.Instance.SetAudioClip(gameObject.GetInstanceID(), "slender scream", volume, 1f, true);
        AudioManager.Instance.PlayAudioWithDelay(gameObject.GetInstanceID(), 0.6f);

        slenderShout = true;
        slenderSpeak = false;
    }

    public void SlenderSpeak()
    {
        if (slenderSpeak) return;
        if (slenderShout) 
        {
            AudioManager.Instance.StopAudioWithDelay(gameObject.GetInstanceID(), 0.5f);
        }

        float volume = AudioManager.Instance.slenderVolume;

        AudioManager.Instance.SetAudioClip(gameObject.GetInstanceID(), "slender hello", volume, 1f, true);
        AudioManager.Instance.PlayAudioWithDelay(gameObject.GetInstanceID(), 0.6f);

        slenderSpeak = true;
        slenderShout = false;
    }

    public void SlenderDie()
    {
        slenderShout = false;
        slenderSpeak = false;
        AudioManager.Instance.StopAudio(gameObject.GetInstanceID());
    }

    public void GirlWalk()
    {
        float volume = AudioManager.Instance.girlVolume;

        if (!girlStep)
        {
            AudioManager.Instance.PlaySoundOneShot(girlStepID, "woman step 1", volume);
            girlStep = true;
        }
        else
        {
            AudioManager.Instance.PlaySoundOneShot(girlStepID, "woman step 2", volume);
            girlStep = false;
        }
    }

    public void GirlRun()
    {
        float volume = AudioManager.Instance.girlVolume;

        AudioManager.Instance.PlaySoundOneShot(girlStepID, "woman run", volume);
    }

    public void GirlShout()
    {
        if (girlShout) return;
        if (girlSpeak) 
        {
            AudioManager.Instance.StopAudioWithDelay(gameObject.GetInstanceID(), 0.5f);
        }

        float volume = AudioManager.Instance.girlVolume;

        AudioManager.Instance.SetAudioClip(gameObject.GetInstanceID(), "woman scream", volume, 1f, true);
        AudioManager.Instance.PlayAudioWithDelay(gameObject.GetInstanceID(), 0.6f);

        girlShout = true;
        girlSpeak = false;
    }

    public void GirlSpeak()
    {
        if (girlSpeak) return;
        if (girlShout) 
        {
            AudioManager.Instance.StopAudioWithDelay(gameObject.GetInstanceID(), 0.5f);
        }

        float volume = AudioManager.Instance.girlVolume;

        AudioManager.Instance.SetAudioClip(gameObject.GetInstanceID(), "weeping ghost woman", volume, 1f, true);
        AudioManager.Instance.PlayAudioWithDelay(gameObject.GetInstanceID(), 0.6f);

        girlSpeak = true;
        girlShout = false;
    }

    public void GirlDie()
    {
        girlShout = false;
        girlSpeak = false;
        AudioManager.Instance.StopAudio(gameObject.GetInstanceID());
    }

    #endregion
}