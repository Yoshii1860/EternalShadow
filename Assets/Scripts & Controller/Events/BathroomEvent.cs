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
            AudioManager.Instance.FadeOut(AudioManager.Instance.environment, 2f);
            Invoke("PlayNewAudio", 2f);
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

    void PlayNewAudio()
    {
        AudioManager.Instance.SetAudioClip(AudioManager.Instance.environment, "horror chase music 2");
        AudioManager.Instance.PlayAudio(AudioManager.Instance.environment, 0.35f, 1f, true);
    }
}

