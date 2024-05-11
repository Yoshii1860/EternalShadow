using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering.PostProcessing;

public class Player : MonoBehaviour, ICustomUpdatable
{
    #region Public Fields

    [Header("Player Stats")]
    [Tooltip("The health of the player.")]
    public float Health = 100f;
    [Tooltip("The stamina of the player.")]
    public float Stamina = 100f;
    [Tooltip("If the player is bleeding.")]
    public bool IsBleeding = false;
    [Tooltip("If the player is dizzy.")]
    public bool IsDizzy = false;
    [Tooltip("If the player is poisoned.")]
    public bool IsPoisoned = false;
    [Tooltip("If the player is out of stamina.")]
    public bool IsOutOfStamina = false;
    [Tooltip("The time it takes for stamina to be reduced.")]
    public float StaminaConsumptionTimer = 0.1f;
    [Tooltip("The time it takes for stamina to be increased.")]
    public float StaminaReg = 0.4f;
    [Tooltip("Possible max health of the player.")]
    // Also used by InventoryManager to update stats
    public float MaxHealth = 100f;
    [Tooltip("Possible max stamina of the player.")]
    // Also used by InventoryManager to update stats
    public float MaxStamina = 100f;
    [Tooltip("If the player is dead.")]
    public bool IsDead = false;
    [Space(10)]

    [Header("Player Components")]
    [Tooltip("The Flashlight of the player.")]
    public Light Flashlight;
    [Tooltip("If the player has a Flashlight.")]
    public bool IsLightAvailable = false;
    [Space(10)]

    [Header("Audio Clips")]
    [Tooltip("The audio clips when being hurt.")]
    [SerializeField] private string[] _painClips;
    [Tooltip("The audio clips when being hit.")]
    [SerializeField] private string[] _hitClips;
    [Tooltip("The audio clips when being poisoned.")]
    [SerializeField] private string[] _poisonedClips;
    [Space(10)]

    [Header("UI References")]
    [Tooltip("The blood overlay image.")]
    [SerializeField] private Image _bloodOverlayImage;
    [Tooltip("The poison overlay image.")]
    [SerializeField] private Image _poisonOverlayImage;
    [Tooltip("The dizzy overlay image.")]
    [SerializeField] private Image _dizzyOverlayImage;
    [Tooltip("The text displaying the stamina.")]
    [SerializeField] private TextMeshProUGUI _staminaText;
    [Tooltip("The text displaying the bullets.")]
    [SerializeField] private TextMeshProUGUI _bulletsText;
    [Tooltip("The icon displaying the bullets.")]
    [SerializeField] private Image _bulletsIcon;
    [Tooltip("The post process volume of the environment.")]
    [SerializeField] private PostProcessVolume _postProcessVolume;

    private ChromaticAberration _chromaticAberration;

    [Space(10)]
    [SerializeField] private bool _debugMode = false;

    #endregion




    #region Private Fields

    // Coroutine-related
    Coroutine _bulletsDisplayRoutine;

    // Stats-related
    private bool _isHealthRegenerating = false;
    private bool _isReducingStamina = false;
    private bool _isIncreasingStamina = false;
    const float MAX_HEALTH_THRESHOLD = 80f;
    const float MEDIUM_HEALTH_THRESHOLD = 60f;
    const float LOW_HEALTH_THRESHOLD = 30f;
    const float BREATHING_THRESHOLD = 60f;
    const float STRONG_BREATHING_THRESHOLD = 30f;

    // UI-related
    const float MAX_OPACITY_DIVIDER = 100f;
    const float MEDIUM_OPACITY_DIVIDER = 150f;
    const float LOW_OPACITY_DIVIDER = 200f;


    // Audio-related
    private int _speakerID;
    private int _speaker2ID;
    private bool _isBreathing = false;
    private bool _isBreathingLouder = false;
    private bool _isBreathingLoudest = false;
    private bool _isMoaning = false;
    private bool _isHeartBeating = false;

