using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NavKeypad;

public class KeypadCode : InteractableObject
{
    #region Properties

    [Header("Keypad value")]
    [SerializeField] private string _keyValue;
    [Header("Button Animation Settings")]
    [SerializeField] private float _buttonSpeed = 0.1f;
    [SerializeField] private float _moveDistance = 0.0025f;
    [SerializeField] private float _buttonPressedTime = 0.1f;
    [Header("Component References")]
    [SerializeField] private Keypad _keypad;
    
    private bool _isMoving;

    #endregion





    #region Base Methods

    protected override void RunItemCode()
    {
        if (!_isMoving)
        {
            _keypad.AddInput(_keyValue);
            StartCoroutine(MoveSmooth());
        }
    }

    #endregion




    #region Coroutines

    // Coroutine to move the button smoothly
    IEnumerator MoveSmooth()
    {

        _isMoving = true;

        // Move the button forward
        Vector3 startPos = transform.localPosition;
        Vector3 endPos = transform.localPosition + new Vector3(0, 0, _moveDistance);

        float elapsedTime = 0;
        while (elapsedTime < _buttonSpeed)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / _buttonSpeed);

            transform.localPosition = Vector3.Lerp(startPos, endPos, t);

            yield return null;
        }
        transform.localPosition = endPos;
        yield return new WaitForSeconds(_buttonPressedTime);

        // Move the button back to its original position
        startPos = transform.localPosition;
        endPos = transform.localPosition - new Vector3(0, 0, _moveDistance);

        elapsedTime = 0;
        while (elapsedTime < _buttonSpeed)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / _buttonSpeed);

            transform.localPosition = Vector3.Lerp(startPos, endPos, t);

            yield return null;
        }
        transform.localPosition = endPos;

        _isMoving = false;
    }

    #endregion
}
