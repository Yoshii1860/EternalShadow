using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnStartScript : MonoBehaviour
{
    [SerializeField] GameObject girl;
    
    // Start is called before the first frame update
    void Start()
    {
        girl.SetActive(false);
    }
}
