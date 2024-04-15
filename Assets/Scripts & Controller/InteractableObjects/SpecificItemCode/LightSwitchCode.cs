using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightSwitchCode : InteractableObject
{
    [Space(10)]
    [Header("RUN ITEM CODE")]
    [Tooltip("All Lights to turn on/off")]
    [SerializeField] GameObject allLights;
    [Tooltip("The door to unlock")]
    [SerializeField] GameObject reflectionProbes;
    [SerializeField] GameObject reflectionProbesDark;
    [Tooltip("The handle to switch on the light")]
    [SerializeField] GameObject handle;
    [SerializeField] PaintingEvent paintingEvent;
    [SerializeField] Door doorToUnlock;
    [SerializeField] Door doorToUnlock2;
    private bool eventStarted = false;

    // Override the base class method for specific implementation
    protected override void RunItemCode()
    {
        if (eventStarted) return;
        if (GameManager.Instance.eventData.CheckEvent("Light")) return;
        GameManager.Instance.eventData.SetEvent("Light");
        paintingEvent.active = true;
        doorToUnlock.locked = false;
        doorToUnlock2.locked = false;
        StartCoroutine(RotateHandleGradually());

        // Find All scripts "LightSwitch" in scene
        LightSwitch[] lightSwitches = FindObjectsOfType<LightSwitch>();
        foreach (LightSwitch lightSwitch in lightSwitches)
        {
            lightSwitch.noCurrent = false;
        }
    }

    public void EventLoad()
    {
        paintingEvent.active = true;
        foreach (Light l in allLights.GetComponentsInChildren<Light>())
        {
            l.enabled = true;
        }

        reflectionProbesDark.SetActive(false);
        reflectionProbes.SetActive(true);

        LightSwitch[] lightSwitches = FindObjectsOfType<LightSwitch>();
        foreach (LightSwitch lightSwitch in lightSwitches)
        {
            lightSwitch.noCurrent = false;
        }
    }

    IEnumerator RotateHandleGradually()
    {
        AudioManager.Instance.PlaySoundOneShot(gameObject.GetInstanceID(), "lever pull", 0.6f, 1f);
        AudioManager.Instance.FadeOut(AudioManager.Instance.environment, 2f);
        eventStarted = true;
        // Rotate the handle from -35° to -125°
        for (int i = 0; i < 90; i++)
        {
            handle.transform.Rotate(Vector3.forward, -1);
            yield return new WaitForSeconds(0.01f);
        }
        AudioManager.Instance.PlaySoundOneShot(gameObject.GetInstanceID(), "power up", 0.6f, 1f);
        StartCoroutine(TurnOnLights());
    }

    IEnumerator TurnOnLights()
    {
        yield return new WaitForSeconds(1f);
        AudioManager.Instance.SetAudioClip(AudioManager.Instance.environment, "hospital music");
        yield return new WaitForSeconds(0.1f);
        AudioManager.Instance.PlayAudio(AudioManager.Instance.environment, 0.15f, 1f, true);
        // Turn on all the lights with a delay
        foreach (Light l in allLights.GetComponentsInChildren<Light>())
        {
            l.enabled = true;
            yield return null;
        }

        reflectionProbesDark.SetActive(false);
        reflectionProbes.SetActive(true);
    }
}
