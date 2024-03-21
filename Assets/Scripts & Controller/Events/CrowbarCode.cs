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
    }

    public void BreakSecond()
    {
        secondRailing.SetTrigger("Fall");
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
