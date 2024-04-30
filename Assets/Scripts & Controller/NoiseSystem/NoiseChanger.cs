using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class NoiseChanger : MonoBehaviour
{
    #region Variables

    [SerializeField] private NoiseType _noiseType;
    private NoiseType _savedNoiseType;

    #endregion




    #region Collision

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            _savedNoiseType = other.gameObject.GetComponent<NoiseController>().GetNoiseType();
            other.gameObject.GetComponent<NoiseController>().ChangeNoiseType(_noiseType);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (_noiseType == NoiseType.TRASH || _noiseType == NoiseType.GLASS || _noiseType == NoiseType.PLASTIC || _noiseType == NoiseType.WOODEN_PLANK)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                    other.gameObject.GetComponent<NoiseController>().ChangeNoiseType(_savedNoiseType);
            }
        }
    }

    #endregion
}
