using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintingText : InteractableObject
{
    [Space(10)]
    [Header("RUN ITEM CODE")]
    [SerializeField] GameObject canvas;

    protected override void RunItemCode()
    {
        GameManager.Instance.playerController.ToggleArms(false);
        GameManager.Instance.PickUp();
        canvas.SetActive(true);
        GameManager.Instance.canvasActive = true;
        StartCoroutine(ToggleText());
    }

    IEnumerator ToggleText()
    {
        yield return new WaitUntil(() => GameManager.Instance.CurrentSubGameState == GameManager.SubGameState.Default);
        canvas.SetActive(false);
        GameManager.Instance.canvasActive = false;
        GameManager.Instance.playerController.ToggleArms(true);
    }
}
