using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameEnd : MonoBehaviour, ICustomUpdatable
{
    #region Fields

    [SerializeField] private GameObject _finishGameCanvas;
    [SerializeField] private TextMeshProUGUI _creditsText;
    [SerializeField] private float _creditsScrollSpeed  = 30f;

    private Door _door;
    private bool _playOnce = false;

    [SerializeField] private bool _debugMode = false;

    #endregion




    #region Custom Update

    public void CustomUpdate(float deltaTime)
    {
        if (!_door.IsLocked && _door.IsOpen && !_playOnce)
        {
            _playOnce = true;
            StartCoroutine(EndGame());
        }
    }

    #endregion




    #region Unity Methods

    void Update()
    {
        if (_debugMode)
        {
            _debugMode = false;
            StartCoroutine(EndGame());
        }
    }

    void Start()
    {
        _door = GetComponent<Door>();
    }

    #endregion




    #region Coroutines

    IEnumerator EndGame()
    {
        AudioManager.Instance.PlayClipOneShot(AudioManager.Instance.PlayerSpeaker2, "speaker end", 1f, 1f);

        yield return new WaitForSeconds(2f);

        // Start the gameplay event
        GameManager.Instance.GameplayEvent();

        // Fade out all audio
        AudioManager.Instance.FadeOutAll(3f);

        // Fade in the black screen
        Image blackScreenImage = GameManager.Instance.Blackscreen.GetComponent<Image>();
        blackScreenImage.color = new Color(0, 0, 0, 0);
        GameManager.Instance.Blackscreen.gameObject.SetActive(true);

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / 2;
            blackScreenImage.color = new Color(0, 0, 0, t);
            yield return null;
        }

        yield return new WaitForSeconds(3f);

        // Show the finish game canvas and set the piano music
        _finishGameCanvas.SetActive(true);
        AudioManager.Instance.SetAudioClip(AudioManager.Instance.Environment, "piano music");

        yield return new WaitForSeconds(0.1f);

        // Play the piano music
        AudioManager.Instance.PlayAudio(AudioManager.Instance.Environment, 0.6f, 1f, true);

        // scroll through credits, it starts at -400f and stops at 3400f 
        RectTransform creditsRect = _creditsText.GetComponent<RectTransform>();
        float creditsY = -400f;
        while (creditsY < 3400f)
        {
            creditsY += _creditsScrollSpeed * Time.deltaTime;
            creditsRect.anchoredPosition = new Vector2(0, creditsY);
            yield return null;
        }

        // Stop the piano music
        AudioManager.Instance.StopAudioWithDelay(AudioManager.Instance.Environment, 5f);

        yield return new WaitForSeconds(5f);

        // Go back to the main menu
        GameManager.Instance.BackToMainMenu();
    }

    #endregion
}