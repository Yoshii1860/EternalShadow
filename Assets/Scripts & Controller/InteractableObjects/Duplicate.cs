using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Duplicate : MonoBehaviour
{
    [HideInInspector]
    public string DuplicateID;
    public GameObject DuplicateObject;

    private void Start()
    {
        DuplicateObject.GetComponent<UniqueIDComponent>().UniqueID = DuplicateID;
    }
}
