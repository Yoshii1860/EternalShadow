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

    void Start()
    {
        bonesaw.SetActive(false);
    }

    // Override the base class method for specific implementation
    protected override void RunItemCode()
    {
        Item item = InventoryManager.Instance.FindItem("Bonesaw");

        if (item != null)
        {
            InventoryManager.Instance.RemoveItem(item);
            bonesaw.SetActive(true);
            GameManager.Instance.GameplayEvent();
            GameManager.Instance.playerController.LookAtDirection(skullcap.transform.parent);
            bonesaw.GetComponent<Animator>().SetTrigger("Cut");
            StartCoroutine(CutSkull());
        }
        else
        {
            GameManager.Instance.DisplayMessage(displayMessage, 2f);
        }
    }

    IEnumerator CutSkull()
    {
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
        GameManager.Instance.playerController.LookAtReset();
        GameManager.Instance.ResumeGame();
    }
}
