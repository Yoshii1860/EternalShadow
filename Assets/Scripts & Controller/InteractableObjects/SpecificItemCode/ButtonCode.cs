using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ButtonCode : InteractableObject
{
    #region Variables

    [Space(10)]
    [Header("Button Functionality")]
    [Tooltip("The door to open")]
    [SerializeField] private Door _doorToOpen;
    [Tooltip("The slenderman to spawn when the button is pushed")]
    [SerializeField] private GameObject _slenderman;
    [Tooltip("The blackscreen canvas")]
    [SerializeField] private GameObject _blackScreenCanvas;

    private Animator _animator;

    private bool _isTextFaded = false;

    #endregion




    #region Base Methods

    // Override the base class method for specific implementation
    protected override void RunItemCode()
    {
        if (GameManager.Instance.EventData.CheckEvent("Button")) return;
        GameManager.Instance.EventData.SetEvent("Button");

        _animator = GetComponent<Animator>();
        _animator.SetTrigger("Button");
        AudioManager.Instance.PlayClipOneShot(AudioManager.Instance.PlayerSpeaker, "push button", 1f, 1f);

        _doorToOpen.IsLocked = false;
        _doorToOpen.Interact();
        StartCoroutine(StartEvent());
    }

    #endregion




    #region Coroutines

    IEnumerator StartEvent()
    {
        // Fade out the environment audio
        AudioManager.Instance.FadeOutAudio(AudioManager.Instance.Environment, 2f);

        yield return new WaitForSeconds(1f);

        // Fade in the black screen
        _blackScreenCanvas.SetActive(true);
        Image image = _blackScreenCanvas.GetComponentInChildren<Image>();

        for (float i = image.color.a; i < 1; i += 0.01f)
        {
            image.color = new Color(0, 0, 0, i);
            yield return new WaitForSeconds(0.01f);
        }
        image.color = new Color(0, 0, 0, 1);
        
        // Set the new audio clip for the environment
        AudioManager.Instance.SetAudioClip(AudioManager.Instance.Environment, "horror chase music 1");

        // Fade in the text
        StartCoroutine(TextFade());

        yield return new WaitUntil(() => _isTextFaded);

        // Play the new environment audio
        AudioManager.Instance.PlayAudio(AudioManager.Instance.Environment, 0.15f, 1f, true);

        // Fade out the black screen
        for (float i = image.color.a; i > 0; i -= 0.01f)
        {
            image.color = new Color(0, 0, 0, i);
            yield return new WaitForSeconds(0.01f);
        }
        image.color = new Color(0, 0, 0, 0);
        _blackScreenCanvas.SetActive(false);

        yield return new WaitForSeconds(0.5f);

        // Spawn slenderman and force player in sight
        _slenderman.SetActive(true);
        _slenderman.GetComponent<AISensor>().PlayerInSightForced(6f);

        yield return new WaitForSeconds(2f);

        // Play speaker audio clip
        AudioManager.Instance.PlayClipOneShot(AudioManager.Instance.PlayerSpeaker2, "speaker slender");
    }

    // Coroutine to fade the text in and out
    IEnumerator TextFade(bool alpha = true)
    {
        float scale = 0.002f;
        TextMeshProUGUI text = _blackScreenCanvas.GetComponentInChildren<TextMeshProUGUI>();
        WaitForSeconds wait = new WaitForSeconds(0.01f);

        // slowly fade alpha to 1 and scale to 1.2
        if (alpha)
        {
            for (float i = text.color.a; i < 1; i += 0.01f)
            {
                text.color = new Color(text.color.r, text.color.g, text.color.b, i);
                text.transform.localScale = new Vector3(text.transform.localScale.x + scale, text.transform.localScale.y + scale, text.transform.localScale.z + scale);
                yield return wait;
            }
            StartCoroutine(TextFade(false));
        }
        else
        {
            for (float i = text.color.r; i >= 0; i -= 0.01f)
            {
                text.color = new Color(i, text.color.g, text.color.b, text.color.a);
                text.transform.localScale = new Vector3(text.transform.localScale.x + scale, text.transform.localScale.y + scale, text.transform.localScale.z + scale);
                yield return wait;
            }
            text.gameObject.SetActive(false);
            _isTextFaded = true;
        }
    }

    #endregion




    #region Public Methods

    // Function to load the event from the save file
    public void EventLoad()
    {
        Debug.Log("Button event loaded");
    }

    #endregion
}
