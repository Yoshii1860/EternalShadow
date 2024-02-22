using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintingCode : MonoBehaviour
{
    [Header("Painting Variables")]
    [Tooltip("Unique number for each painting in the list")]
    public int paintingNumber;
    [Tooltip("The light that will highlight the painting")]
    public GameObject spotLight;

    // set the painting number to the index of the painting in the list
    void Start()
    {
        paintingNumber = transform.GetSiblingIndex();
        spotLight = transform.GetComponentInChildren<Light>().gameObject;
        spotLight.SetActive(false);
    }
}
