using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YardEvent : MonoBehaviour
{
    #region Serialized Fields

    [Space(10)]
    [Header("Serialized Objects")]
	[Tooltip("The Enemy")]
    [SerializeField] GameObject girl;
    [Tooltip("The main entrance Door")]
    [SerializeField] Door door;
    [Tooltip("The target the player should look at after getting blinded")]
    [SerializeField] Transform lookAwayTarget;
    [Tooltip("The lights of the two lanterns")]
    [SerializeField] GameObject yardLights;
    [Tooltip("All lights of the environment")]
    [SerializeField] GameObject allLights;
    [Tooltip("The main light above the enemy")]
    [SerializeField] GameObject mainLight;
    [Tooltip("The reflection probes inside the house")]
    [SerializeField] ReflectionProbe[] reflectionProbes;
    [Tooltip("All red lights inside and outside the house")]
    [SerializeField] GameObject[] alertLights;
    [Tooltip("The head object of the girl")]
    [SerializeField] Transform girlHead;
    [Tooltip("The position for the girl to respawn after the event")]
    [SerializeField] Transform girlRespawnPosition;
    [Tooltip("The position for the girl to move to after the event")]
    [SerializeField] Transform girlDestinationPosition;
    [Tooltip("The sun object in the environment")]
    [SerializeField] GameObject sun;

    #endregion

    #region Event Paramters

    [Space(10)]
    [Header("Event - Increase Light Intensity")]
    [Tooltip("Intensity increase of the light over time in milliseconds")]
    [SerializeField] int intensityInMS = 40;
    [Tooltip("The intensity of the light when the player gets blinded")]
    [SerializeField] int startBlindness = 20;
    [Tooltip("The time it takes for the player to look away after getting blinded")]
    [SerializeField] float blindedTime = 3f;

    [Space(10)]
    [Header("Event - Lights off")]
    [Tooltip("The speed of the player looking back at the girl")]
    [SerializeField] float translationSpeed = 1f;
    [Tooltip("The pause between being unblinded and looking back")]
    [SerializeField] float lookBackTime = 2f;

    [Space(10)]
    [Header("Event - End Event")]
    [Tooltip("The position the player should be moved to after the event")]
    [SerializeField] Vector3 playerPositionAfterEvent = new Vector3(0f, -6f, -1f);
    [Tooltip("The rotation the player should be moved to after the event")]
    [SerializeField] Vector3 playerRotationAfterEvent = new Vector3(0f, -90f, 0f);

    #endregion

    #region Private Fields

    int counter = 0;
    Vector3 girlPosition;
    Vector3 girlRotation;
    bool firstTime = true;
    bool secondTime = true;
    bool endEvent = false;
    public bool musicBox = false;

    List<int> audioSourceIDList = new List<int>();

    #endregion

    #region Unity Callbacks

    void OnTriggerExit(Collider other) 
    {
        // Check if this is the first time OnTriggerExit is called
        if (firstTime)
        {
            // Perform one-time actions on the first exit
            firstTime = false;

            // Disable AISensor to stop AI updates during the event
            girl.GetComponent<AISensor>().hidden = true;

            // Activate the girl GameObject
            girl.SetActive(true);

            // Set up and play weeping ghost woman audio on the girl
            AudioManager.Instance.AddAudioSource(girl.GetComponent<AudioSource>());
            AudioManager.Instance.SetAudioClip(girl.GetInstanceID(), "weeping ghost woman", 0.6f, 1f, true);
            AudioManager.Instance.FadeIn(girl.GetInstanceID(), 10f, 0.6f);

            // Open the associated door
            door.OpenDoor();
        }
        else if (secondTime)
        {
            // Check if this is the second time OnTriggerExit is called
            secondTime = false;

            // Trigger a gameplay event in the GameManager to stop player from moving
            GameManager.Instance.GameplayEvent();

            // Make the player character look at the girl
            GameManager.Instance.playerController.LookAtDirection(girl.transform);
            
            // Add all flickering lights to CustomUpdateManager for dynamic updates
            FlickeringLight[] flickeringLights = yardLights.GetComponentsInChildren<FlickeringLight>();
            if (flickeringLights != null)
            {
                foreach (FlickeringLight fLight in flickeringLights)
                {
                    GameManager.Instance.customUpdateManager.AddCustomUpdatable(fLight);
                }
            }
            
            // Add the main light to CustomUpdateManager for dynamic updates
            FlickeringLight flickeringLight = mainLight.GetComponent<FlickeringLight>();
            GameManager.Instance.customUpdateManager.AddCustomUpdatable(flickeringLight);

            // Start coroutines for gradual increase of light intensity,
            // turning off lights, and ending the event
            StartCoroutine(IncreaseIntensityGradually());
            StartCoroutine(LightsOff());
            StartCoroutine(EndEvent());
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Turn off all lights and reset the reflection probes to adjust to darkness.
    /// </summary>
    void Lightning()
    {
        // Stop all audio sources associated with lights
        foreach (int audioSourceID in audioSourceIDList)
        {
            AudioManager.Instance.StopAudio(audioSourceID);
        }

        // Disable flickering lights and turn off all lights in the environment
        foreach (Light l in allLights.GetComponentsInChildren<Light>())
        {
            // Remove flickering lights from CustomUpdateManager for efficiency
            if (l.GetComponent<FlickeringLight>() != null)
            {
                GameManager.Instance.customUpdateManager.RemoveCustomUpdatable(l.GetComponent<FlickeringLight>());
            }

            // Turn off the light
            l.intensity = 0.5f;
            l.enabled = false;
        }

        // Enable alert lights to create a specific atmosphere
        for (int i = 0; i < alertLights.Length; i++)
        {
            alertLights[i].GetComponent<Light>().enabled = true;
        }

        // Gradually fade out the audio associated with the girl
        AudioManager.Instance.FadeOut(girl.GetInstanceID(), 0.5f);

        // Start a coroutine to activate reflection probes gradually
        StartCoroutine(ActivateProbesGradually());
    }

    #endregion

    #region Coroutines

    /// <summary>
    /// Gradually increases the intensity of lights in the yard, creating an immersive atmosphere.
    /// </summary>
    IEnumerator IncreaseIntensityGradually()
    {
        // Wait for a delay before starting the intensity increase
        yield return new WaitForSeconds(2f);

        // Enable and play audio for each yard light with random volume and pitch
        foreach (Light l in yardLights.GetComponentsInChildren<Light>())
        {
            l.enabled = true;
            float volume = Random.Range(0.3f, 0.8f);
            float pitch = Random.Range(0.8f, 1.2f);

            // Play audio with a delay and store audio source ID for future control
            bool availableSource = AudioManager.Instance.PlayAudio(l.gameObject.GetInstanceID(), volume, pitch, true);
            if (availableSource)
            {
                audioSourceIDList.Add(l.gameObject.GetInstanceID());
            }

            // Introduce a random delay between each light sound for a natural effect
            yield return new WaitForSeconds(Random.Range(0.1f, 0.5f));
        }

        // Access the main light and its flickering component
        Light mainLightComp = mainLight.GetComponent<Light>();
        FlickeringLight flickeringLight = mainLightComp.GetComponent<FlickeringLight>();
        flickeringLight.smoothing = 5;

        // Play audio for the main light with a specific clip
        AudioManager.Instance.PlayAudio(mainLight.GetInstanceID(), 0.6f, 1f, true);
        audioSourceIDList.Add(mainLight.GetInstanceID());
        AudioManager.Instance.PlaySoundOneShot(AudioManager.Instance.environment, "dark piano tension", 1f, 1f);

        // Gradually increase the intensity of all yard lights and the main light
        for (int i = 0; i < intensityInMS; i++)
        {
            foreach (Light l in yardLights.GetComponentsInChildren<Light>())
            {
                // Adjust various parameters for each light
                l.intensity += 0.1f;
                l.range += 0.1f;
                l.GetComponent<FlickeringLight>().minIntensity += 0.03f;
                l.GetComponent<FlickeringLight>().maxIntensity += 0.1f;

                // Increase audio volume if an AudioSource is attached
                if (l.GetComponent<AudioSource>() != null)
                {
                    l.GetComponent<AudioSource>().volume += 0.05f;
                }
            }

            // Adjust parameters for the main light
            mainLightComp.intensity += 0.1f;
            mainLightComp.range += 0.1f;
            flickeringLight.minIntensity += 0.03f;
            flickeringLight.maxIntensity += 0.1f;
            counter++;

            // Trigger the start of the blindness effect when a specific intensity is reached
            if (counter == startBlindness)
            {
                StartCoroutine(StartBlindness());
            }

            // Introduce a small delay between intensity increments
            yield return new WaitForSeconds(0.1f);
        }
    }

    /// <summary>
    /// Initiates the blindness animation for the player and adjusts the camera's focus direction.
    /// </summary>
    IEnumerator StartBlindness()
    {
        // Trigger the blindness animation for the player character
        GameManager.Instance.playerAnimController.BlindnessAnimation();

        // Create a temporary transform to store the lookAt position and rotation
        girlPosition = GameManager.Instance.player.transform.position;
        girlPosition.x -= 1f;

        // Wait for the specified blinded time duration before adjusting the player's focus direction
        yield return new WaitForSeconds(blindedTime);
        GameManager.Instance.playerController.LookAtDirection(lookAwayTarget);
    }


    /// <summary>
    /// Manages the transition to darkness by triggering lightning, stopping blindness animation,
    /// and repositioning the girl in front of the player.
    /// </summary>
    IEnumerator LightsOff()
    {
        // Wait until the specified intensity increase duration is reached
        yield return new WaitUntil(() => counter == intensityInMS);

        // Trigger the lightning effect to darken the environment
        Lightning();
        yield return new WaitForSeconds(1f);

        // Stop the blindness animation for the player
        GameManager.Instance.playerAnimController.StopBlindnessAnimation();

        // Manually calculate and set the position and rotation of the girl
        Debug.Log("Girl Position: " + girl.transform.position);
        Debug.Log("Setting it to: " + girlPosition);
        girl.transform.position = girlPosition;
        girl.transform.rotation = Quaternion.Euler(0f, 95f, 0f);
        Debug.Log("New position: " + girl.transform.position);

        // Wait for a specified duration before initiating the next phase
        yield return new WaitForSeconds(lookBackTime);

        // Move the lookAtTarget slowly towards the girl's head position
        float elapsedTime = 0f;
        Vector3 startingPosition = lookAwayTarget.position;
        Vector3 targetPosition = girlHead.position;

        bool oneShot = false;

        // Gradually interpolate the position of lookAtTarget towards the girl's head
        while (elapsedTime < 10f)
        {
            lookAwayTarget.position = Vector3.Lerp(startingPosition, targetPosition, elapsedTime);

            elapsedTime += Time.deltaTime * translationSpeed;

            // Trigger a one-shot audio event and push the player back at a specific time
            if (elapsedTime >= 1f && !oneShot)
            {
                oneShot = true;
                AudioManager.Instance.SetAudioClip(AudioManager.Instance.environment, "jumpscare", 1.5f, 1f, false);
                AudioManager.Instance.PlayAudio(AudioManager.Instance.environment);
                yield return new WaitForSeconds(0.1f);
                GameManager.Instance.playerController.PushPlayerBack();
            }

            // Stop all sounds and mark the end of the event after a certain duration
            if (elapsedTime >= 3f)
            {
                AudioManager.Instance.StopAllExcept(AudioManager.Instance.environment);
                endEvent = true;
            }

            // Wait for the next frame
            yield return null;
        }
    }

    /// <summary>
    /// Manages the final phase of the event, including displaying a black screen, stopping sounds,
    /// closing the door, moving the player, and preparing for the next game state.
    /// </summary>
    IEnumerator EndEvent()
    {
        // Wait until the end event condition is met
        yield return new WaitUntil(() => endEvent);

        // Display a black screen to create a transition effect
        GameManager.Instance.blackScreen.SetActive(true);

        // Close the door associated with the event
        door.CloseDoor();

        // Wait for a short duration
        yield return new WaitForSeconds(1f);

        // Deactivate the girl GameObject
        girl.GetComponent<AISensor>().hidden = false;
        girl.SetActive(false);

        // Turn off the sun
        sun.SetActive(false);

        // Reset the player's lookAt direction to default
        GameManager.Instance.playerController.LookAtReset();

        // Move the player to the specified position and rotation after the event
        GameManager.Instance.playerController.transform.position = playerPositionAfterEvent;
        GameManager.Instance.playerController.transform.rotation = Quaternion.Euler(playerRotationAfterEvent);

        // Wait for another short duration
        yield return new WaitForSeconds(1f);

        // Fade in the black screen to transition to the next phase
        GameManager.Instance.StartCoroutine(GameManager.Instance.StartGameWithBlackScreen(false));

        // Play background music associated with the next phase
        AudioManager.Instance.SetAudioClip(AudioManager.Instance.environment, "hospital music", 0.15f, 1f, true);

        // Wait until the background music is no longer playing
        Debug.Log("Waiting for background music to stop...");
        yield return new WaitUntil(() => !AudioManager.Instance.IsPlaying(AudioManager.Instance.environment));

        // Stop all sounds, play ambient audio, and set player's voice audio
        AudioManager.Instance.StopAll();
        AudioManager.Instance.PlayAudio(AudioManager.Instance.environment, 0.1f, 1f, true);
        AudioManager.Instance.SetAudioClip(AudioManager.Instance.playerSpeaker, "player3", 0.8f, 1f, false);

        // Wait until the game state transitions to the default state
        Debug.Log("Waiting for game state to transition to default...");
        yield return new WaitUntil(() => GameManager.Instance.CurrentSubGameState == GameManager.SubGameState.Default);

        // Start Audio for Musicbox
        musicBox = true;

        // Play the player's voice audio after a short delay
        AudioManager.Instance.PlayAudioWithDelay(AudioManager.Instance.playerSpeaker, 2f);
    }

    /// <summary>
    /// Gradually activates reflection probes, adjusting to the changing environment.
    /// </summary>
    IEnumerator ActivateProbesGradually()
    {
        // Iterate through each reflection probe and render it with a delay
        for (int i = 0; i < reflectionProbes.Length; i++)
        {
            reflectionProbes[i].RenderProbe();
            yield return new WaitForSeconds(0.5f);
        }
    }

    #endregion
}

