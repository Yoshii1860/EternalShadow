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

    // If key was picked up and used before flipped
    [SerializeField] Door door;
    // If key was picked up before flipped
    [SerializeField] GameObject key;
    // Key of flipped room
    [SerializeField] GameObject keyFlipped;

    void Start()
    {
        foreach (GameObject mannequin in mannequins)
        {
            mannequin.SetActive(false);
        }
    }

    public void EventLoad()
    {
        unflippedCell.SetActive(false);
        flippedCell.SetActive(true);
        for (int i = 0; i < mannequins.Length; i++)
        {
            if (!mannequins[i].activeSelf) continue;
            Mannequin mannequinCode = mannequins[i].GetComponent<Mannequin>();
            mannequinCode.started = true;
            mannequinCode.move = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            // if the player is looking at the lookAtObject or at least less then 30 degrees away from it
            if (Vector3.Angle(other.transform.forward, lookAtObject.position - other.transform.position) < 30)
            {
                if (!door.locked || key == null) keyFlipped.SetActive(false);
                unflippedCell.SetActive(true);
                flippedCell.SetActive(false);
                horrorDoll.SetActive(false);
                GameManager.Instance.eventData.SetEvent("Flipped");
                for (int i = 0; i < mannequins.Length; i++)
                {
                    Debug.Log("Mannequin " + i + " activated");
                    mannequins[i].SetActive(true);
                }
                StartCoroutine(StartMannequins());
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

        FinishEvent();
    }

    void FinishEvent()
    {
        Destroy(gameObject);
    }
}
