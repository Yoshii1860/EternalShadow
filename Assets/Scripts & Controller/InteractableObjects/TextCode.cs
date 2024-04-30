using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(UniqueIDComponent))]
public class TextCode : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private string _mainHeader;
    [SerializeField] private string _mainBody;
    [SerializeField] private string _subHeader;
    [SerializeField] private string _subBody;
    [Space(10)]

    [Header("Audio After Text")]
    [Tooltip("If true, audio will play after text is read.")]
    public bool HasAudioToPlay = false;
    public string SpeakerClipName = "";
    public float Delay = 0f;
    public bool IsAudioActive = true;

    public void ReadText()
    {
        if (HasAudioToPlay && IsAudioActive)
        {
            GameManager.Instance.TextCanvas.GetComponent<TextCanvasCode>().SetAudioClip(SpeakerClipName, Delay);
            IsAudioActive = false;
        }
        GameManager.Instance.TextCanvas.GetComponent<TextCanvasCode>().SetText(_mainHeader, _mainBody, _subHeader, _subBody);
    }
}
