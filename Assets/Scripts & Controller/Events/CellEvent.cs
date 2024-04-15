using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellEvent : MonoBehaviour
{

    [SerializeField] GameObject flippedCell;
    [SerializeField] GameObject unflippedCell;
    [SerializeField] GameObject[] mannequins;
    [SerializeField] Transform lookAtObject;
    [Tooltip("The horror doll that will only be there when it`s flipped")]
    [SerializeField] GameObject horrorDoll;
    [SerializeField] GameObject groundFog;
    bool playOnce = false;

    void Start()
    {
        foreach (GameObject mannequin in mannequins)
        {
            //GameManager.Instance.customUpdateManager.AddCustomUpdatable(mannequin.GetComponent<Mannequin>());
            mannequin.SetActive(false);
        }
    }

    public void EventLoad()
    {
        unflippedCell.SetActive(true);
        flippedCell.SetActive(false);
        groundFog.SetActive(false);
        playOnce = true;
        for (int i = 0; i < mannequins.Length; i++)
        {
            Mannequin mannequinCode = mannequins[i].GetComponent<Mannequin>();
            if (!mannequinCode.isDead)
            {
                mannequins[i].SetActive(true);
                mannequinCode.started = true;
                mannequinCode.move = true;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            // if the player is looking at the lookAtObject or at least less then 30 degrees away from it
            if (Vector3.Angle(other.transform.forward, lookAtObject.position - other.transform.position) < 30)
            {
                unflippedCell.SetActive(true);
                flippedCell.SetActive(false);
                horrorDoll.SetActive(false);
                groundFog.SetActive(false);
                GameManager.Instance.eventData.SetEvent("Flipped");
                for (int i = 0; i < mannequins.Length; i++)
                {
                    Debug.Log("Mannequin " + i + " activated");
                    mannequins[i].SetActive(true);
                }
                StartCoroutine(StartMannequins());
            }
            else
            {
                if (!playOnce) 
                {
                    AudioManager.Instance.PlayOneShotWithDelay(AudioManager.Instance.playerSpeaker2, "speaker flipped", 0.5f);
                    playOnce = true;
                }
            }
        }
    }

    IEnumerator StartMannequins()
    {
        yield return new WaitForSeconds(2f);

        for (int i = 0; i < mannequins.Length; i++)
        {
            Mannequin mannequinCode = mannequins[i].GetComponent<Mannequin>();
            mannequinCode.started = true;
            mannequinCode.move = true;
        }

        yield return new WaitForSeconds(1f);

        AudioManager.Instance.PlaySoundOneShot(AudioManager.Instance.playerSpeaker2, "speaker mannequins 2");

        FinishEvent();
    }

    void FinishEvent()
    {
        gameObject.SetActive(false);
    }
}
