using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class LightSwitch : MonoBehaviour
{
    [SerializeField] Light[] lights;
    [SerializeField] Transform lightSwitch;
    float switchOff = -30f;
    float switchOn = 0;
    public bool noCurrent = true;

    void Start()
    {
        GetComponent<AudioSource>().spatialBlend = 1;
        GetComponent<AudioSource>().rolloffMode = AudioRolloffMode.Custom;
    }

    public void SwitchLights()
    {
        if (noCurrent || lights[0] == null) return;

        if (lights[0].enabled) 
        {
            lightSwitch.localEulerAngles = new Vector3(0, 0, switchOff);
            AudioManager.Instance.PlaySoundOneShot(gameObject.GetInstanceID(), "light off", 0.6f);
        }
        else 
        {
            lightSwitch.localEulerAngles = new Vector3(0, 0, switchOn);
            AudioManager.Instance.PlaySoundOneShot(gameObject.GetInstanceID(), "light on", 0.6f);
        }

        foreach (Light light in lights)
        {
            light.enabled = !light.enabled;
        }
    }
}
