using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem 
{
    #region Save Data

    public static void SaveGameFile(
        string filename, 
        Player player, 
        Transform weaponPool, 
        Transform enemyPool, 
        Transform interactableObjectPool, 
        Transform interactStaticObjectPool, 
        Transform doorObjectPool, 
        Transform textObjectPool, 
        Transform summonObjectPool,
        Transform autoSavePool)
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
            SaveData data = new SaveData(player, weaponPool, enemyPool, interactableObjectPool, interactStaticObjectPool, doorObjectPool, textObjectPool, summonObjectPool, autoSavePool);

            // Serialize and write the data to the file
            formatter.Serialize(stream, data);
        }

        // Resume the game after saving
        GameManager.Instance.ResumeGame();
    }

    #endregion




    #region Load Data

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