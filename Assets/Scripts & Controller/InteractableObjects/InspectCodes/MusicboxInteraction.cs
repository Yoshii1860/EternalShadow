using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicboxInteraction : Interaction
{
    #region Variables

    // Reference to the music box lid
    private Transform _musicBoxLid;

    #endregion




    #region Unity Methods

    // Start is called before the first frame update
    void Start()
    {
        // for each child look for tag "lid"
        foreach (Transform child in transform)
        {
            if (child.CompareTag("lid"))
            {
                _musicBoxLid = child;
            }
        }
    }

    #endregion




    #region Base Methods

    // Override the base class method for specific implementation
    public override void OnInteract()
    {
        Debug.Log("MusicboxInteraction.OnInteract()");
        StartCoroutine(OpenLid());
    }

    #endregion




    #region Coroutines

    // Coroutine to open the music box lid
    IEnumerator OpenLid()
    {
        for (float i = _musicBoxLid.localEulerAngles.x; i > 0; i -= 0.5f)
        {
            _musicBoxLid.localEulerAngles = new Vector3(i, 0, 0);
            yield return new WaitForSeconds(0.01f);
        }
    }

    #endregion
}
