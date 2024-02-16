using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NavKeypad;

public class KeypadCode : InteractableObject
{
    [Header("Value")]
    [SerializeField] string value;
    [Header("Button Animation Settings")]
    [SerializeField] float bttnspeed = 0.1f;
    [SerializeField] float moveDist = 0.0025f;
    [SerializeField] float buttonPressedTime = 0.1f;
    [Header("Component References")]
    [SerializeField] Keypad keypad;
    bool moving;

    protected override void RunItemCode()
    {
        if (!moving)
        {
            keypad.AddInput(value);
            StartCoroutine(MoveSmooth());
        }
    }

    IEnumerator MoveSmooth()
    {

        moving = true;
        Vector3 startPos = transform.localPosition;
        Vector3 endPos = transform.localPosition + new Vector3(0, 0, moveDist);

        float elapsedTime = 0;
        while (elapsedTime < bttnspeed)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / bttnspeed);

            transform.localPosition = Vector3.Lerp(startPos, endPos, t);

            yield return null;
        }
        transform.localPosition = endPos;
        yield return new WaitForSeconds(buttonPressedTime);
        startPos = transform.localPosition;
        endPos = transform.localPosition - new Vector3(0, 0, moveDist);

        elapsedTime = 0;
        while (elapsedTime < bttnspeed)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / bttnspeed);

            transform.localPosition = Vector3.Lerp(startPos, endPos, t);

            yield return null;
        }
        transform.localPosition = endPos;

        moving = false;
    }
}
