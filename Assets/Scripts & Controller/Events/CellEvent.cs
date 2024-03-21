using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellEvent : MonoBehaviour
{

    [SerializeField] GameObject flippedCell;
    [SerializeField] GameObject unflippedCell;
    [SerializeField] GameObject[] mannequins;
    [SerializeField] Transform lookAtObject;

    void Start()
    {
        foreach (GameObject mannequin in mannequins)
        {
            mannequin.SetActive(false);
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
                for (int i = 0; i < mannequins.Length; i++)
                {
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
