using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Duplicate : MonoBehaviour
{
    public string duplicateID;
    public GameObject duplicateObject;

    void Start()
    {
        duplicateID = duplicateObject.GetComponent<UniqueIDComponent>().UniqueID;
    }
}
