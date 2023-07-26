using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class LoadObject : MonoBehaviour
{
    [SerializeField] GameObject filePrefab;
    [SerializeField] Transform contentCanvas;

    void OnEnable()
    {
        string[] savedFiles = GetSavedFiles();

        foreach (string file in savedFiles)
        {
            GameObject fileObject = Instantiate(filePrefab, contentCanvas);
            fileObject.GetComponentInChildren<TextMeshProUGUI>().text = file;
        }
    }

    // Function to get the list of saved file names (excluding file extensions)
    public string[] GetSavedFiles()
    {
        // Get all files in the persistent data path with the ".shadow" extension
        string[] savedFiles = Directory.GetFiles(Application.persistentDataPath, "*.shadow");
        for (int i = 0; i < savedFiles.Length; i++)
        {
            // Remove the directory path and ".shadow" extension, leaving just the file names
            savedFiles[i] = Path.GetFileNameWithoutExtension(savedFiles[i]);
        }
        return savedFiles;
    }

    public void OnButtonClick()
    {
        // Get the file name from the button text
        string filename = EventSystem.current.currentSelectedGameObject.GetComponentInChildren<TextMeshProUGUI>().text;
        // Delete all children of the parent canvas to prevent duplicates
        foreach (Transform child in EventSystem.current.currentSelectedGameObject.transform.parent.transform)
        {
            Destroy(child.gameObject);
        }
        // Load the game data from the file
        GameManager.Instance.LoadData(filename);
        // Disable the parent canvas
        GameObject parentCanvas = EventSystem.current.currentSelectedGameObject.transform.gameObject;
        while (parentCanvas != null)
        {
            if (parentCanvas.transform.parent != null)
            {
                parentCanvas = parentCanvas.transform.parent.gameObject;
            }
            else
            {
                parentCanvas.SetActive(false);
                break;
            }
        }
    }
}