using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class SaveObject : MonoBehaviour
{
    [SerializeField] GameObject filePrefab;
    [SerializeField] Transform contentCanvas;
    [SerializeField] int maxFiles = 8;

    void OnEnable()
    {
        string[] savedFiles = GetSavedFiles();
        int fileCount = 0;

        foreach (string file in savedFiles)
        {
            GameObject fileObject = Instantiate(filePrefab, contentCanvas);
            fileObject.GetComponentInChildren<TextMeshProUGUI>().text = file;
            fileCount ++;
        }

        for (int i = fileCount; i < maxFiles; i++)
        {
            GameObject fileObject = Instantiate(filePrefab, contentCanvas);
            fileObject.GetComponentInChildren<TextMeshProUGUI>().text = " ";
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
        if (filename != " ")
        {
            // Delete File
            File.Delete(Application.persistentDataPath + filename + ".shadow");
        }
        // Get Scene Name and Date/Time
        Scene scene = SceneManager.GetActiveScene();
        filename = scene.name + " - " + System.DateTime.Now.ToString("dd-MM-yyy HH-mm");
        Debug.Log("SaveObject.OnButtonClick: " + filename);
        // Delete all children of the parent canvas to prevent duplicates
        foreach (Transform child in EventSystem.current.currentSelectedGameObject.transform.parent.transform)
        {
            Destroy(child.gameObject);
        }
        // Save the game data
        GameManager.Instance.SaveData(filename);
        // Disable the parent canvas
        CloseCanvas();
    }

    void CloseCanvas()
    {
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