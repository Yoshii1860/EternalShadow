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
    [SerializeField] ReflectionProbe[] reflectionProbes;
    [Tooltip("The handle to switch on the light")]
    [SerializeField] GameObject handle;

    // Override the base class method for specific implementation
    protected override void RunItemCode()
    {
        StartCoroutine(RotateHandleGradually());
    }

    IEnumerator RotateHandleGradually()
    {
        // Rotate the handle from -35° to -125°
        for (int i = 0; i < 90; i++)
        {
            handle.transform.Rotate(Vector3.forward, -1);
            yield return new WaitForSeconds(0.01f);
        }

        StartCoroutine(TurnOnLights());
    }

    IEnumerator TurnOnLights()
    {
        // Turn on all the lights with a delay
        foreach (Light l in allLights.GetComponentsInChildren<Light>())
        {
            l.enabled = true;
            yield return null;
        }

        StartCoroutine(ActivateProbesGradually());
    }

    IEnumerator ActivateProbesGradually()
    {
        // Iterate through each reflection probe and render it with a delay
        for (int i = 0; i < reflectionProbes.Length; i++)
        {
            reflectionProbes[i].RenderProbe();
            yield return new WaitForSeconds(0.5f);
        }
    }
}
