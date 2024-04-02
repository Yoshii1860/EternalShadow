using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ButtonCode : InteractableObject
{
    [Space(10)]
    [Header("RUN ITEM CODE")]
    [Tooltip("The door to open")]
    [SerializeField] Door doorToOpen;
    [Tooltip("The slenderman to spawn when the button is pushed")]
    [SerializeField] GameObject slenderman;
    [Tooltip("The blackscreen canvas")]
    [SerializeField] GameObject blackScreenCanvas;
    Animator animator;

    bool textFaded = false;

    // Override the base class method for specific implementation
    protected override void RunItemCode()
    {
        animator = GetComponent<Animator>();
        animator.SetTrigger("Button");
        AudioManager.Instance.PlaySoundOneShot(AudioManager.Instance.playerSpeaker, "push button", 1f, 1f);
        doorToOpen.locked = false;
        doorToOpen.Interact();
        StartCoroutine(StartEvent());
    }

    IEnumerator StartEvent()
    {
        AudioManager.Instance.FadeOut(AudioManager.Instance.environment, 2f);

        yield return new WaitForSeconds(1f);

        blackScreenCanvas.SetActive(true);
        Image image = blackScreenCanvas.GetComponentInChildren<Image>();

        for (float i = image.color.a; i < 1; i += 0.01f)
        {
            image.color = new Color(0, 0, 0, i);
            yield return new WaitForSeconds(0.01f);
        }
        image.color = new Color(0, 0, 0, 1);

        AudioManager.Instance.SetAudioClip(AudioManager.Instance.environment, "horror chase music 1");

        StartCoroutine(TextFade());

        yield return new WaitUntil(() => textFaded);

        AudioManager.Instance.PlayAudio(AudioManager.Instance.environment, 0.35f, 1f, true);

        for (float i = image.color.a; i > 0; i -= 0.01f)
        {
            image.color = new Color(0, 0, 0, i);
            yield return new WaitForSeconds(0.01f);
        }
        image.color = new Color(0, 0, 0, 0);

        blackScreenCanvas.SetActive(false);

        yield return new WaitForSeconds(0.5f);

        slenderman.SetActive(true);
        slenderman.GetComponent<AISensor>().PlayerInSightForced(6f);
    }

    IEnumerator TextFade(bool alpha = true)
    {
        float scale = 0.002f;
        TextMeshProUGUI text = blackScreenCanvas.GetComponentInChildren<TextMeshProUGUI>();
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
            textFaded = true;
        }
    }
}
