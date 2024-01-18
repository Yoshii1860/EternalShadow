using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Player : MonoBehaviour, ICustomUpdatable
{
    [Tooltip("The health of the player.")]
    public int health = 100;
    [Tooltip("The stamina of the player.")]
    public int stamina = 100;
    [Tooltip("The regeneration in seconds of the player.")]
    public int healthReg = 10;
    [Tooltip("If the player is bleeding - reduces health over time.")]
    public bool isBleeding = false;
    [Tooltip("Field for Debugging isPoisoned bool.")]
    public bool isPoisonedDebug = false;
    public bool isNotPoisonedDebug = false;
    [Tooltip("If the player is dizzy - stops at least once a minute for 3 seconds.")]
    public bool isDizzy = false;
    [Tooltip("If the player is out of stamina - walk slow during 3 seconds.")]
    public bool isOutOfStamina = false;
    [Tooltip("The stamina consumption timer of the player.")]
    public float staminaConsumptionTimer = 0.1f;
    [Tooltip("The stamina regeneration timer of the player.")]
    public float staminaReg = 0.4f;
    [Tooltip("The flashlight of the player.")]
    [SerializeField] Light flashlight;
    [Tooltip("If the player has a light source.")]
    public bool lightAvail = false;

    [Space(10)]
    [Header("References")]
    [SerializeField] TextMeshProUGUI healthText;
    [SerializeField] TextMeshProUGUI staminaText;
    [SerializeField] TextMeshProUGUI bulletsText;
    [SerializeField] GameObject playerSpeaker;

    private int reducedStamina;
    private int maxHealth = 100;
    private int maxStamina = 100;

    bool reducingStamina = false;
    bool increasingStamina = false;

    bool isPlaying = false;
    int speakerID;
    bool breath = false;
    bool breathLouder = false;
    bool breathLoudest = false;


    void Update() 
    {
/////////////////////////////////////
// DEBUG STATEMENT - REMOVE LATER //
/////////////////////////////////////
        if(isPoisonedDebug)
        {
            isPoisoned = true;
        }
        if(isNotPoisonedDebug)
        {
            isPoisoned = false;
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
/////////////////////////////////////

    void Start()
    {
        flashlight.enabled = false;
        speakerID = playerSpeaker.GetInstanceID();
    }

    public void CustomUpdate(float deltaTime)
    {
        Breathing();
    }

    void Breathing()
    {
        if (stamina > 60)
        {
            return;
        }
        else if (   stamina < 60 
                &&  stamina > 30
                &&  !breath)
        {
            AudioManager.Instance.SetAudioClip(speakerID, "breathing");
            AudioManager.Instance.PlayAudio(speakerID, 0.4f, 1f, true);
            Debug.Log("Playing breathing");
            breath = true;
            breathLouder = false;
            breathLoudest = false;
            StartCoroutine(StopBreathing());
            return;
        }
        else if (   stamina < 30 
                &&  !isOutOfStamina
                &&  !breathLouder)
        {
            AudioManager.Instance.SetAudioVolume(speakerID, 0.7f);
            Debug.Log("Playing breathing louder");
            breath = false;
            breathLouder = true;
            breathLoudest = false;
            return;
        }
        else if (   isOutOfStamina
                &&  !breathLoudest)
        {
            AudioManager.Instance.SetAudioVolume(speakerID, 1.1f);
            Debug.Log("Playing breathing loudest");
            breath = false;
            breathLouder = false;
            breathLoudest = true;
            return;
        }
    }

    IEnumerator StopBreathing()
    {
        yield return new WaitUntil(() => stamina > 60);
        AudioManager.Instance.StopAudioWithDelay(speakerID);
        Debug.Log("Stopping breathing");
        breath = false;
        breathLouder = false;
        breathLoudest = false;
    }

    void OnIsPoisonedChanged()
    {
        if (isPoisoned)
        {
            maxStamina = 80;
            if (stamina > maxStamina) stamina = 80;
            staminaReg = 0.3f;
            staminaText.text = stamina.ToString();
        }
        else
        {
            maxStamina = 100;
            staminaReg = 0.4f;
            staminaText.text = stamina.ToString();
        }
    }

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

    public bool TakeDamage()
    {
        health -= 10;
        healthText.text = health.ToString();
        bool isDead = health <= 0;
        if (isDead) Die();
        return isDead;
    }

    private void Die()
    {
        Destroy(transform.GetComponent<CapsuleCollider>());
        Debug.Log("Player died.");
    }

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
        if (stamina <= 0)
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
        if (stamina != 0) stamina -= 1;
        staminaText.text = stamina.ToString();
        reducingStamina = false;
    }

    IEnumerator IncreaseStaminaOverTime()
    {
        increasingStamina = true;
        yield return new WaitForSeconds(staminaReg);
        if (stamina < maxStamina) stamina += 1;
        staminaText.text = stamina.ToString();
        increasingStamina = false;
    }

    public void SetBulletsUI(int currentBullets, int bulletsInInventory)
    {
        bulletsText.text = currentBullets + "/" + bulletsInInventory;
    }
}