using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class NoiseChanger : MonoBehaviour
{
    [SerializeField] NoiseType noiseType;
    NoiseType savedNoiseType;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Noise Changer Triggered: " + noiseType.ToString());
            savedNoiseType = other.gameObject.GetComponent<NoiseController>().GetNoiseType();
            other.gameObject.GetComponent<NoiseController>().ChangeNoiseType(noiseType);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (noiseType == NoiseType.TRASH || noiseType == NoiseType.GLASS || noiseType == NoiseType.PLASTIC || noiseType == NoiseType.WOODEN_PLANK)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                    other.gameObject.GetComponent<NoiseController>().ChangeNoiseType(savedNoiseType);
            }
        }
    }
}
