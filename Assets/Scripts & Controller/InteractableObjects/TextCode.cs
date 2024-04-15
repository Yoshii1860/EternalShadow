using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextCode : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] string mainHeader;
    [SerializeField] string mainBody;
    [SerializeField] string subHeader;
    [SerializeField] string subBody;
    [Space(10)]

    [Header("Audio After Text")]
    [Tooltip("If true, audio will play after text is read.")]
    public bool audioAfterText = false;
    public string speakerClipName = "";
    public float delay = 0f;
    public bool audioPlayed = false;

    public void ReadText()
    {
        if (audioAfterText && !audioPlayed)
        {
            GameManager.Instance.textCanvas.GetComponent<TextCanvasCode>().SetAudioClip(speakerClipName, delay);
            audioPlayed = true;
        }
        GameManager.Instance.textCanvas.GetComponent<TextCanvasCode>().NewText(mainHeader, mainBody, subHeader, subBody);
    }
}
