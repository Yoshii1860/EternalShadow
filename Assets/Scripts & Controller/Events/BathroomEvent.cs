using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BathroomEvent : MonoBehaviour
{
    [SerializeField] Animator girl;

    void OnTriggerEnter(Collider other) 
    {
        if (other.CompareTag("Player"))
        {
            if (!girl.gameObject.activeSelf)
            {
                girl.gameObject.SetActive(true);
            }
            GameManager.Instance.customUpdateManager.RemoveCustomUpdatable(girl.GetComponent<AISensor>());
            girl.SetTrigger("GetOut");
            Invoke("StartGirl", 7f);
        }
    }

    void StartGirl()
    {
        GameManager.Instance.customUpdateManager.AddCustomUpdatable(girl.GetComponent<AISensor>());
        girl.GetComponent<EnemyBT>().enabled = true;
        girl.GetComponentInChildren<Collider>().enabled = true;
    }
}

