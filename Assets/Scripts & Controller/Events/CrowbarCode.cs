using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrowbarCode : MonoBehaviour
{
    [SerializeField] Animator firstRailing;
    [SerializeField] Animator secondRailing;
    [SerializeField] MirrorCode mirrorCode;

    public void BreakFirst()
    {
        firstRailing.SetTrigger("Fall");
        AudioManager.Instance.PlaySoundOneShot(gameObject.GetInstanceID(), "rails removal 2", 0.6f, 1f);
    }

    public void BreakSecond()
    {
        secondRailing.SetTrigger("Fall");
        AudioManager.Instance.PlaySoundOneShot(gameObject.GetInstanceID(), "rails removal 2", 0.6f, 1f);
    }

    public void RemovalAudio()
    {
        AudioManager.Instance.PlaySoundOneShot(gameObject.GetInstanceID(), "rails removal 1", 0.6f, 1f);
    }

    public void DeactivateCrowbar()
    {
        firstRailing.transform.GetComponentInChildren<Collider>().enabled = false;
        secondRailing.transform.GetComponentInChildren<Collider>().enabled = false;
        GameManager.Instance.playerController.SetFollowTarget();
        GameManager.Instance.ResumeGame();
        mirrorCode.enabled = true;
        GameManager.Instance.customUpdateManager.AddCustomUpdatable(mirrorCode);
        gameObject.SetActive(false);
    }
}