    // Timer-related
    private bool _isWaitTimerActive = false;
    private bool _isWaitTimerUpdating = false;
    private bool _isFadingInBulletsUI = false;
    private int _waitingTime = 15;

    #endregion




    #region Unity Lifecycle Methods

    // Start is called before the first frame update
    void Start()
    {
        Flashlight.enabled = false;
        _speakerID = AudioManager.Instance.PlayerSpeaker;
        _speaker2ID = AudioManager.Instance.PlayerSpeaker2;
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Characters"), LayerMask.NameToLayer("Enemies"));
        _postProcessVolume.profile.TryGetSettings(out _chromaticAberration);
    }

    // Custom update method
    public void CustomUpdate(float deltaTime)
    {
        if (IsDead) return;
        Breathing();
        LowLife();
        if (_isWaitTimerActive && !_isWaitTimerUpdating)
        {
            _isWaitTimerUpdating = true;
            StartCoroutine(WaitTimer(_waitingTime));
        }
    }

    #endregion




    #region Public Methods

    public void SetToDefault(bool isUpdateable = true)
    {
        MaxHealth = 100f;
        MaxStamina = 100f;
        Health = MaxHealth;
        Stamina = MaxStamina;
        IsDizzy = false;
        IsPoisoned = false;
        IsBleeding = false;
        IsOutOfStamina = false;
        _isBreathing = false;
        _isBreathingLouder = false;
        _isBreathingLoudest = false;
        _isMoaning = false;
        _isHeartBeating = false;
        _bloodOverlayImage.color = new Color(_bloodOverlayImage.color.r, _bloodOverlayImage.color.g, _bloodOverlayImage.color.b, 0f);
        _poisonOverlayImage.color = new Color(_poisonOverlayImage.color.r, _poisonOverlayImage.color.g, _poisonOverlayImage.color.b, 0f);
        _dizzyOverlayImage.color = new Color(_dizzyOverlayImage.color.r, _dizzyOverlayImage.color.g, _dizzyOverlayImage.color.b, 0f);
        _chromaticAberration.intensity.value = 0f;
        AudioManager.Instance.StopAudioWithDelay(_speakerID);
        AudioManager.Instance.StopAudioWithDelay(_speaker2ID);
        if (!isUpdateable) GameManager.Instance.CustomUpdateManager.RemoveCustomUpdatable(this);
    }

    #endregion




    #region Health Methods

