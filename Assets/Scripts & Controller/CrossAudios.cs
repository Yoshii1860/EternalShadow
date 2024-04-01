using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossAudios : MonoBehaviour
{
    private void CrossShake()
    {
        AudioManager.Instance.PlaySoundOneShot(AudioManager.Instance.playerSpeaker, "cross shake", 0.6f, 1f);
    }

    private void CrossFall()
    {
        AudioManager.Instance.FadeOut(gameObject.GetInstanceID(), 0.5f);
        AudioManager.Instance.PlaySoundOneShot(AudioManager.Instance.playerSpeaker2, "cross fall", 0.6f, 1f);
    }
}
