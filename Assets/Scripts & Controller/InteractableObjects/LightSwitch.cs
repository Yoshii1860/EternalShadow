using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class LightSwitch : MonoBehaviour
{
    #region Variables

    [Tooltip("All lights in the room")]
    [SerializeField] Light[] _lights;
    [Tooltip("The switch that turns the lights on and off")]
    [SerializeField] private Transform _lightSwitch;
    private float _switchOff = -30f;
    private float _switchOn = 0;

    [Tooltip("Is there a current in the room?")]
    public bool NoCurrent = true; // controlled by switch in upper floor office

    #endregion




    #region Public Methods

    public void SwitchLights()
    {
        // if there is no current in the room, the lights cannot be turned on
        if (NoCurrent || _lights[0] == null) return;

        // rotate the switch
        if (_lights[0].enabled) 
        {
            _lightSwitch.localEulerAngles = new Vector3(0, 0, _switchOff);
            AudioManager.Instance.PlayClipOneShot(gameObject.GetInstanceID(), "light off", 0.6f);
        }
        else 
        {
            _lightSwitch.localEulerAngles = new Vector3(0, 0, _switchOn);
            AudioManager.Instance.PlayClipOneShot(gameObject.GetInstanceID(), "light on", 0.6f);
        }

        // toggle the lights
        foreach (Light light in _lights)
        {
            light.enabled = !light.enabled;
        }
    }

    #endregion
}
