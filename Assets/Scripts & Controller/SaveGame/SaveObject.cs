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
    public class SaveFileComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            string format = "dd-MM-yyyy HH-mm-ss";
            System.DateTime xDate, yDate;

            // Try parsing the date and time from filenames
            if (System.DateTime.TryParseExact(x, format, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out xDate) &&
                System.DateTime.TryParseExact(y, format, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out yDate))
            {
                // Compare the parsed dates to determine the order
                return yDate.CompareTo(xDate); // Sorting in descending order (newest first)
            }
            else
            {
                // If parsing fails, just compare the original filenames
                return y.CompareTo(x);
            }
        }
    }

    [SerializeField] GameObject filePrefab;
    [SerializeField] Transform contentCanvas;
    [SerializeField] int maxFiles = 8;

    void OnEnable()
    {
        string[] savedFiles = GetSavedFiles();

        // Sort the savedFiles array based on the date and time in filenames
        System.Array.Sort(savedFiles, new SaveFileComparer());

        int fileCount = 0;

        foreach (string file in savedFiles)
        {
            GameObject fileObject = Instantiate(filePrefab, contentCanvas);
            fileObject.GetComponentInChildren<TextMeshProUGUI>().text = file;
            fileCount++;
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
            File.Delete(Path.Combine(Application.persistentDataPath, filename + ".shadow"));
        }
        // Get Scene Name and Date/Time
        Scene scene = SceneManager.GetActiveScene();
        filename = scene.name + " - " + System.DateTime.Now.ToString("dd-MM-yyy HH-mm-ss");
        Debug.Log("SaveObject.OnButtonClick: " + filename);
        // Save the parent canvas of clicked button
        GameObject parentCanvas = EventSystem.current.currentSelectedGameObject.transform.gameObject;
        while (parentCanvas != null)
        {
            if (parentCanvas.transform.parent != null)
            {
                parentCanvas = parentCanvas.transform.parent.gameObject;
            }
            else
            {
                parentCanvas = parentCanvas.GetComponentInChildren<SaveObject>().gameObject;
                break;
            }
        }
        // Delete all children of the parent canvas to prevent duplicates
        foreach (Transform child in EventSystem.current.currentSelectedGameObject.transform.parent.transform)
        {
            Destroy(child.gameObject);
        }
        // Save the game data
        GameManager.Instance.SaveData(filename);
        parentCanvas.SetActive(false);
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