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
    [SerializeField] string hitClip = "";

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
        AudioSource[] audioSources = GetComponentsInChildren<AudioSource>();
        Debug.Log("AudioSources length of enemy: " + audioSources.Length);
        foreach (AudioSource audioSource in audioSources)
        {
            AudioManager.Instance.StopAudio(audioSource.gameObject.GetInstanceID());
        }
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
}