using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossAudios : MonoBehaviour
{
    // To be called on animation event of the cross
    private void CrossShake()
    {
        AudioManager.Instance.PlayClipOneShot(AudioManager.Instance.PlayerSpeaker, "cross shake", 0.6f, 1f);
    }

    // To be called on animation event of the cross
    private void CrossFall()
    {
        AudioManager.Instance.FadeOutAudio(gameObject.GetInstanceID(), 0.5f);
        AudioManager.Instance.PlayClipOneShot(AudioManager.Instance.PlayerSpeaker2, "cross fall", 0.6f, 1f);
    }
}
