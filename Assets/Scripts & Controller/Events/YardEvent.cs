using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YardEvent : MonoBehaviour
{
    [SerializeField] GameObject girl;
    [SerializeField] GameObject gate;
    [SerializeField] Door door;
    [SerializeField] GameObject light;
    bool unique = true;

    void OnTriggerExit(Collider other) 
    {
        if (unique)
        {
            unique = false;
            gate.transform.eulerAngles = new Vector3(0, 0, 0);
            StartCoroutine(RotateDoor());
        }
    }

    IEnumerator RotateDoor()
    {
        girl.GetComponent<EnemyBT>().enabled = true;
        girl.GetComponent<AISensor>().enabled = true;
        girl.GetComponent<Animator>().SetBool("Walking", true);
        yield return new WaitForSeconds(3f);
        door.CloseDoor();
        yield return new WaitForSeconds(1.5f);
        light.SetActive(false); 
        girl.GetComponent<Animator>().SetBool("Walking", false);
        girl.SetActive(false);
    }
}
