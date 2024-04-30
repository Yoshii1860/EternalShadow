using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintingCode : MonoBehaviour
{
    #region Variables

    [Header("Painting Variables")]
    [Tooltip("Unique number for each painting in the list")]
    public int PaintingNumber;
    [Tooltip("The light that will highlight the painting")]
    public GameObject SpotLight;

    #endregion




    #region Unity Methods

    // set the painting number to the index of the painting in the list
    private void Start()
    {
        PaintingNumber = transform.GetSiblingIndex();
        SpotLight = transform.GetComponentInChildren<Light>().gameObject;
        SpotLight.SetActive(false);
    }

    #endregion
}
