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

    [Space(10)]
    [Header("Event Settings")]
    [Tooltip("Intensity increase of the light over time in milliseconds")]
    [SerializeField] int intensityInMS = 40;
    [Tooltip("The intensity of the light when the player gets blinded")]
    [SerializeField] int startBlindness = 20;
    [Tooltip("The time it takes for the player to look away after getting blinded")]
    [SerializeField] float fromBlindToLookAt = 3f;

    int counter = 0;
    [SerializeField] Transform lookAtTarget;
    [SerializeField] Transform girlHead;
    Transform girlTransform;
    float translationSpeed = 1f;

    bool unique = true;

    void OnTriggerExit(Collider other) 
    {
        if (unique)
        {
            unique = false;
            GameManager.Instance.GameplayEvent();

            GameManager.Instance.playerController.LookAtDirection(girl.transform);

            StartCoroutine(IncreaseIntensityGradually());

            StartCoroutine(LightsOff());
        }
    }

    IEnumerator IncreaseIntensityGradually()
    {
        yield return new WaitForSeconds(2f);

        foreach (Light l in yardLights.GetComponentsInChildren<Light>())
        {
            l.enabled = true;
        }

        Light mainLight = light.GetComponent<Light>();

        FlickeringLight flickeringLight = light.GetComponent<FlickeringLight>();
        flickeringLight.smoothing = 5;

        for (int i = 0; i < intensityInMS; i++)
        {
            foreach (Light l in yardLights.GetComponentsInChildren<Light>())
            {
                l.intensity += 0.1f;
                l.range += 0.1f;
                l.GetComponent<FlickeringLight>().minIntensity += 0.03f;
                l.GetComponent<FlickeringLight>().maxIntensity += 0.1f;
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

        yield return new WaitForSeconds(fromBlindToLookAt);
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
        girl.transform.position = new Vector3(girlTransform.position.x, girlTransform.position.y - 1.44398f, girlTransform.position.z);
        girl.transform.rotation = Quaternion.Euler(0f, girlTransform.rotation.eulerAngles.y + 180f, 0f);

        yield return new WaitForSeconds(2f);

        // move targetLookAt slowly to girl
        float elapsedTime = 0f;
        Vector3 startingPosition = lookAwayTarget.position;
        Vector3 targetPosition = girlHead.position;

        while (elapsedTime < 10f)
        {
            // Interpolate the position between starting and target positions over time
            lookAwayTarget.position = Vector3.Lerp(startingPosition, targetPosition, elapsedTime);
            
            // Increment the elapsed time based on delta time
            elapsedTime += Time.deltaTime * translationSpeed;

            // Wait for the next frame
            if (elapsedTime >= 6f)
            {
                // finish the movement faster
                translationSpeed = 100f;
            }
            yield return null;
        }

        // door.CloseDoor();
        // GameManager.Instance.playerController.LookAtReset();
        // GameManager.Instance.ResumeGame();
    }

    // turn off all lights and reset the reflectionProbes to adjust to darkness
    void Lightning()
    {
        foreach (Light l in allLights.GetComponentsInChildren<Light>())
        {
            l.enabled = false;
        }
        for (int i = 0; i < alertLights.Length; i++)
        {
            alertLights[i].GetComponent<Light>().enabled = true;
        }
        StartCoroutine(ActivateProbesGradually());
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
