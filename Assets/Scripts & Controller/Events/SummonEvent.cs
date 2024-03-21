using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummonEvent : MonoBehaviour
{
    [SerializeField] GameObject[] summoningObjects;
    [SerializeField] ParticleSystem summoningParticles;
    [SerializeField] Transform girl;
    [SerializeField] Vector3 girlPosition;
    [SerializeField] Vector3 girlRotation;

    public void CheckItems()
    {
        foreach (GameObject summoningObject in summoningObjects)
        {
            if (summoningObject.activeSelf == false)
            {
                return;
            }
        }

        summoningParticles.Play();
        girl.gameObject.SetActive(false);
        StartCoroutine(PlaySummoningAnimation());
    }

    IEnumerator PlaySummoningAnimation()
    {
        girl.GetComponent<EnemyBT>().enabled = false;
        girl.rotation = Quaternion.Euler(girlRotation);
        girl.position = girlPosition;
        girl.GetComponentInChildren<Renderer>().enabled = false;

        yield return new WaitForSeconds(3f);

        girl.gameObject.SetActive(true);
        girl.GetComponent<Animator>().SetTrigger("Summon");

        yield return new WaitForSeconds(0.5f);

        girl.GetComponentInChildren<Renderer>().enabled = true;

        yield return new WaitForSeconds(7f);

        girl.GetComponent<EnemyBT>().ResetTree();
        girl.GetComponent<EnemyBT>().enabled = true;
        girl.GetComponent<Animator>().SetTrigger("Release");
    }
}

