using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicboxInteraction : Interaction
{
    private Transform musicBoxLid;

    void Start()
    {
        // for each child look for tag "lid"
        foreach (Transform child in transform)
        {
            if (child.CompareTag("lid"))
            {
                musicBoxLid = child;
            }
        }
    }

    public override void OnInteract()
    {
        Debug.Log("MusicboxInteraction.OnInteract()");
        StartCoroutine(OpenLid());
    }

    IEnumerator OpenLid()
    {
        for (float i = musicBoxLid.localEulerAngles.x; i > 0; i -= 0.5f)
        {
            musicBoxLid.localEulerAngles = new Vector3(i, 0, 0);
            yield return new WaitForSeconds(0.01f);
        }
    }
}
