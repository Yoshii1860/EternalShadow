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

        GameManager.Instance.LoadNewScene(filename);
    }

    public void OnReturnClick()
    {
        // Get the parent of the clicked object
        RectTransform parentTransform = EventSystem.current.currentSelectedGameObject.transform.parent.GetComponent<RectTransform>();;

        // Loop through the children of the parent to find siblings with GridLayoutGroup
        foreach (Transform siblingTransform in parentTransform)
        {
            GridLayoutGroup gridLayout = siblingTransform.GetComponent<GridLayoutGroup>();
            if (gridLayout != null)
            {
                foreach (Transform child in siblingTransform)
                {
                    Destroy(child.gameObject);
                }
            }
        }
        GameManager.Instance.ResumeGame();
        this.gameObject.SetActive(false);
    }
}