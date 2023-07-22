using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveObject : MonoBehaviour
{
    public Player player;

    public void Save()
    {
        Debug.Log("SaveObject");
        GameManager.Instance.SaveData(player);
    }
}