    // Update the player's blood overlay and play sounds based on health
    private void LowLife()
    {
        // high health, turn off moaning sound and heartbeat sound, no blood overlay
    	if (Health >= MAX_HEALTH_THRESHOLD)
        {
            if (_isMoaning) _isMoaning = false;
            if (_isHeartBeating)
            {
                AudioManager.Instance.StopAudioWithDelay(_speaker2ID);
                _isHeartBeating = false;
            }

            // turn transparency of blood effect to lost health and keep the other colors
            _bloodOverlayImage.color = new Color(_bloodOverlayImage.color.r, _bloodOverlayImage.color.g, _bloodOverlayImage.color.b, 0f);

            if (_chromaticAberration.enabled.value)
            {
                _chromaticAberration.intensity.value = 0f;
                _chromaticAberration.enabled.value = false;
            }
        }
        // medium health, change blood overlay transparency
        if (Health < MAX_HEALTH_THRESHOLD && Health >= MEDIUM_HEALTH_THRESHOLD)
        {
            if (_isMoaning) _isMoaning = false;
            if (_isHeartBeating)
            {
                AudioManager.Instance.StopAudioWithDelay(_speaker2ID);
                _isHeartBeating = false;
            }

            _bloodOverlayImage.color = new Color(_bloodOverlayImage.color.r, _bloodOverlayImage.color.g, _bloodOverlayImage.color.b, ((MaxHealth - Health) / LOW_OPACITY_DIVIDER));

            if (!_chromaticAberration.enabled.value)
            {
                _chromaticAberration.enabled.value = true;
                _chromaticAberration.intensity.value = 0.2f;
            }
            else _chromaticAberration.intensity.value = 0.2f;
        }
        // low health, play moaning sound and change blood overlay transparency
        if (Health < MEDIUM_HEALTH_THRESHOLD && Health >= LOW_HEALTH_THRESHOLD)
        {
            if (!_isMoaning) _isMoaning = true;
            if (_isHeartBeating)
            {
                AudioManager.Instance.StopAudioWithDelay(_speaker2ID);
                _isHeartBeating = false;
            }

            _bloodOverlayImage.color = new Color(_bloodOverlayImage.color.r, _bloodOverlayImage.color.g, _bloodOverlayImage.color.b, ((MaxHealth - Health) / MEDIUM_OPACITY_DIVIDER));
            _chromaticAberration.intensity.value = 0.5f;

            if (!_isWaitTimerActive)
            {
                // randomize and play moaning sound
                _waitingTime = Random.Range(10, 30);
                AudioManager.Instance.PlayClipOneShot(_speakerID, _painClips[Random.Range(0, _painClips.Length)], Random.Range(0.2f, 0.4f), Random.Range(0.85f, 1.1f));
                _isWaitTimerActive = true;
            }
        }
        // very low health, play moaning sound, change blood overlay transparency and heartbeat sound
        // recover health after 10 seconds and up to 30 health
        if (Health < LOW_HEALTH_THRESHOLD)
        {
            _bloodOverlayImage.color = new Color(_bloodOverlayImage.color.r, _bloodOverlayImage.color.g, _bloodOverlayImage.color.b, ((MaxHealth - Health) / MAX_OPACITY_DIVIDER));
            _chromaticAberration.intensity.value = 1f;

            if (!_isWaitTimerActive)
            {
                // randomize moaning sound
                // play moaning sound
                _waitingTime = Random.Range(5, 20);
                AudioManager.Instance.PlayClipOneShot(_speakerID, _painClips[Random.Range(0, _painClips.Length)], Random.Range(0.4f, 0.6f), Random.Range(0.85f, 1.1f));
                _isWaitTimerActive = true;
            }
            
            // play heartbeat sound
            if (!_isHeartBeating)
            {
                AudioManager.Instance.SetAudioClip(_speaker2ID, "heartbeat");
                AudioManager.Instance.PlayAudio(_speaker2ID, 0.2f, 1f, true);
                _isHeartBeating = true;
                StartCoroutine(HeartbeatScreen());
            }

            // recover health after 10 seconds and up to 30 health
            if (!_isHealthRegenerating)
            {
                _isHealthRegenerating = true;
                StartCoroutine(RecoverHealth());
            }
        }
    }

    // Recover health over time
    IEnumerator RecoverHealth()
    {
        yield return new WaitForSeconds(5f);
        while (Health < LOW_HEALTH_THRESHOLD)
        {
            Health += 1f;
            yield return new WaitForSeconds(5f);
        }
        _isHealthRegenerating = false;
    }

    // Wait for a certain amount of time - used for different status effects
    IEnumerator WaitTimer(int timer)
    {
        yield return new WaitForSeconds(timer);
        _isWaitTimerActive = false;
        _isWaitTimerUpdating = false;
    }

    #endregion




    #region Status Methods

    //////////////////////////////////////////////////////////////////////////
    // Poison
    //////////////////////////////////////////////////////////////////////////

    // Player is poisoned, called by enemies/BTs when attacking
    public void Poisoned()
    {
        IsPoisoned = true;
        GameManager.Instance.DisplayMessage("You have an infection. Take some antibiotics!", 3f);
        OnIsPoisonedChanged();
        StartCoroutine(PoisonEffect());
        StartCoroutine(PoisonedRoutine());
    }

