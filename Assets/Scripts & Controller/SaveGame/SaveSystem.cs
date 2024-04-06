using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

/// <summary>
/// Handles saving and loading game data.
/// </summary>
public static class SaveSystem 
{
    #region Save Data
    /// <summary>
    /// Saves the game data to a file.
    /// </summary>
    /// <param name="filename">Name of the file to save.</param>
    /// <param name="player">Reference to the player.</param>
    /// <param name="weaponPool">Reference to the weapon pool.</param>
    /// <param name="interactableObjectPool">Reference to the object pool.</param>
    /// <param name="enemyPool">Reference to the enemy pool.</param>
    /// <param name="interactableObjectPool">Reference to the interactable objects pool.</param>
    public static void SaveGameFile(string filename, Player player, Transform weaponPool, Transform enemyPool, Transform interactableObjectPool, Transform interactStaticObjectPool, Transform doorObjectPool, EventData eventData)
    {
        Debug.Log("SaveSystem.cs - SAVE - filename: " + filename);

        // Binary formatter for serialization
        BinaryFormatter formatter = new BinaryFormatter();

        // Constructing the file path
        string path = Application.persistentDataPath + "/" + filename + ".shadow";

        // Open a stream to create the file
        using (FileStream stream = new FileStream(path, FileMode.Create))
        {
            // Create save data
            SaveData data = new SaveData(player, weaponPool, enemyPool, interactableObjectPool, interactStaticObjectPool, doorObjectPool, eventData);

            // Serialize and write the data to the file
            formatter.Serialize(stream, data);
        }

        // Resume the game after saving
        GameManager.Instance.ResumeGame();
    }

    #endregion

    #region Load Data

    /// <summary>
    /// Loads the game data from a file.
    /// </summary>
    /// <param name="filename">Name of the file to load.</param>
    /// <returns>The loaded game data or null if the file is not found.</returns>
    public static SaveData LoadGameFile(string filename)
    {
        Debug.Log("SaveSystem.cs - LOAD - filename: " + filename);

        // Constructing the file path
        string path = Application.persistentDataPath + "/" + filename + ".shadow";

        // Resume the game before attempting to load
        GameManager.Instance.ResumeGame();

        // Check if the file exists
        if (File.Exists(path))
        {
            // Binary formatter for deserialization
            BinaryFormatter formatter = new BinaryFormatter();

            // Open a stream to read the file
            using (FileStream stream = new FileStream(path, FileMode.Open))
            {
                // Deserialize the data and return it
                return formatter.Deserialize(stream) as SaveData;
            }
        } 
        else 
        {
            Debug.LogError("Save file not found in " + path);
            return null;
        }
    }

    #endregion
}