using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameEnd : MonoBehaviour, ICustomUpdatable
{
    Door door;
    bool once = false;
    [SerializeField] GameObject finishCanvas;
    [SerializeField] TextMeshProUGUI creditsText;
    [SerializeField] float creditsSpeed  = 10f;
    public bool debug = false;


    public void CustomUpdate(float deltaTime)
    {
        if (!door.locked && door.open && !once)
        {
            once = true;
            StartCoroutine(EndGame());
        }
    }

    void Update()
    {
        if (debug)
        {
            debug = false;
            StartCoroutine(EndGame());
        }
    }

    void Start()
    {
        door = GetComponent<Door>();
    }

    IEnumerator EndGame()
    {
        AudioManager.Instance.PlaySoundOneShot(AudioManager.Instance.playerSpeaker2, "speaker end", 1f, 1f);
        yield return new WaitForSeconds(2f);
        GameManager.Instance.GameplayEvent();
        Image blackScreenImage = GameManager.Instance.blackScreen.GetComponent<Image>();
        blackScreenImage.color = new Color(0, 0, 0, 0);
        GameManager.Instance.blackScreen.gameObject.SetActive(true);
        AudioManager.Instance.FadeOutAll(3f);
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / 2;
            blackScreenImage.color = new Color(0, 0, 0, t);
            yield return null;
        }
        yield return new WaitForSeconds(3f);
        finishCanvas.SetActive(true);
        AudioManager.Instance.SetAudioClip(AudioManager.Instance.environment, "piano music");
        yield return new WaitForSeconds(0.1f);
        AudioManager.Instance.PlayAudio(AudioManager.Instance.environment, 0.6f, 1f, true);

        // scroll through credits, it starts at -400f and stops at 3400f 
        RectTransform creditsRect = creditsText.GetComponent<RectTransform>();
        float creditsY = -400f;
        while (creditsY < 3400f)
        {
            creditsY += creditsSpeed * Time.deltaTime;
            creditsRect.anchoredPosition = new Vector2(0, creditsY);
            yield return null;
        }

        AudioManager.Instance.StopAudioWithDelay(AudioManager.Instance.environment, 5f);

        yield return new WaitForSeconds(5f);
        GameManager.Instance.BackToMainMenu();
    }
}