    // Coroutine for the poison effect
    IEnumerator PoisonEffect()
    {
        //  change transparency of poison overlay flawlessly
        while (IsPoisoned)
        {
            _waitingTime = Random.Range(20, 60);
            if (!_isWaitTimerActive && !_isMoaning)
            {
                // randomize and play moaning sound
                if (!GameManager.Instance.IsGamePaused) AudioManager.Instance.PlayClipOneShot(_speakerID, _poisonedClips[Random.Range(0, _poisonedClips.Length)], Random.Range(0.2f, 0.4f), Random.Range(0.85f, 1.1f));
                _isWaitTimerActive = true;
            }

            yield return new WaitForSeconds(2f);
            //  change transparency of poison overlay flawlessly
            for (float i = 0.018f; i <= 0.078f; i += 0.01f)
            {
                _poisonOverlayImage.color = new Color(_poisonOverlayImage.color.r, _poisonOverlayImage.color.g, _poisonOverlayImage.color.b, i);
                yield return new WaitForSeconds(0.1f);
            }

            // change transparency of poison overlay back
            for (float i = 0.078f; i >= 0.018f; i -= 0.01f)
            {
                _poisonOverlayImage.color = new Color(_poisonOverlayImage.color.r, _poisonOverlayImage.color.g, _poisonOverlayImage.color.b, i);
                yield return new WaitForSeconds(0.1f);
            }

            // undo poison effect if not poisoned anymore
            if (!IsPoisoned)
            {
                _poisonOverlayImage.color = new Color(_poisonOverlayImage.color.r, _poisonOverlayImage.color.g, _poisonOverlayImage.color.b, 0f);
            }
        }
    }

    // Coroutine for the poison effect
    IEnumerator PoisonedRoutine()
    {
        _poisonOverlayImage.color = new Color(_poisonOverlayImage.color.r, _poisonOverlayImage.color.g, _poisonOverlayImage.color.b, 0.018f);
        // randomize and play moaning sound
        AudioManager.Instance.PlayClipOneShot(_speakerID, _hitClips[Random.Range(0, _hitClips.Length)], Random.Range(0.3f, 0.6f), Random.Range(0.85f, 1.1f));
        
        StartCoroutine(PoisonEffect());

        yield return new WaitUntil (() => !IsPoisoned);
        if (_debugMode) Debug.Log("Poisoned routine ended.");
        _poisonOverlayImage.color = new Color(_poisonOverlayImage.color.r, _poisonOverlayImage.color.g, _poisonOverlayImage.color.b, 0f);
    }




    //////////////////////////////////////////////////////////////////////////
    // Dizziness
    //////////////////////////////////////////////////////////////////////////

    // Player is dizzy, called by enemies/BTs when attacking
    public void Dizzy()
    {
        GameManager.Instance.DisplayMessage("You suffer from a concussion. Take some painkiller!", 3f);
        IsDizzy = true;
        StartCoroutine(DizzinessEffect());
        StartCoroutine(DizzyRoutine());
    }

    // Coroutine for the dizziness effect
    IEnumerator DizzyRoutine()
    {
        yield return new WaitForSeconds(10f);

        // randomize and play moaning sound
        while (IsDizzy && !GameManager.Instance.IsGamePaused)
        {
            _waitingTime = Random.Range(20, 120);
            yield return new WaitForSeconds(_waitingTime);
            if (!IsDizzy) break;
            yield return new WaitUntil(() => !GameManager.Instance.IsGamePaused);
            StartCoroutine(DizzinessEffect());
            yield return new WaitForSeconds(10f);
        }

        // undo dizziness effect if not dizzy anymore
        if (!IsDizzy)
        {
            _dizzyOverlayImage.color = new Color(_dizzyOverlayImage.color.r, _dizzyOverlayImage.color.g, _dizzyOverlayImage.color.b, 0f);
        }
        // if still dizzy, repeat the routine
        else
        {
            StartCoroutine(DizzyRoutine());
        }
    }

