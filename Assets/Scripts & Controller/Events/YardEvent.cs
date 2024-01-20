using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YardEvent : MonoBehaviour
{
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
    [SerializeField] GameObject light;
    [Tooltip("The reflection probes inside the house")]
    [SerializeField] ReflectionProbe[] reflectionProbes;
    [Tooltip("All red lights inside and outside the house")]
    [SerializeField] GameObject[] alertLights;
    [Tooltip("The target the player should look at after getting blinded")]
    [SerializeField] Transform lookAtTarget;
    [Tooltip("The head object of the girl")]
    [SerializeField] Transform girlHead;

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

    [Space(10)]
    [Header("Sounds")]


    int counter = 0;
    Transform girlTransform;
    bool firstTime = true;
    bool secondTime = true;
    bool endEvent = false;

    List<int> audioSourceIDList = new List<int>();

    void OnTriggerExit(Collider other) 
    {
        if (firstTime)
        {
            firstTime = false;
            girl.SetActive(true);
            AudioManager.Instance.AddAudioSource(girl.GetComponent<AudioSource>());
            AudioManager.Instance.SetAudioClip(girl.GetInstanceID(), "weeping ghost woman", 0.6f, 1f, true);
            AudioManager.Instance.FadeIn(girl.GetInstanceID(), 5f, 0.6f);
            door.OpenDoor();
        }
        else if (secondTime)
        {
            secondTime = false;
            GameManager.Instance.GameplayEvent();

            GameManager.Instance.playerController.LookAtDirection(girl.transform);

            StartCoroutine(IncreaseIntensityGradually());

            StartCoroutine(LightsOff());

            StartCoroutine(EndEvent());
        }
    }

    IEnumerator IncreaseIntensityGradually()
    {
        yield return new WaitForSeconds(2f);

        foreach (Light l in yardLights.GetComponentsInChildren<Light>())
        {
            l.enabled = true;
            float volume = Random.Range(0.3f, 0.8f);
            float pitch = Random.Range(0.8f, 1.2f);
            // play audio with delay
            bool availableSource = AudioManager.Instance.PlayAudio(l.gameObject.GetInstanceID(), volume, pitch, true);
            if (availableSource)
            {
                audioSourceIDList.Add(l.gameObject.GetInstanceID());
            }
            // random delay between each light sound to make it sound more natural
            yield return new WaitForSeconds(Random.Range(0.1f, 0.5f));
        }

        Light mainLight = light.GetComponent<Light>();

        FlickeringLight flickeringLight = light.GetComponent<FlickeringLight>();
        flickeringLight.smoothing = 5;
        AudioManager.Instance.PlayAudio(light.gameObject.GetInstanceID(), 0.6f, 1f, true);
        audioSourceIDList.Add(light.gameObject.GetInstanceID());
        AudioManager.Instance.PlaySoundOneShot(AudioManager.Instance.environment, "dark piano tension", 1f, 1f, false);

        for (int i = 0; i < intensityInMS; i++)
        {
            foreach (Light l in yardLights.GetComponentsInChildren<Light>())
            {
                l.intensity += 0.1f;
                l.range += 0.1f;
                l.GetComponent<FlickeringLight>().minIntensity += 0.03f;
                l.GetComponent<FlickeringLight>().maxIntensity += 0.1f;
                if (l.GetComponent<AudioSource>() != null)
                {
                    l.GetComponent<AudioSource>().volume += 0.05f;
                }
            }
            mainLight.intensity += 0.1f;
            mainLight.range += 0.1f;
            flickeringLight.minIntensity += 0.03f;
            flickeringLight.maxIntensity += 0.1f;
            counter++;

            if (counter == startBlindness)
            {
                StartCoroutine(StartBlindness());
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator StartBlindness()
    {
        GameManager.Instance.playerAnimController.BlindnessAnimation();
        girlTransform = new GameObject("GirlTransform").transform;
        girlTransform.position = lookAtTarget.position;
        girlTransform.rotation = lookAtTarget.rotation;

        yield return new WaitForSeconds(blindedTime);
        GameManager.Instance.playerController.LookAtDirection(lookAwayTarget);
    }


    IEnumerator LightsOff()
    {
        yield return new WaitUntil(() => counter == intensityInMS);
        Lightning();
        yield return new WaitForSeconds(1f);
        GameManager.Instance.playerAnimController.StopBlindnessAnimation();

        // move girl in front of player
        //////////////////////////////////////////////////////////////////
        // manually calculated where the girl should be - BETTER SOLUTION?
        //////////////////////////////////////////////////////////////////
        girl.transform.position = new Vector3(girlTransform.position.x, girlTransform.position.y - 1.3847f, girlTransform.position.z);
        girl.transform.rotation = Quaternion.Euler(0f, girlTransform.rotation.eulerAngles.y - 180f, 0f);

        yield return new WaitForSeconds(lookBackTime);

        // move targetLookAt slowly to girl
        float elapsedTime = 0f;
        Vector3 startingPosition = lookAwayTarget.position;
        Vector3 targetPosition = girlHead.position;

        bool oneShot = false;

        while (elapsedTime < 10f)
        {
            // Interpolate the position between starting and target positions over time
            lookAwayTarget.position = Vector3.Lerp(startingPosition, targetPosition, elapsedTime);
        
            // Increment the elapsed time based on delta time
            elapsedTime += Time.deltaTime * translationSpeed;

            if (elapsedTime >= 1f && !oneShot)
            {
                oneShot = true;
                AudioManager.Instance.SetAudioClip(AudioManager.Instance.environment, "jumpscare", 1.5f, 1f, false);
                AudioManager.Instance.PlayAudio(AudioManager.Instance.environment);
            }

            if (elapsedTime >= 3f)
            {
                AudioManager.Instance.StopAllExcept(AudioManager.Instance.environment);
                endEvent = true;
            }

            // Wait for the next frame
            yield return null;
        }
    }

    // turn off all lights and reset the reflectionProbes to adjust to darkness
    void Lightning()
    {
        foreach (int audioSourceID in audioSourceIDList)
        {
            AudioManager.Instance.StopAudio(audioSourceID);
        }

        foreach (Light l in allLights.GetComponentsInChildren<Light>())
        {
            l.enabled = false;
        }

        for (int i = 0; i < alertLights.Length; i++)
        {
            alertLights[i].GetComponent<Light>().enabled = true;
        }

        AudioManager.Instance.FadeOut(girl.GetInstanceID(), 0.5f);

        StartCoroutine(ActivateProbesGradually());
    }

    IEnumerator EndEvent()
    {
        yield return new WaitUntil(() => endEvent);
        GameManager.Instance.blackScreen.SetActive(true);
        
        // stop all audios
        // GameManager.Instance.audioManager.StopAll();
        // Play Heartbeat sound

        // close door
        door.CloseDoor();

        yield return new WaitForSeconds(1f);

        // deactivate girl
        girl.SetActive(false);

        // default targetLookAt
        GameManager.Instance.playerController.LookAtReset();
        // move player to the inside
        GameManager.Instance.playerController.transform.position = playerPositionAfterEvent;
        GameManager.Instance.playerController.transform.rotation = Quaternion.Euler(playerRotationAfterEvent);

        yield return new WaitForSeconds(1f);

        // fade in blackscreen
        GameManager.Instance.StartCoroutine(GameManager.Instance.StartGameWithBlackScreen());
        AudioManager.Instance.SetAudioClip(AudioManager.Instance.environment, "hospital music", 0.15f, 1f, true);

        yield return new WaitUntil(() => !AudioManager.Instance.IsPlaying(AudioManager.Instance.environment));

        AudioManager.Instance.StopAll();
        AudioManager.Instance.PlayAudio(AudioManager.Instance.environment, 0.1f, 1f, true);
        AudioManager.Instance.SetAudioClip(AudioManager.Instance.playerSpeaker, "player3", 0.8f, 1f, false);

        yield return new WaitUntil(() => GameManager.Instance.CurrentSubGameState == GameManager.SubGameState.Default);

        AudioManager.Instance.PlayAudioWithDelay(AudioManager.Instance.playerSpeaker, 2f);
    }

    IEnumerator ActivateProbesGradually()
    {
        for (int i = 0; i < reflectionProbes.Length; i++)
        {
            reflectionProbes[i].RenderProbe();
            yield return new WaitForSeconds(0.5f);
        }
    }
}
