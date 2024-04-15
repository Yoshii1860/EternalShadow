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
    [SerializeField] GameObject speaker;
    [SerializeField] Door[] doors;
    [SerializeField] GameObject[] lockers;

    public void CheckItems()
    {
        foreach (GameObject summoningObject in summoningObjects)
        {
            if (summoningObject.activeSelf == false)
            {
                return;
            }
        }
        AudioManager.Instance.PlayAudio(speaker.GetInstanceID(), 0.8f, 1f, false);
        AudioManager.Instance.FadeOut(AudioManager.Instance.environment, 3f);
        summoningParticles.Play();

        CloseDoors();

        girl.SetActive(false);
        slender.SetActive(false);
        GameManager.Instance.customUpdateManager.RemoveCustomUpdatable(girl.GetComponent<AISensor>());
        GameManager.Instance.customUpdateManager.RemoveCustomUpdatable(slender.GetComponent<AISensor>());

        loot.SetActive(true);

        StartCoroutine(PlaySummoningAnimation());
    }

    IEnumerator PlaySummoningAnimation()
    {
        yield return new WaitForSeconds(1f);

        AudioManager.Instance.PlaySoundOneShot(priest.GetInstanceID(), "summoning", 0.7f, 1f);

        yield return new WaitForSeconds(3f);

        AudioManager.Instance.SetAudioClip(AudioManager.Instance.environment, "horror chase music 3");
        AudioManager.Instance.PlayAudio(AudioManager.Instance.environment, 0.4f, 1f, true);
        
        priest.GetComponent<Animator>().SetTrigger("summon");

        yield return new WaitForSeconds(2f);

        AudioManager.Instance.PlaySoundOneShot(AudioManager.Instance.playerSpeaker2, "speaker summon", 1f);

        yield return new WaitForSeconds(5f);

        AudioManager.Instance.FadeIn(priest.GetInstanceID(), 3f);
        priest.GetComponent<Boss>().FollowPlayer();
        priest.GetComponent<CapsuleCollider>().enabled = true;
        priest.GetComponent<BoxCollider>().enabled = true; 
    }

    void CloseDoors()
    {
        foreach (Door door in doors)
        {
            door.CloseDoor();
            door.locked = true;
            door.displayMessage = "This door cannot be opened!";
        }

        foreach (GameObject locker in lockers)
        {
            locker.GetComponent<LockerCode>().CloseLocker();
        }
    }
}

