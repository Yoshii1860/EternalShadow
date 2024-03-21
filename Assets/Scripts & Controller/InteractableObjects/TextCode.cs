using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextCode : MonoBehaviour
{
    [SerializeField] string mainHeader;
    [SerializeField] string mainBody;
    [SerializeField] string subHeader;
    [SerializeField] string subBody;

    public void ReadText()
    {
        GameManager.Instance.textCanvas.GetComponent<TextCanvasCode>().NewText(mainHeader, mainBody, subHeader, subBody);
    }
}
