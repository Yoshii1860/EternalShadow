using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YardEvent : MonoBehaviour
{
    #region Serialized Fields

    [Space(10)]
    [Header("References")]
	[Tooltip("The Enemy")]
    [SerializeField] private GameObject _girl;
    [Tooltip("The main entrance Door")]
    [SerializeField] private Door _mainDoor;
    [Tooltip("The lights in the yard")]
    [Space(10)]

    [Header("Lights")]
    [SerializeField] private GameObject _yardLights;
    [Tooltip("All lights of the environment")]
    [SerializeField] private GameObject _allLights;
    [Tooltip("The main light above the enemy")]
    [SerializeField] private GameObject _mainLight;
    [Tooltip("The reflection probes inside the house")]
    [SerializeField] private GameObject _reflectionProbesLight;
    [Tooltip("The reflection probes inside the house after the event")]
    [SerializeField] private GameObject _reflectionProbesDark;
    [Tooltip("The sun object in the environment")]
    [SerializeField] private GameObject _sun;
    [Tooltip("All red lights inside and outside the house")]
    [SerializeField] private GameObject[] _alertLights;
    [Space(10)]

    [Header("Transforms")]
    [Tooltip("The target the player should look at after getting blinded")]
    [SerializeField] private Transform _lookAwayTarget;
    [Tooltip("The head object of the girl")]
    [SerializeField] private Transform _girlHead;
    [Tooltip("The position for the girl to respawn after the event")]
    [SerializeField] private Transform _girlRespawnPosition;
    [Tooltip("The position for the girl to move to after the event")]
    [SerializeField] private Transform _girlDestinationPosition;
    [Space(10)]

    [Header("Various Components")]
    [Tooltip("The music box object in the environment")]
    [SerializeField] private Musicbox _musicboxEvent;

    #endregion




    #region Event Paramters

    [Space(10)]
    [Header("Event - Increase Light Intensity")]
    [Tooltip("Intensity increase of the light over time in milliseconds")]
    [SerializeField] private int _intensityInMS = 100;
    [Tooltip("The intensity of the light when the player gets blinded")]
    [SerializeField] private int _startBlindnessIntensity = 20;
    [Tooltip("The time it takes for the player to look away after getting blinded")]
    [SerializeField] private float _blindedTime = 3.5f;

    [Space(10)]
    [Header("Event - Lights off")]
    [Tooltip("The speed of the player looking back at the girl")]
    [SerializeField] private float _lookingBackSpeed = 1f;
    [Tooltip("The pause between being unblinded and looking back")]
    [SerializeField] private float _lookingBackTime = 2f;

    [Space(10)]
    [Header("Event - End Event")]
    [Tooltip("The position the player should be moved to after the event")]
    [SerializeField] private Vector3 _playerPositionAfterEvent = new Vector3(0f, -6f, -1f);
    [Tooltip("The rotation the player should be moved to after the event")]
    [SerializeField] private Vector3 _playerRotationAfterEvent = new Vector3(0f, -90f, 0f);

    #endregion




    #region Private Fields

    private Light[] yardLightsArray;

    private int _intensityCounter = 0;
    private Vector3 _girlPositionAfterBlindness;
    private float _girlYRotationAfterBlindness = 95f;

    private bool _isFirstEnter = true;
    private bool _isSecondEnter = true;
    private bool _hasEventEnded = false;

    private List<int> _audioSourceIDList = new List<int>();

    #endregion




    #region Unity Callbacks

    private void OnTriggerExit(Collider other) 
    {
        // Check if this is the first time OnTriggerExit is called
        if (_isFirstEnter)
        {
            FirstEnter();
        }
        else if (_isSecondEnter)
        {
            SecondEnter();
        }
    }

    #endregion




    #region Private Methods

    private void FirstEnter()
    {
        // Perform one-time actions on the first exit
        _isFirstEnter = false;

        // Disable AISensor to stop AI updates during the event
        GameManager.Instance.CustomUpdateManager.RemoveCustomUpdatable(_girl.GetComponent<AISensor>());
        _girl.GetComponent<EnemyBT>().enabled = false;

        // Activate the girl GameObject
        _girl.SetActive(true);

        // Set up and play weeping ghost woman audio on the girl
        AudioManager.Instance.AddAudioSource(_girl.GetComponent<AudioSource>());
        AudioManager.Instance.SetAudioClip(_girl.GetInstanceID(), "weeping ghost woman", 0.6f, 1f, true);
        AudioManager.Instance.FadeInAudio(_girl.GetInstanceID(), 10f, 0.6f);

        // Open the associated door
        _mainDoor.OpenDoor();
    }

    private void SecondEnter()
    {
        // Check if this is the second time OnTriggerExit is called
        _isSecondEnter = false;

        // Trigger a gameplay event in the GameManager to stop player from moving
        GameManager.Instance.GameplayEvent();

        // Make the player character look at the girl
        GameManager.Instance.PlayerController.LookAtDirection(_girl.transform);
        
        // Add all flickering lights to CustomUpdateManager for dynamic updates
        FlickeringLight[] flickeringLights = _yardLights.GetComponentsInChildren<FlickeringLight>();
        foreach (FlickeringLight flickeringLight in flickeringLights)
        {
            GameManager.Instance.CustomUpdateManager.AddCustomUpdatable(flickeringLight);
        }
        
        // Add the main light to CustomUpdateManager for dynamic updates
        FlickeringLight flickeringMainLight = _mainLight.GetComponent<FlickeringLight>();
        GameManager.Instance.CustomUpdateManager.AddCustomUpdatable(flickeringMainLight);

        // Start coroutines for gradual increase of light intensity,
        // turning off lights, and ending the event
        StartCoroutine(IncreaseIntensityGradually());
        StartCoroutine(FinishEvent());
        StartCoroutine(PrepareNewScene());
    }

    /// Turn off all lights and reset the reflection probes to adjust to darkness.
    private void TurnAllLightsOff()
    {
        // Stop all audio sources associated with the lights
        for (int i = 0; i < _audioSourceIDList.Count; i++)
        {
            AudioManager.Instance.StopAudio(_audioSourceIDList[i]);
        }

        // Turn off all lights in the environment
        Light[] allLightsArray = _allLights.GetComponentsInChildren<Light>();
        for (int i = 0; i < allLightsArray.Length; i++)
        {
            // Remove flickering lights from CustomUpdateManager for efficiency
            if (allLightsArray[i].GetComponent<FlickeringLight>() != null)
            {
                GameManager.Instance.CustomUpdateManager.RemoveCustomUpdatable(allLightsArray[i].GetComponent<FlickeringLight>());
            }

            // Turn off the lights
            allLightsArray[i].enabled = false;
        }

        // Set the intensity of all yard lights and the main light back to 1
        Light[] yardLightsArray = _yardLights.GetComponentsInChildren<Light>();
        for (int i = 0; i < yardLightsArray.Length; i++)
        {
            yardLightsArray[i].intensity = 1f;
        }
        _mainLight.GetComponent<Light>().intensity = 1f;

        // Enable alert lights in the building
        for (int i = 0; i < _alertLights.Length; i++)
        {
            _alertLights[i].GetComponent<Light>().enabled = true;
        }

        // Gradually fade out the audio associated with the girl
        AudioManager.Instance.FadeOutAudio(_girl.GetInstanceID(), 0.5f);

        // Turn on the reflection probes for darkness
        _reflectionProbesLight.SetActive(false);
        _reflectionProbesDark.SetActive(true);
    }

    #endregion




    #region Coroutines

    // Enable and play audio for each yard light with random volume and pitch
    private IEnumerator FlickeringLightSounds()
    {
        yardLightsArray = _yardLights.GetComponentsInChildren<Light>();
        for (int i = 0; i < yardLightsArray.Length; i++)
        {
            yardLightsArray[i].enabled = true;
            float volume = Random.Range(0.3f, 0.8f);
            float pitch = Random.Range(0.8f, 1.2f);

            // Play audio with a delay and store audio source ID for future control
            Debug.Log("Playing audio for light " + yardLightsArray[i].gameObject.name + "...");
            bool availableSource = AudioManager.Instance.PlayAudio(yardLightsArray[i].gameObject.GetInstanceID(), volume, pitch, true);

            // Add the audio source ID to the list if it is available
            if (availableSource)
            {
                _audioSourceIDList.Add(yardLightsArray[i].gameObject.GetInstanceID());
            }

            // Introduce a random delay between each light sound for a natural effect
            yield return new WaitForSeconds(Random.Range(0.1f, 0.5f));
        }

        // Play audio for the main light with a specific clip
        AudioManager.Instance.PlayAudio(_mainLight.GetInstanceID(), 0.6f, 1f, true);
        _audioSourceIDList.Add(_mainLight.GetInstanceID());

        // Play a dark piano tension sound effect for the environment
        AudioManager.Instance.PlayClipOneShot(AudioManager.Instance.Environment, "dark piano tension", 1f, 1f);
    }

    /// Gradually increases the intensity of lights in the yard, creating an immersive atmosphere.
    private IEnumerator IncreaseIntensityGradually()
    {
        // Wait for a delay before starting the intensity increase
        yield return new WaitForSeconds(2f);

        StartCoroutine(FlickeringLightSounds());

        yield return new WaitForSeconds(1f);

        // Access the main light and its flickering component
        Light mainLightComp = _mainLight.GetComponent<Light>();
        FlickeringLight flickeringLight = mainLightComp.GetComponent<FlickeringLight>();
        flickeringLight.SmoothingFactor = 5;

        // Access the flickering lights in the yard and increment their intensity
        FlickeringLight[] flickeringLights = _yardLights.GetComponentsInChildren<FlickeringLight>();
        // Gradually increase the intensity of all yard lights and the main light
        for (int i = 0; i < _intensityInMS; i++)
        {
            for (int j = 0; j < yardLightsArray.Length; j++)
            {
                // Adjust various parameters for each light
                yardLightsArray[j].intensity += 0.05f;
                yardLightsArray[j].range += 0.05f;
                if (flickeringLights[j] != null)
                {
                    flickeringLights[j].MinIntensity += 0.015f;
                    flickeringLights[j].MaxIntensity += 0.05f;
                }

                // Increase audio volume if an AudioSource is attached
                if (yardLightsArray[j].GetComponent<AudioSource>() != null)
                {
                    yardLightsArray[j].GetComponent<AudioSource>().volume += 0.025f;
                }
            }

            // Adjust parameters for the main light
            mainLightComp.intensity += 0.05f;
            mainLightComp.range += 0.05f;
            flickeringLight.MinIntensity += 0.015f;
            flickeringLight.MaxIntensity += 0.05f;
            _intensityCounter++;

            // Trigger the start of the blindness effect when a specific intensity is reached
            if (_intensityCounter == _startBlindnessIntensity)
            {
                StartCoroutine(StartBlindness());
            }

            // Introduce a small delay between intensity increments
            yield return new WaitForSeconds(0.1f);
        }
    }

    /// Initiates the blindness animation for the player and adjusts the camera's focus direction.
    private IEnumerator StartBlindness()
    {
        // Trigger the blindness animation for the player character
        GameManager.Instance.PlayerAnimManager.BlindnessAnimation();

        // Create a temporary transform to store the lookAt position and rotation
        _girlPositionAfterBlindness = GameManager.Instance.Player.transform.position;
        _girlPositionAfterBlindness.x -= 1f;

        // Wait for the specified blinded time duration before adjusting the player's focus direction
        yield return new WaitForSeconds(_blindedTime);
        GameManager.Instance.PlayerController.ToggleArms(false);
        GameManager.Instance.PlayerController.LookAtDirection(_lookAwayTarget);
    }


    /// Manages the transition to darkness by triggering lightning, stopping blindness animation,
    /// and repositioning the girl in front of the player.
    private IEnumerator FinishEvent()
    {
        // Wait until the specified intensity increase duration is reached
        yield return new WaitUntil(() => _intensityCounter == _intensityInMS);

        // Trigger the lightning effect to darken the environment
        TurnAllLightsOff();

        yield return new WaitForSeconds(1f);

        // Stop the blindness animation for the player
        GameManager.Instance.PlayerAnimManager.StopBlindnessAnimation();

        // Manually calculate and set the position and rotation of the girl
        _girl.transform.position = _girlPositionAfterBlindness;
        _girl.transform.rotation = Quaternion.Euler(0f, _girlYRotationAfterBlindness, 0f);

        // Wait for a specified duration before initiating the next phase
        yield return new WaitForSeconds(_lookingBackTime);

        // Move the lookAtTarget slowly towards the girl's head position
        float elapsedTime = 0f;
        Vector3 startingPosition = _lookAwayTarget.position;
        Vector3 targetPosition = _girlHead.position;

        bool isOneShot = false;
        bool playOnce = false;

        // Gradually interpolate the position of lookAtTarget towards the girl's head
        while (elapsedTime < 10f)
        {
            _lookAwayTarget.position = Vector3.Lerp(startingPosition, targetPosition, elapsedTime);

            elapsedTime += Time.deltaTime * _lookingBackSpeed;

            // Trigger a one-shot animation event at a specific time for girl to wide open her mouth
            if (elapsedTime >= 0.6f && !playOnce)
            {
                playOnce = true;
                Debug.Log("Mouth Animation Triggered!");
                _girl.GetComponent<Animator>().SetTrigger("Mouth");
            }

            // Trigger a one-shot audio event and push the player back at a specific time
            if (elapsedTime >= 1f && !isOneShot)
            {
                isOneShot = true;
                AudioManager.Instance.SetAudioClip(AudioManager.Instance.Environment, "jumpscare", 1.5f, 1f, false);
                AudioManager.Instance.PlayAudio(AudioManager.Instance.Environment);
                GameManager.Instance.Player.transform.LookAt(_girl.transform.position);
                yield return new WaitForSeconds(0.1f);
                GameManager.Instance.PlayerController.PushPlayerBack();
            }

            // Stop all sounds and mark the end of the event after a certain duration
            if (elapsedTime >= 2.5f && !_hasEventEnded)
            {
                _hasEventEnded = true;
            }

            // Wait for the next frame
            yield return null;
        }
    }

    /// Manages the final phase of the event, including displaying a black screen, stopping sounds,
    /// closing the door, moving the player, and preparing for the next game state.
    private IEnumerator PrepareNewScene()
    {
        // Wait until the end event condition is met
        yield return new WaitUntil(() => _hasEventEnded);

        // Display a black screen to create a transition effect
        GameManager.Instance.Blackscreen.SetActive(true);
        GameManager.Instance.PlayerController.ToggleArms(true);
        // Close the door associated with the event
        _mainDoor.CloseDoor();

        yield return new WaitForSeconds(2f);

        AudioManager.Instance.StopAllExcept(AudioManager.Instance.Environment);

        // Wait for a short duration
        yield return new WaitForSeconds(1f);

        // Deactivate the girl GameObject nad set girl to a new destination
        _girl.SetActive(false);
        _girl.transform.position = new Vector3(-10.1878738f,-1.7069f,-13.7854099f);
        _girl.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        GameManager.Instance.CustomUpdateManager.AddCustomUpdatable(_girl.GetComponent<AISensor>());
        _girl.GetComponent<EnemyBT>().enabled = false;
        _girl.GetComponentInChildren<Collider>().enabled = false;

        // Turn off the sun
        _sun.SetActive(false);

        // Reset the player's lookAt direction to default
        GameManager.Instance.PlayerController.LookAtReset();

        // Move the player to the specified position and rotation after the event
        GameManager.Instance.PlayerController.transform.position = _playerPositionAfterEvent;
        GameManager.Instance.PlayerController.transform.rotation = Quaternion.Euler(_playerRotationAfterEvent);

        // Wait for another short duration
        yield return new WaitForSeconds(1f);

        // Fade in the black screen to transition to the next phase
        GameManager.Instance.StartCoroutine(GameManager.Instance.LoadGameWithBlackScreen());

        // Play background music associated with the next phase
        AudioManager.Instance.SetAudioClip(AudioManager.Instance.Environment, "hospital music", 0.15f, 1f, true);

        // Wait until the background music is no longer playing
        Debug.Log("Waiting for background music to stop...");
        yield return new WaitUntil(() => !AudioManager.Instance.IsPlaying(AudioManager.Instance.Environment));

        // Stop all sounds, play ambient audio, and set player's voice audio
        AudioManager.Instance.StopAll();
        AudioManager.Instance.PlayAudio(AudioManager.Instance.Environment, 0.1f, 1f, true);

        // Wait until the game state transitions to the default state
        Debug.Log("Waiting for game state to transition to default...");
        yield return new WaitUntil(() => GameManager.Instance.CurrentSubGameState == GameManager.SubGameState.DEFAULT);

        GameManager.Instance.EventData.SetEvent("Yard");

        // Start Audio for Musicbox
        GameManager.Instance.CustomUpdateManager.AddCustomUpdatable(_musicboxEvent);

        // Play the player's voice audio after a short delay
        AudioManager.Instance.PlayClipOneShot(AudioManager.Instance.PlayerSpeaker, "player3", 0.8f, 1f);
    }
    
    #endregion




    #region Public Methods

    // Load the event from the save file    
    public void EventLoad()
    {
        // Check if lights event has been triggered
        if (!GameManager.Instance.EventData.CheckEvent("Light"))
        {
            foreach (Light l in _allLights.GetComponentsInChildren<Light>())
            {
                l.enabled = false;
            }

            _reflectionProbesDark.SetActive(true);
            _reflectionProbesLight.SetActive(false);

            LightSwitch[] lightSwitches = FindObjectsOfType<LightSwitch>();
            foreach (LightSwitch lightSwitch in lightSwitches)
            {
                lightSwitch.NoCurrent = true;
            }

            for (int i = 0; i < _alertLights.Length; i++)
            {
                _alertLights[i].GetComponent<Light>().enabled = true;
            }
        }

        // Check if yard event has been triggered
        if (!GameManager.Instance.EventData.CheckEvent("Musicbox"))
        {
            GameManager.Instance.CustomUpdateManager.AddCustomUpdatable(_musicboxEvent);
        }

        // Check if bathroom event has been triggered
        if (!GameManager.Instance.EventData.CheckEvent("Bathroom"))
        {
            _girl.SetActive(false);
            _girl.transform.position = new Vector3(-10.1878738f,-1.7069f,-13.7854099f);
            _girl.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            _girl.GetComponent<EnemyBT>().enabled = false;
            _girl.GetComponentInChildren<Collider>().enabled = false;
        }
    }

    #endregion
}