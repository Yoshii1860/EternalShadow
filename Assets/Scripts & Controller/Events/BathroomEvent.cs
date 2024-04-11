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
            GetComponent<Collider>().enabled = false;
            StartCoroutine(StartEvent());
        }
    }

    IEnumerator StartEvent()
    {
        if (!girl.gameObject.activeSelf) girl.gameObject.SetActive(true);
        AudioManager.Instance.FadeOut(AudioManager.Instance.environment, 2f);
        GameManager.Instance.customUpdateManager.RemoveCustomUpdatable(girl.GetComponent<AISensor>());
        girl.SetTrigger("GetOut");
        GameManager.Instance.eventData.CheckEvent("Bathroom");
        yield return new WaitForSeconds(3f);
        AudioManager.Instance.SetAudioClip(AudioManager.Instance.environment, "horror chase music 2");
        AudioManager.Instance.PlayAudio(AudioManager.Instance.environment, 0.30f, 1f, true);
        yield return new WaitForSeconds(4f);
        StartGirl();
    }

    void StartGirl()
    {
        GameManager.Instance.customUpdateManager.AddCustomUpdatable(girl.GetComponent<AISensor>());
        girl.GetComponent<EnemyBT>().enabled = true;
        girl.GetComponentInChildren<Collider>().enabled = true;
    }

    public void EventLoad()
    {
        girl.GetComponent<EnemyBT>().enabled = true;
        girl.GetComponentInChildren<Collider>().enabled = true;
        GetComponent<Collider>().enabled = false;
    }
}

