using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class TextCanvasCode : MonoBehaviour
{   
    #region Variables

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI _mainHeader;
    [SerializeField] private TextMeshProUGUI _mainBody;
    [SerializeField] private TextMeshProUGUI _subHeader;
    [SerializeField] private TextMeshProUGUI _subBody;

    private string _mainHeaderText;
    private string _mainBodyText;
    private string _subHeaderText;
    private string _subBodyText;

    // Audio
    [HideInInspector]
    public string SpeakerClipName = "";
    [HideInInspector]
    public float Delay = 0f;

    private bool IsAudioReady = false;

    private GameObject _panel;

    #endregion




    #region Unity Methods

    private void Start()
    {
        _panel = transform.GetChild(0).gameObject;
        _panel.SetActive(false);
    }

    #endregion




    #region Public Methods

    // function to set text - used by Text Code Scripts
    public void SetText(string newHeader, string newBody, string newSubHeader = "", string newSubBody = "")
    {
        if (!_panel.activeSelf) _panel.SetActive(true);

        _mainHeaderText = newHeader;
        _mainBodyText = newBody;
        _subHeaderText = newSubHeader;
        _subBodyText = newSubBody;

        UpdateTextVisibility();
        SetTexts();
        StartCoroutine(ToggleText());
    }

    // function to set audio clip - used by Text Code Scripts
    public void SetAudioClip(string clipName, float delay)
    {
        SpeakerClipName = clipName;
        this.Delay = delay;
        IsAudioReady = true;
    }

    #endregion




    #region Private Methods

    // function to update text visibility depending on text presence
    private void UpdateTextVisibility()
    {
        _subHeader.gameObject.SetActive(_subHeaderText != string.Empty && _subBodyText != string.Empty);
        _subBody.gameObject.SetActive(_subHeaderText != string.Empty && _subBodyText != string.Empty);
    }


    // function to set up text
    private void SetTexts()
    {
        // Fix new lines
        _mainBodyText = _mainBodyText.Replace("\\n", "\n");

        // Adjust main body position based on sub-text presence
        _mainBody.rectTransform.offsetMin = new Vector2(_mainBody.rectTransform.offsetMin.x, _subHeaderText == string.Empty ? 90 : 350);
        _mainBody.rectTransform.offsetMax = new Vector2(_mainBody.rectTransform.offsetMax.x, -140);

        _mainHeader.text = _mainHeaderText;
        _mainBody.text = _mainBodyText;

        if (_subHeaderText != string.Empty)
        {
            _subBodyText = _subBodyText.Replace("\\n", "\n");
            _subHeader.text = _subHeaderText;
            _subBody.text = _subBodyText;
        }

        // Adjust main body position based on sub-text presence
        _mainBody.rectTransform.offsetMax = new Vector2(_mainBody.rectTransform.offsetMax.x, _mainHeaderText == string.Empty ? -80 : -140);

        // Toogle arm visibility, change to pickup state and set text active
        GameManager.Instance.PlayerController.ToggleArms(false);
        GameManager.Instance.PickUp();
        gameObject.SetActive(true);
        GameManager.Instance.IsCanvasActive = true;
        StartCoroutine(ToggleText());
    }

    #endregion




    #region Coroutines

    // function to run text toggle
    IEnumerator ToggleText()
    {
        yield return new WaitUntil(() => GameManager.Instance.CurrentSubGameState == GameManager.SubGameState.DEFAULT);
        if (IsAudioReady)
        {
            AudioManager.Instance.PlayClipOneShotWithDelay(AudioManager.Instance.PlayerSpeaker2, SpeakerClipName, Delay);
            IsAudioReady = false;
        }
        gameObject.SetActive(false);
        GameManager.Instance.PlayerController.ToggleArms(true);
    }

    #endregion
}