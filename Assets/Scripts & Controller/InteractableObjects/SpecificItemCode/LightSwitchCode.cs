using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightSwitchCode : InteractableObject
{
    #region Variables

    [Space(10)]
    [Header("Light & Reflection Probes")]
    [Tooltip("All Lights to turn on/off")]
    [SerializeField] private GameObject _allLights;
    [Tooltip("Reflection Probes to turn on/off")]
    [SerializeField] private GameObject _reflectionProbesLight;
    [Tooltip("Reflection Probes to turn on/off")]
    [SerializeField] private GameObject _reflectionProbesDark;

    [Space(10)]
    [Header("Doors to Unlock")]
    [SerializeField] private Door _doorToUnlock;
    [SerializeField] private Door _doorToUnlock2;

    [Space(10)]
    [Header("Further References")]
    [Tooltip("The handle to switch on the light")]
    [SerializeField] private GameObject _handleObject;
    [Tooltip("The painting event to start")]
    [SerializeField] private PaintingEvent _paintingEvent;

    private bool _eventStarted = false;

    #endregion




    #region Base Methods

    // Override the base class method for specific implementation
    protected override void RunItemCode()
    {
        if (_eventStarted) return;

        if (GameManager.Instance.EventData.CheckEvent("Light")) return;
        GameManager.Instance.EventData.SetEvent("Light");

        _paintingEvent.IsActive = true;
        _doorToUnlock.IsLocked = false;
        _doorToUnlock2.IsLocked = false;
        StartCoroutine(RotateHandleGradually());

        // Find All scripts "LightSwitch" in scene
        LightSwitch[] lightSwitches = FindObjectsOfType<LightSwitch>();
        foreach (LightSwitch lightSwitch in lightSwitches)
        {
            lightSwitch.NoCurrent = false;
        }
    }

    #endregion




    #region Coroutines

    // Coroutine to rotate the _handleObject gradually
    IEnumerator RotateHandleGradually()
    {
        AudioManager.Instance.PlayClipOneShot(gameObject.GetInstanceID(), "lever pull", 0.6f, 1f);
        AudioManager.Instance.FadeOutAudio(AudioManager.Instance.Environment, 2f);
        _eventStarted = true;
        // Rotate the _handleObject from -35° to -125°
        for (int i = 0; i < 90; i++)
        {
            _handleObject.transform.Rotate(Vector3.forward, -1);
            yield return new WaitForSeconds(0.01f);
        }
        AudioManager.Instance.PlayClipOneShot(gameObject.GetInstanceID(), "power up", 0.6f, 1f);
        StartCoroutine(TurnOnLights());
    }

    // Coroutine to turn on the lights
    IEnumerator TurnOnLights()
    {
        yield return new WaitForSeconds(1f);
        AudioManager.Instance.SetAudioClip(AudioManager.Instance.Environment, "hospital music");
        yield return new WaitForSeconds(0.1f);
        AudioManager.Instance.PlayAudio(AudioManager.Instance.Environment, 0.15f, 1f, true);
        // Turn on all the lights with a delay
        foreach (Light l in _allLights.GetComponentsInChildren<Light>())
        {
            l.enabled = true;
            yield return null;
        }

        _reflectionProbesDark.SetActive(false);
        _reflectionProbesLight.SetActive(true);
    }

    #endregion




    #region Public Methods

    // Method to start the event from the save file
    public void EventLoad()
    {
        _paintingEvent.IsActive = true;
        foreach (Light l in _allLights.GetComponentsInChildren<Light>())
        {
            l.enabled = true;
        }

        _reflectionProbesDark.SetActive(false);
        _reflectionProbesLight.SetActive(true);

        LightSwitch[] lightSwitches = FindObjectsOfType<LightSwitch>();
        foreach (LightSwitch lightSwitch in lightSwitches)
        {
            lightSwitch.NoCurrent = false;
        }
    }

    #endregion
}
