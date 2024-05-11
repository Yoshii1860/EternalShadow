using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveFileButton : MonoBehaviour
{
    public void OnClick()
    {
        MenuController.Instance.SaveFileButton();
    }
}
