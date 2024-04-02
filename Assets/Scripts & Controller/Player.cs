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
    public bool isPoisoned = false;
    public bool isOutOfStamina = false;
    public float staminaConsumptionTimer = 0.1f;
    public float staminaReg = 0.4f;
    public Light flashlight;
    public bool lightAvail = false;
    public float maxHealth = 100f;
    public float maxStamina = 100f;

    [SerializeField] string[] painClips;
    [SerializeField] string[] hitClips;
    [SerializeField] string[] poisenedClips;
    [SerializeField] string[] dizzyClips;

    [Space(10)]
    [Header("UI References")]
    [SerializeField] Image bloodOverlay;
    [SerializeField] Image poisonOverlay;
    [SerializeField] Image dizzyOverlay;
    [SerializeField] TextMeshProUGUI staminaText;
    [SerializeField] TextMeshProUGUI bulletsText;
    [SerializeField] Image bulletsIcon;
    Coroutine bulletsDisplayRoutine;

    // Stats-related
    private float reducedStamina;
    bool healthReg = false;
    bool reducingStamina = false;
    bool increasingStamina = false;

    // Audio-related
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
    bool formerShowBool = false;

    #endregion

    #region Unity Lifecycle Methods

    void Start()
    {
        flashlight.enabled = false;
        speakerID = AudioManager.Instance.playerSpeaker;
        speaker2ID = AudioManager.Instance.playerSpeaker2;
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Characters"), LayerMask.NameToLayer("Enemies"));
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

    public bool poisonDebug = false;
    public bool dizzyDebug = false;
    public bool bleedingDebug = false;

    // Just for DEBUG
    void Update()
    {
        if (isPoisoned && !poisonDebug)
        {
            Poisoned();
            poisonDebug = true;
        }

        if (isDizzy && !dizzyDebug)
        {
            Dizzy();
            dizzyDebug = true;
        }

        if (isBleeding && !bleedingDebug)
        {
            Bleeding();
            bleedingDebug = true;
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
        GameManager.Instance.DisplayMessage("You have an infection. Take some antibiotics!", 3f);
        OnIsPoisonedChanged();
        StartCoroutine(PoisonEffect());
        StartCoroutine(PoisonedRoutine());
    }

    IEnumerator PoisonEffect()
    {
        //  change transparency of poison overlay flawlessly
        while (isPoisoned)
        {
            waitingTime = Random.Range(20, 60);
            if (!waitTimer && !isMoaning)
            {
                // randomize and play moaning sound
                if (!GameManager.Instance.isPaused) AudioManager.Instance.PlaySoundOneShot(speakerID, poisenedClips[Random.Range(0, poisenedClips.Length)], Random.Range(0.2f, 0.4f), Random.Range(0.85f, 1.1f));
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

            if (!isPoisoned)
            {
                poisonOverlay.color = new Color(poisonOverlay.color.r, poisonOverlay.color.g, poisonOverlay.color.b, 0f);
            }
        }
    }

    IEnumerator PoisonedRoutine()
    {
        poisonOverlay.color = new Color(poisonOverlay.color.r, poisonOverlay.color.g, poisonOverlay.color.b, 0.018f);
        // randomize and play moaning sound
        AudioManager.Instance.PlaySoundOneShot(speakerID, hitClips[Random.Range(0, hitClips.Length)], Random.Range(0.3f, 0.6f), Random.Range(0.85f, 1.1f));
        
        StartCoroutine(PoisonEffect());

        yield return new WaitUntil (() => !isPoisoned);
        Debug.Log("Poisoned routine ended.");
        poisonOverlay.color = new Color(poisonOverlay.color.r, poisonOverlay.color.g, poisonOverlay.color.b, 0f);
    }

    public void Dizzy()
    {
        GameManager.Instance.DisplayMessage("You suffer from a concussion. Take some painkiller!", 3f);
        isDizzy = true;
        StartCoroutine(DizzinessEffect());
        StartCoroutine(DizzyRoutine());
    }

    IEnumerator DizzyRoutine()
    {
        yield return new WaitForSeconds(10f);

        while (isDizzy && !GameManager.Instance.isPaused)
        {
            waitingTime = Random.Range(20, 120);
            yield return new WaitForSeconds(waitingTime);
            if (!isDizzy) break;
            yield return new WaitUntil(() => !GameManager.Instance.isPaused);
            StartCoroutine(DizzinessEffect());
            yield return new WaitForSeconds(10f);
        }

        if (!isDizzy)
        {
            dizzyOverlay.color = new Color(dizzyOverlay.color.r, dizzyOverlay.color.g, dizzyOverlay.color.b, 0f);
        }
        else
        {
            StartCoroutine(DizzyRoutine());
        }
    }

    IEnumerator DizzinessEffect()
    {
        // randomize and play moaning sound
        AudioManager.Instance.PlaySoundOneShot(speakerID, "dizzy1", Random.Range(0.3f, 0.6f), Random.Range(0.85f, 1.1f));
        yield return new WaitForSeconds(2f);
        AudioManager.Instance.PlaySoundOneShot(speakerID, "ringing", 1f, 1f);
        GameManager.Instance.playerController.MoveSpeedChange(0.5f, 7f);
        //  change transparency of poison overlay flawlessly
        for (float i = 0f; i <= 0.5f; i += 0.025f)
        {
            dizzyOverlay.color = new Color(dizzyOverlay.color.r, dizzyOverlay.color.g, dizzyOverlay.color.b, i);
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(3f);

        // change transparency of poison overlay back
        for (float i = 0.5f; i >= 0f; i -= 0.025f)
        {
            dizzyOverlay.color = new Color(dizzyOverlay.color.r, dizzyOverlay.color.g, dizzyOverlay.color.b, i);
            yield return new WaitForSeconds(0.1f);
        }
        dizzyOverlay.color = new Color(dizzyOverlay.color.r, dizzyOverlay.color.g, dizzyOverlay.color.b, 0f);
    }

    public void Bleeding()
    {
        isBleeding = true;
        GameManager.Instance.DisplayMessage("You are bleeding. Use a bandage!", 3f);
        int stopBleedingAmount = Random.Range(30, 50);
        StartCoroutine(BleedingRoutine(stopBleedingAmount));
    }

    IEnumerator BleedingRoutine(int stopBleedingAmount)
    {
        yield return new WaitForSeconds(10f);
        if (isBleeding && !GameManager.Instance.isPaused)
        {
            yield return new WaitUntil(() => !GameManager.Instance.isPaused);
            AudioManager.Instance.PlaySoundOneShot(speakerID, painClips[Random.Range(0, painClips.Length)], Random.Range(0.3f, 0.5f), Random.Range(0.85f, 1.1f));
            health -= 3f;
            if (health <= stopBleedingAmount) 
            {
                isBleeding = false;
                GameManager.Instance.DisplayMessage("The bleeding stopped.", 3f);
                yield break;
            }
            StartCoroutine(BleedingRoutine(stopBleedingAmount));
        }
    }

    IEnumerator HeartbeatScreen()
    {
        while(heartbeat)
        {
            if (!GameManager.Instance.isPaused) break;
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
            maxStamina = 75f;
            if (stamina > maxStamina) stamina = 75f;
            staminaReg = 0.2f;
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
            GameManager.Instance.playerAnimController.Flashlight(flashlight.transform.parent.gameObject, flashlight.enabled);
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

        if (bulletsDisplayRoutine != null) StopCoroutine(bulletsDisplayRoutine);
        bulletsDisplayRoutine = StartCoroutine(FadeBulletsUI());
    }

    public void ShowBulletsUI(bool show)
    {
        if (!show || formerShowBool) return;
        if (bulletsDisplayRoutine != null) StopCoroutine(bulletsDisplayRoutine);
        bulletsDisplayRoutine = StartCoroutine(FadeBulletsUI());
    }


    IEnumerator FadeBulletsUI()
    {
        formerShowBool = true;
        for (float i = bulletsText.color.a; i <= 1f; i += 0.01f)
        {
            bulletsText.color = new Color(bulletsText.color.r, bulletsText.color.g, bulletsText.color.b, i);
            bulletsIcon.color = new Color(bulletsIcon.color.r, bulletsIcon.color.g, bulletsIcon.color.b, i);
            yield return new WaitForSeconds(0.01f);
        }

        yield return new WaitForSeconds(10f);
        formerShowBool = false;

        for (float i = 1f; i >= 0f; i -= 0.01f)
        {
            bulletsText.color = new Color(bulletsText.color.r, bulletsText.color.g, bulletsText.color.b, i);
            bulletsIcon.color = new Color(bulletsIcon.color.r, bulletsIcon.color.g, bulletsIcon.color.b, i);
            yield return new WaitForSeconds(0.01f);
        }
    }  

    #endregion
}