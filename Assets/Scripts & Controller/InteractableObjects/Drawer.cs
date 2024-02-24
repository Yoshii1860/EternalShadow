using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Drawer : InteractableObject
{
    [Space(10)]
    [Header("OPEN DOOR/DRAWER")]
    [Tooltip("Is it a drawer (true) or a door (false)?")]
    [SerializeField] bool drawer;
    [Tooltip("The position (drawer) or rotation (door) to move to")]
    [SerializeField] Vector3 openPosOrRot;

    void Start()
    {
        transform.GetComponent<AudioSource>().spatialBlend = 1;
        transform.GetComponent<AudioSource>().rolloffMode = AudioRolloffMode.Custom;
    }

    protected override void RunItemCode()
    {
        StartCoroutine(LerpToPosition());
    }

    IEnumerator LerpToPosition()
    {
        Vector3 currentVector = drawer ? transform.localPosition : transform.localEulerAngles;
        float t = 0f;
        float timeToMove = 1f;

        if (drawer) AudioManager.Instance.PlaySoundOneShot(gameObject.GetInstanceID(), "drawer", 1f, 1f);
        else AudioManager.Instance.PlaySoundOneShot(gameObject.GetInstanceID(), "closet door", 1f, 1f);

        yield return new WaitForSeconds(0.3f);

        while (t < 1)
        {
            t += Time.deltaTime / timeToMove;
            if (drawer)
            {
                transform.localPosition = Vector3.Lerp(currentVector, openPosOrRot, t);
            }
            else
            {
                transform.localEulerAngles = Vector3.Lerp(currentVector, openPosOrRot, t);
            }
            yield return null;
        }
    }
}
