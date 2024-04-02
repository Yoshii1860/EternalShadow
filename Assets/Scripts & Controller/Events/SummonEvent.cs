using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummonEvent : MonoBehaviour
{
    [SerializeField] GameObject[] summoningObjects;
    [SerializeField] ParticleSystem summoningParticles;
    [SerializeField] GameObject slender;
    [SerializeField] GameObject girl;
    [SerializeField] GameObject priest;
    [SerializeField] GameObject loot;

    public void CheckItems()
    {
        foreach (GameObject summoningObject in summoningObjects)
        {
            if (summoningObject.activeSelf == false)
            {
                return;
            }
        }

        AudioManager.Instance.FadeOut(AudioManager.Instance.environment, 3f);
        summoningParticles.Play();

        girl.SetActive(false);
        slender.SetActive(false);
        GameManager.Instance.customUpdateManager.RemoveCustomUpdatable(girl.GetComponent<AISensor>());
        GameManager.Instance.customUpdateManager.RemoveCustomUpdatable(slender.GetComponent<AISensor>());

        loot.SetActive(true);

        StartCoroutine(PlaySummoningAnimation());
    }

    IEnumerator PlaySummoningAnimation()
    {
        priest.GetComponent<CapsuleCollider>().enabled = true;
        priest.GetComponent<BoxCollider>().enabled = true; 

        yield return new WaitForSeconds(3f);

        AudioManager.Instance.SetAudioClip(AudioManager.Instance.environment, "horror chase music 3");
        AudioManager.Instance.PlayAudio(AudioManager.Instance.environment, 0.4f, 1f, true);
        
        priest.GetComponent<Animator>().SetTrigger("summon");
        priest.GetComponent<AudioSource>().Play();

        yield return new WaitForSeconds(7f);

        priest.GetComponent<Boss>().FollowPlayer();
    }
}

