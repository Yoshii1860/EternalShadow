using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkullCode : InteractableObject
{
    [Space(10)]
    [Header("RUN ITEM CODE")]
    [SerializeField] GameObject bonesaw;
    [SerializeField] GameObject skullcap;
    [SerializeField] Vector3 skullcapPosition; // 0, 0.022, -0.007
    [SerializeField] Vector3 skullcapRotation; // -15.45, 16.68, 102.13
    [SerializeField] float cutTimer = 4.5f;
    [SerializeField] string displayMessage;
    [SerializeField] Transform vCamFollowTarget;

    void Start()
    {
        bonesaw.SetActive(false);
    }

    // Override the base class method for specific implementation
    protected override void RunItemCode()
    {
        if (GameManager.Instance.eventData.CheckEvent("Skull")) return;

        Item item = InventoryManager.Instance.FindItem("Bonesaw");

        if (item != null)
        {
            GameManager.Instance.eventData.SetEvent("Skull");
            InventoryManager.Instance.RemoveItem(item);
            bonesaw.SetActive(true);
            GameManager.Instance.GameplayEvent();
            GameManager.Instance.playerController.ToggleArms(false);
            GameManager.Instance.playerController.SetFollowTarget(vCamFollowTarget);
            bonesaw.GetComponent<Animator>().SetTrigger("Cut");
            StartCoroutine(CutSkull());
        }
        else
        {
            GameManager.Instance.DisplayMessage(displayMessage, 2f);
        }
    }

    public void EventLoad()
    {
        bonesaw.SetActive(false);
        skullcap.transform.localPosition = new Vector3(0, 0.022f, -0.007f);
        skullcap.transform.localRotation = Quaternion.Euler(-15.45f, 16.68f, 102.13f);
        skullcap.transform.parent.GetComponent<Collider>().enabled = false;
    }

    IEnumerator CutSkull()
    {
        // properly aligning the audio with the animation
        yield return new WaitForSeconds(0.4f);
        AudioManager.Instance.PlaySoundOneShot(gameObject.GetInstanceID(), "bonesaw cut", 0.6f, 1f);
        yield return new WaitForSeconds(cutTimer);

        // translate skullcap to desired position and rotation over time
        float t = 0;
        Vector3 initialPosition = skullcap.transform.localPosition;
        Quaternion initialRotation = skullcap.transform.localRotation;

        while (t < 1)
        {
            t += Time.deltaTime;
            skullcap.transform.localPosition = Vector3.Lerp(initialPosition, skullcapPosition, t);
            skullcap.transform.localRotation = Quaternion.Lerp(initialRotation, Quaternion.Euler(skullcapRotation), t);
            yield return null;
        }

        yield return new WaitForSeconds(1f);

        bonesaw.SetActive(false);
        skullcap.transform.parent.GetComponent<Collider>().enabled = false;
        GameManager.Instance.playerController.ToggleArms(true);
        GameManager.Instance.playerController.SetFollowTarget();
        GameManager.Instance.ResumeGame();
    }
}
