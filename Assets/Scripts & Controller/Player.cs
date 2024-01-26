using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Player : MonoBehaviour, ICustomUpdatable
{
    #region Fields

    [Tooltip("The health of the player.")]
    public float health = 100f;
    [Tooltip("The stamina of the player.")]
    public float stamina = 100f;
    public bool isBleeding = false;
    public bool isDizzy = false;
    public bool isOutOfStamina = false;
    public float staminaConsumptionTimer = 0.1f;
    public float staminaReg = 0.4f;
    [SerializeField] Light flashlight;
    public bool lightAvail = false;

    [SerializeField] string[] painClips;
    [SerializeField] string[] hitClips;
    [SerializeField] string[] poisenedClips;
    [SerializeField] string[] dizzyClips;

    [Space(10)]
    [Header("References")]
    [SerializeField] Image bloodOverlay;
    [SerializeField] Image poisonOverlay;
    [SerializeField] TextMeshProUGUI staminaText;
    [SerializeField] TextMeshProUGUI bulletsText;

    // Stats-related
    private float reducedStamina;
    private float maxHealth = 100f;
    private float maxStamina = 100f;
    bool healthReg = false;
    bool reducingStamina = false;
    bool increasingStamina = false;

    // Audio-related
    bool isPlaying = false;
    int speakerID;
    int speaker2ID;
    bool breath = false;
    bool breathLouder = false;
    bool breathLoudest = false;
    bool isMoaning = false;
    bool heartbeat = false;

    // Timer-related
    bool waitTimer = false;
    bool timerIsRunning = false;
    int waitingTime = 15;

    #endregion

    #region Unity Lifecycle Methods

    void Start()
    {
        flashlight.enabled = false;
        speakerID = AudioManager.Instance.playerSpeaker;
        speaker2ID = AudioManager.Instance.playerSpeaker2;
    }

    public void CustomUpdate(float deltaTime)
    {
        Breathing();
        LowLife();
        if (waitTimer && !timerIsRunning)
        {
            timerIsRunning = true;
            StartCoroutine(WaitTimer(waitingTime));
        }
    }

    #endregion

    #region Debug Methods

    public bool isPoisonedDebug = false;
    public bool isNotPoisonedDebug = false;
    public bool poisonMeDebug = false;

    void Update()
    {
        if (isPoisonedDebug)
        {
            isPoisoned = true;
        }
        if (isNotPoisonedDebug)
        {
            isPoisoned = false;
        }

        if (poisonMeDebug)
        {
            Poisoned();
            poisonMeDebug = false;
        }
    }

    // Private backing field for isPoisoned property
    [SerializeField] bool _isPoisoned = false;

    // Public property for isPoisoned
    public bool isPoisoned
    {
        get => _isPoisoned;
        set
        {
            if (_isPoisoned != value)
            {
                _isPoisoned = value;
                OnIsPoisonedChanged();
            }
        }
    }

    #endregion

    #region Health and State Methods

    void LowLife()
    {
    	if (health >= 80f)
        {
            if (isMoaning) isMoaning = false;
            if (heartbeat)
            {
                AudioManager.Instance.StopAudioWithDelay(speaker2ID);
                heartbeat = false;
            }

            // turn transparency of blood effect to lost health and keep the other colors
            bloodOverlay.color = new Color(bloodOverlay.color.r, bloodOverlay.color.g, bloodOverlay.color.b, 0f);
        }
        if (health < 80f && health >= 60f)
        {
            if (isMoaning) isMoaning = false;
            if (heartbeat)
            {
                AudioManager.Instance.StopAudioWithDelay(speaker2ID);
                heartbeat = false;
            }

            bloodOverlay.color = new Color(bloodOverlay.color.r, bloodOverlay.color.g, bloodOverlay.color.b, ((maxHealth - health) / 200f));
        }
        if (health < 60f && health >= 30f)
        {
            if (!isMoaning) isMoaning = true;
            if (heartbeat)
            {
                AudioManager.Instance.StopAudioWithDelay(speaker2ID);
                heartbeat = false;
            }

            bloodOverlay.color = new Color(bloodOverlay.color.r, bloodOverlay.color.g, bloodOverlay.color.b, ((maxHealth - health) / 150f));

            if (!waitTimer)
            {
                // randomize and play moaning sound
                waitingTime = Random.Range(10, 30);
                AudioManager.Instance.PlaySoundOneShot(speakerID, painClips[Random.Range(0, painClips.Length)], Random.Range(0.2f, 0.4f), Random.Range(0.85f, 1.1f));
                waitTimer = true;
            }
        }
        if (health < 30f)
        {
            bloodOverlay.color = new Color(bloodOverlay.color.r, bloodOverlay.color.g, bloodOverlay.color.b, ((maxHealth - health) / 100f));

            if (!waitTimer)
            {
                // randomize moaning sound
                // play moaning sound
                waitingTime = Random.Range(5, 20);
                AudioManager.Instance.PlaySoundOneShot(speakerID, painClips[Random.Range(0, painClips.Length)], Random.Range(0.4f, 0.6f), Random.Range(0.85f, 1.1f));
                waitTimer = true;
            }
            
            // play heartbeat sound
            if (!heartbeat)
            {
                AudioManager.Instance.SetAudioClip(speaker2ID, "heartbeat");
                AudioManager.Instance.PlayAudio(speaker2ID, 0.2f, 1f, true);
                heartbeat = true;
                StartCoroutine(HeartbeatScreen());
            }

            // recover health after 10 seconds and up to 30 health
            if (!healthReg)
            {
                healthReg = true;
                StartCoroutine(RecoverHealth());
            }
        }
    }

    public void Poisoned()
    {
        isPoisoned = true;
        AudioManager.Instance.PlaySoundOneShot(speakerID, hitClips[Random.Range(0, hitClips.Length)], Random.Range(0.3f, 0.6f), Random.Range(0.85f, 1.1f));
        StartCoroutine(PoisonedRoutine());
    }

    IEnumerator PoisonedRoutine()
    {
        poisonOverlay.color = new Color(poisonOverlay.color.r, poisonOverlay.color.g, poisonOverlay.color.b, 0.018f);
        
        while (isPoisoned)
        {
            waitingTime = Random.Range(20, 60);
            if (!waitTimer && !isMoaning)
            {
                // randomize and play moaning sound
                AudioManager.Instance.PlaySoundOneShot(speakerID, poisenedClips[Random.Range(0, poisenedClips.Length)], Random.Range(0.2f, 0.4f), Random.Range(0.85f, 1.1f));
                waitTimer = true;
            }

            yield return new WaitForSeconds(2f);
            //  change transparency of poison overlay flawlessly
            for (float i = 0.018f; i <= 0.078f; i += 0.01f)
            {
                poisonOverlay.color = new Color(poisonOverlay.color.r, poisonOverlay.color.g, poisonOverlay.color.b, i);
                yield return new WaitForSeconds(0.1f);
            }

            // change transparency of poison overlay back
            for (float i = 0.078f; i >= 0.018f; i -= 0.01f)
            {
                poisonOverlay.color = new Color(poisonOverlay.color.r, poisonOverlay.color.g, poisonOverlay.color.b, i);
                yield return new WaitForSeconds(0.1f);
            }
        }
        if (!isPoisoned)
        {
            poisonOverlay.color = new Color(poisonOverlay.color.r, poisonOverlay.color.g, poisonOverlay.color.b, 0f);
        }
    }

    IEnumerator HeartbeatScreen()
    {
        while(heartbeat)
        {
            // scale bloodOverlay to heartbeat
            yield return new WaitForSeconds(0.15f);
            bloodOverlay.transform.localScale = new Vector3(1.01f, 1.01f, 1.01f);
            yield return new WaitForSeconds(0.5f);
            bloodOverlay.transform.localScale = new Vector3(1f, 1f, 1f);
            yield return new WaitForSeconds(0.62f);
        }
    }

    IEnumerator RecoverHealth()
    {
        yield return new WaitForSeconds(5f);
        while (health < 30f)
        {
            health += 1f;
            yield return new WaitForSeconds(5f);
        }
        healthReg = false;
    }

    IEnumerator WaitTimer(int timer)
    {
        yield return new WaitForSeconds(timer);
        waitTimer = false;
        timerIsRunning = false;
    }

    #endregion

    #region Stamina Methods

    public void IncreaseStamina() 
    {
        if (stamina > maxStamina)
        {
            return;
        }
        else if (!increasingStamina)
        {
            StartCoroutine(IncreaseStaminaOverTime());
        }
    }

    public void ReduceStamina()
    {
        if (stamina <= 0f)
        {
            isOutOfStamina = true;
            return;
        }
        else if (!reducingStamina)
        {
            StartCoroutine(ReduceStaminaOverTime());
        }
    }

    IEnumerator ReduceStaminaOverTime()
    {
        reducingStamina = true;
        yield return new WaitForSeconds(staminaConsumptionTimer);
        if (stamina != 0f) stamina -= 1f;
        staminaText.text = stamina.ToString();
        reducingStamina = false;
    }

    IEnumerator IncreaseStaminaOverTime()
    {
        increasingStamina = true;
        yield return new WaitForSeconds(staminaReg);
        if (stamina < maxStamina) stamina += 1f;
        staminaText.text = stamina.ToString();
        increasingStamina = false;
    }

    #endregion

    #region Stamina Methods

    void Breathing()
    {
        if (stamina > 60f)
        {
            return;
        }
        else if (   stamina < 60f 
                &&  stamina > 30f
                &&  !breath)
        {
            if (!isMoaning) 
            {
                AudioManager.Instance.SetAudioClip(speakerID, "breathing");
                AudioManager.Instance.PlayAudio(speakerID, 0.4f, 1f, true);
            }
            Debug.Log("Playing breathing");
            breath = true;
            breathLouder = false;
            breathLoudest = false;
            StartCoroutine(StopBreathing());
            return;
        }
        else if (   stamina < 30f 
                &&  !isOutOfStamina
                &&  !breathLouder)
        {
            if (!isMoaning)
            {
                AudioManager.Instance.SetAudioClip(speakerID, "breathing");
                AudioManager.Instance.PlayAudio(speakerID, 0.7f, 1f, true);
            }
            Debug.Log("Playing breathing louder");
            breath = false;
            breathLouder = true;
            breathLoudest = false;
            return;
        }
        else if (   isOutOfStamina
                &&  !breathLoudest)
        {
            if (!isMoaning)
            {
                AudioManager.Instance.SetAudioClip(speakerID, "breathing");
                AudioManager.Instance.PlayAudio(speakerID, 1.1f, 1f, true);
            }
            Debug.Log("Playing breathing loudest");
            breath = false;
            breathLouder = false;
            breathLoudest = true;
            return;
        }
    }

    IEnumerator StopBreathing()
    {
        yield return new WaitUntil(() => stamina > 60f);
        AudioManager.Instance.StopAudioWithDelay(speakerID);
        Debug.Log("Stopping breathing");
        breath = false;
        breathLouder = false;
        breathLoudest = false;
    }

    #endregion

    #region Property Changing Method

    void OnIsPoisonedChanged()
    {
        if (isPoisoned)
        {
            maxStamina = 80f;
            if (stamina > maxStamina) stamina = 80f;
            staminaReg = 0.3f;
            staminaText.text = stamina.ToString();
        }
        else
        {
            maxStamina = 100f;
            staminaReg = 0.4f;
            staminaText.text = stamina.ToString();
        }
    }

    #endregion

    #region Player Actions

    public void LightSwitch()
    {
        if (lightAvail)
        {
            flashlight.enabled = !flashlight.enabled;
            Debug.Log("Light is available");
        }
        else
        {
            Debug.Log("Light is not available");
        }
    }

    public bool TakeDamage(float damage)
    {
        health -= damage;
        Debug.Log("Player took " + damage + " damage. " + health + " health left.");
        bool isDead = health <= 0;
        if (isDead) Die();
        return isDead;
    }

    private void Die()
    {
        Destroy(transform.GetComponent<CapsuleCollider>());
        Debug.Log("Player died.");
    }

    #endregion

    #region UI Methods

    public void SetBulletsUI(int currentBullets, int bulletsInInventory)
    {
        bulletsText.text = currentBullets + "/" + bulletsInInventory;
    }

    #endregion
}