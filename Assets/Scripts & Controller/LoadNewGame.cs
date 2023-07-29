using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LoadNewGame : MonoBehaviour
{
    public void OnLoadNewGame()
    {
        GameManager.Instance.LoadNewGame();
    }

    public void OnLoadGame()
    {
        RectTransform parentTransform = EventSystem.current.currentSelectedGameObject.transform.parent.GetComponent<RectTransform>();;
        GameObject parent = parentTransform.GetComponentInChildren<LoadObject>(true).gameObject;
        parent.SetActive(true);
    }
}
