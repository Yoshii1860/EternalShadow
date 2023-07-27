using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class DEBUGDeleteSaveFiles : MonoBehaviour
{
    public bool deleteSaveFiles = false;

    void Start()
    {
        if (deleteSaveFiles)
        {
            DeleteAllSaveFiles();
        }
    }
    
    public void DeleteAllSaveFiles()
    {
        string[] saveFiles = Directory.GetFiles(Application.persistentDataPath, "*.shadow");

        foreach (string saveFile in saveFiles)
        {
            File.Delete(saveFile);
        }

        Debug.Log("##############################");
        Debug.Log("##### All Save Files Deleted");
        Debug.Log("##############################");
    }
}