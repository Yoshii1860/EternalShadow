using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveObject : MonoBehaviour
{
    TMP_InputField inputField;
    public GameObject loadCanvas;

    void Start()
    {
        inputField = loadCanvas.GetComponentInChildren<TMP_InputField>();
    }

    public void Save()
    {
        Debug.Log("SaveObject");
        string filename = inputField.text;
        GameManager.Instance.SaveData(filename);
        loadCanvas.SetActive(false);
    }
}