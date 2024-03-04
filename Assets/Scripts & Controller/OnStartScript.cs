using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnStartScript : MonoBehaviour
{
    [SerializeField] GameObject girl;
    [SerializeField] GameObject slender;
    
    // Start is called before the first frame update
    void Start()
    {
        girl.SetActive(false);
        slender.SetActive(false);
        GameManager.Instance.StartGame();
    }
}
