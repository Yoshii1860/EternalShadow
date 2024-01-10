using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YardEvent : MonoBehaviour
{
    [SerializeField] GameObject girl;
    [SerializeField] GameObject gate;
    [SerializeField] Door door;
    [SerializeField] GameObject yardLights;
    [SerializeField] GameObject allLights;
    [SerializeField] GameObject light;
    [SerializeField] ReflectionProbe[] reflectionProbes;
    [SerializeField] GameObject[] alertLights;
    bool unique = true;

    void OnTriggerExit(Collider other) 
    {
        if (unique)
        {
            unique = false;
            GameManager.Instance.GameplayEvent();
            foreach (FlickeringLight fl in yardLights.GetComponentsInChildren<FlickeringLight>())
            {
                fl.enabled = true;
            }
            light.GetComponent<FlickeringLight>().smoothing = 5;
            gate.transform.eulerAngles = new Vector3(0, 0, 0);
            StartCoroutine(RotateDoor());
        }
    }

    IEnumerator RotateDoor()
    {
        girl.GetComponent<EnemyBT>().enabled = true;
        girl.GetComponent<AISensor>().enabled = true;
        girl.GetComponent<Animator>().SetBool("Walking", true);
        yield return new WaitForSeconds(3f);
        door.CloseDoor();
        yield return new WaitForSeconds(1.5f);
        Lightning();
        girl.GetComponent<Animator>().SetBool("Walking", false);
        girl.SetActive(false);
        GameManager.Instance.ResumeGame();
    }

    // turn off all lights and reset the reflectionProbes to adjust to darkness
    void Lightning()
    {
        foreach (Light l in allLights.GetComponentsInChildren<Light>())
        {
            l.enabled = false;
        }
        light.GetComponent<Light>().color = Color.red;
        light.GetComponent<Light>().enabled = true;
        light.GetComponent<FlickeringLight>().enabled = false;
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
