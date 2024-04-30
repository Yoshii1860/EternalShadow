using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interaction : MonoBehaviour
{
    #region Variables

    private bool _isInteractable = true;

    #endregion




    #region Public Methods

    public void Interact()
    {
        if (_isInteractable)
        {
            _isInteractable = false;
            OnInteract();
        }
    }

    #endregion




    #region Base Methods

    public virtual void OnInteract()
    {
        Debug.Log("Interaction.OnInteract()");
    }

    #endregion
}