    // Coroutine for the dizziness effect
    IEnumerator DizzinessEffect()
    {
        // randomize and play moaning sound
        AudioManager.Instance.PlayClipOneShot(_speakerID, "dizzy1", Random.Range(0.3f, 0.6f), Random.Range(0.85f, 1.1f));
        yield return new WaitForSeconds(2f);
        AudioManager.Instance.PlayClipOneShot(_speakerID, "ringing", 1f, 1f);
        GameManager.Instance.PlayerController.MoveSpeedChange(0.5f, 7f);
        //  change transparency of poison overlay flawlessly
        for (float i = 0f; i <= 0.5f; i += 0.025f)
        {
            _dizzyOverlayImage.color = new Color(_dizzyOverlayImage.color.r, _dizzyOverlayImage.color.g, _dizzyOverlayImage.color.b, i);
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(3f);

        // change transparency of poison overlay back
        for (float i = 0.5f; i >= 0f; i -= 0.025f)
        {
            _dizzyOverlayImage.color = new Color(_dizzyOverlayImage.color.r, _dizzyOverlayImage.color.g, _dizzyOverlayImage.color.b, i);
            yield return new WaitForSeconds(0.1f);
        }
        _dizzyOverlayImage.color = new Color(_dizzyOverlayImage.color.r, _dizzyOverlayImage.color.g, _dizzyOverlayImage.color.b, 0f);
    }




    //////////////////////////////////////////////////////////////////////////
    // Bleeding
    //////////////////////////////////////////////////////////////////////////

    // Player is bleeding, called by enemies/BTs when attacking
    public void Bleeding()
    {
        IsBleeding = true;
        GameManager.Instance.DisplayMessage("You are bleeding. Use a bandage!", 3f);
        int stopBleedingAmount = Random.Range(30, 50);
        StartCoroutine(BleedingRoutine(stopBleedingAmount));
    }

    // Coroutine for the bleeding effect
    IEnumerator BleedingRoutine(int stopBleedingAmount)
    {
        yield return new WaitForSeconds(10f);

        // randomize and play moaning sound, reduce health over time
        if (IsBleeding && !GameManager.Instance.IsGamePaused)
        {
            yield return new WaitUntil(() => !GameManager.Instance.IsGamePaused);
            AudioManager.Instance.PlayClipOneShot(_speakerID, _painClips[Random.Range(0, _painClips.Length)], Random.Range(0.3f, 0.5f), Random.Range(0.85f, 1.1f));
            Health -= 3f;

            // stop bleeding if health is above the stopBleedingAmount
            if (Health <= stopBleedingAmount) 
            {
                IsBleeding = false;
                GameManager.Instance.DisplayMessage("The bleeding stopped.", 3f);
                yield break;
            }
            // repeat the routine if still bleeding
            else
            {
                StartCoroutine(BleedingRoutine(stopBleedingAmount));
            }
        }
    }

    // Create heartbeat effect on blood overlay
    IEnumerator HeartbeatScreen()
    {
        while(_isHeartBeating)
        {
            if (!GameManager.Instance.IsGamePaused) break;
            // scale bloodOverlay to heartbeat
            yield return new WaitForSeconds(0.15f);
            _bloodOverlayImage.transform.localScale = new Vector3(1.01f, 1.01f, 1.01f);
            yield return new WaitForSeconds(0.5f);
            _bloodOverlayImage.transform.localScale = new Vector3(1f, 1f, 1f);
            yield return new WaitForSeconds(0.62f);
        }
    }

    #endregion




    #region Stamina Methods

    // Increase stamina over time
    public void IncreaseStamina() 
    {
        if (Stamina > MaxStamina)
        {
            return;
        }
        else if (!_isIncreasingStamina)
        {
            StartCoroutine(IncreaseStaminaOverTime());
        }
    }

    // Reduce stamina over time
    public void ReduceStamina()
    {
        if (Stamina <= 0f)
        {
            IsOutOfStamina = true;
            return;
        }
        else if (!_isReducingStamina)
        {
            StartCoroutine(ReduceStaminaOverTime());
        }
    }

    // Coroutine for reducing stamina over time
    IEnumerator ReduceStaminaOverTime()
    {
        _isReducingStamina = true;
        yield return new WaitForSeconds(StaminaConsumptionTimer);
        if (Stamina != 0f) Stamina -= 1f;
        _staminaText.text = Stamina.ToString();
        _isReducingStamina = false;
    }

    // Coroutine for increasing stamina over time
    IEnumerator IncreaseStaminaOverTime()
    {
        _isIncreasingStamina = true;
        yield return new WaitForSeconds(StaminaReg);
        if (Stamina < MaxStamina) Stamina += 1f;
        _staminaText.text = Stamina.ToString();
        _isIncreasingStamina = false;
    }

    // Play breathing sounds based on stamina
    void Breathing()
    {
        // high stamina, turn off breathing sounds
        if (Stamina > BREATHING_THRESHOLD)
        {
            return;
        }
        // medium stamina, play breathing sound
        else if (   Stamina < BREATHING_THRESHOLD 
                &&  Stamina > LOW_HEALTH_THRESHOLD
                &&  !_isBreathing)
        {
            if (!_isMoaning) 
            {
                AudioManager.Instance.SetAudioClip(_speakerID, "breathing");
                AudioManager.Instance.PlayAudio(_speakerID, 0.4f, 1f, true);
            }
            if (_debugMode) Debug.Log("Playing breathing");
            _isBreathing = true;
            _isBreathingLouder = false;
            _isBreathingLoudest = false;
            StartCoroutine(StopBreathing());
            return;
        }
        // low stamina, play breathing louder sound
        else if (   Stamina < STRONG_BREATHING_THRESHOLD 
                &&  !IsOutOfStamina
                &&  !_isBreathingLouder)
        {
            if (!_isMoaning)
            {
                AudioManager.Instance.SetAudioClip(_speakerID, "breathing");
                AudioManager.Instance.PlayAudio(_speakerID, 0.7f, 1f, true);
            }
            if (_debugMode) Debug.Log("Playing breathing louder");
            _isBreathing = false;
            _isBreathingLouder = true;
            _isBreathingLoudest = false;
            return;
        }
        // out of stamina, play breathing loudest sound
        else if (   IsOutOfStamina
                &&  !_isBreathingLoudest)
        {
            if (!_isMoaning)
            {
                AudioManager.Instance.SetAudioClip(_speakerID, "breathing");
                AudioManager.Instance.PlayAudio(_speakerID, 1.1f, 1f, true);
            }
            if (_debugMode) Debug.Log("Playing breathing loudest");
            _isBreathing = false;
            _isBreathingLouder = false;
            _isBreathingLoudest = true;
            return;
        }
    }

    // Coroutine for stopping breathing sounds
    IEnumerator StopBreathing()
    {
        yield return new WaitUntil(() => Stamina > BREATHING_THRESHOLD);
        AudioManager.Instance.StopAudioWithDelay(_speakerID);
        if (_debugMode) Debug.Log("Stopping breathing");
        _isBreathing = false;
        _isBreathingLouder = false;
        _isBreathingLoudest = false;
    }

    #endregion




    #region Property Changing Methods

    // Toggle the stats of the player based on the poison status
    void OnIsPoisonedChanged()
    {
        if (IsPoisoned)
        {
            MaxStamina = 75f;
            if (Stamina > MaxStamina) Stamina = 75f;
            StaminaReg = 0.2f;
            _staminaText.text = Stamina.ToString();
        }
        else
        {
            MaxStamina = 100f;
            StaminaReg = 0.4f;
            _staminaText.text = Stamina.ToString();
        }
    }

    #endregion




    #region Player Actions

    // Switch the flashlight on and off
    public void LightSwitch()
    {
        if (IsLightAvailable)
        {
            Flashlight.enabled = !Flashlight.enabled;
            GameManager.Instance.PlayerAnimManager.Flashlight(Flashlight.transform.parent.gameObject, Flashlight.enabled);
            if (_debugMode) Debug.Log("Light is available");
        }
        else
        {
            if (_debugMode) Debug.Log("Light is not available");
        }
    }

    // Player takes damage, usually called by enemies/BTs
    public bool TakeDamage(float damage)
    {
        Health -= damage;
        if (_debugMode) Debug.Log("Player took " + damage + " damage. " + Health + " health left.");
        bool IsDead = Health <= 0;
        if (IsDead) Die();
        return IsDead;
    }

    // Player dies and starts the death routine
    private void Die()
    {
        if (IsDead) return;
        IsDead = true;
        IsDizzy = false;
        IsPoisoned = false;
        IsBleeding = false;
        StartCoroutine(DieRoutine());
        if (_debugMode) Debug.Log("Player died.");
    }

    // Death routine of the player
    IEnumerator DieRoutine()
    {
        GameManager.Instance.GameplayEvent();
        AudioManager.Instance.PlayClipOneShot(_speakerID, "player die", 1f, 1f);

        yield return new WaitForSeconds(1f);

        // fade out all audio
        AudioManager.Instance.FadeOutAll(5f);

        // fade in black screen
        Image blackscreenImage = GameManager.Instance.Blackscreen.GetComponent<Image>();
        blackscreenImage.color = new Color(0f, 0f, 0f, 0f);
        GameManager.Instance.Blackscreen.gameObject.SetActive(true);
        for (float i = 0f; i <= 1f; i += 0.005f)
        {
            blackscreenImage.color = new Color(0f, 0f, 0f, i);
            yield return new WaitForSeconds(0.01f);
        }

        SetToDefault(false);

        // play death music and open death screen
        AudioManager.Instance.SetAudioClip(AudioManager.Instance.Environment, "death music", 0.5f, 1f);
        GameManager.Instance.OpenDeathScreen();

        yield return new WaitForSeconds(0.5f);

        // fade out black screen
        GameManager.Instance.Blackscreen.GetComponent<Image>().color = new Color(0f, 0f, 0f, 1f);
    }

    #endregion




    #region UI Methods

    // Set the bullets UI
    public void SetBulletsUI(int currentBullets, int bulletsInInventory)
    {
        _bulletsText.text = currentBullets + "/" + bulletsInInventory;

        if (_bulletsDisplayRoutine != null) StopCoroutine(_bulletsDisplayRoutine);
        _bulletsDisplayRoutine = StartCoroutine(FadeBulletsUI());
    }

    // Show the bullets UI
    public void ShowBulletsUI(bool show)
    {
        if (!show || _isFadingInBulletsUI) return;
        if (_bulletsDisplayRoutine != null) StopCoroutine(_bulletsDisplayRoutine);
        _bulletsDisplayRoutine = StartCoroutine(FadeBulletsUI());
    }

    // Fade the bullets UI in and out
    IEnumerator FadeBulletsUI()
    {
        _isFadingInBulletsUI = true;
        for (float i = _bulletsText.color.a; i <= 1f; i += 0.01f)
        {
            _bulletsText.color = new Color(_bulletsText.color.r, _bulletsText.color.g, _bulletsText.color.b, i);
            _bulletsIcon.color = new Color(_bulletsIcon.color.r, _bulletsIcon.color.g, _bulletsIcon.color.b, i);
            yield return new WaitForSeconds(0.01f);
        }

        yield return new WaitForSeconds(10f);
        _isFadingInBulletsUI = false;

        for (float i = 1f; i >= 0f; i -= 0.01f)
        {
            _bulletsText.color = new Color(_bulletsText.color.r, _bulletsText.color.g, _bulletsText.color.b, i);
            _bulletsIcon.color = new Color(_bulletsIcon.color.r, _bulletsIcon.color.g, _bulletsIcon.color.b, i);
            yield return new WaitForSeconds(0.01f);
        }
    }  

    #endregion
}