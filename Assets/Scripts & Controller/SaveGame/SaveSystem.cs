using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem 
{
    public static void SaveGameFile(string filename, Player player, Transform weaponPool, Transform objectPool, Transform enemyPool, Transform interactableObjectsPool)
    {
        Debug.Log("SaveSystem.cs - SAVE - filename: " + filename);
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/" + filename + ".shadow";
        FileStream stream = new FileStream(path, FileMode.Create);

        SaveData data = new SaveData(player, weaponPool, objectPool, enemyPool, interactableObjectsPool);

        formatter.Serialize(stream, data);
        stream.Close();
        GameManager.Instance.ResumeGame();
    }

    public static SaveData LoadGameFile(string filename)
    {
        Debug.Log("SaveSystem.cs - LOAD - filename: " + filename);
        string path = Application.persistentDataPath + "/" + filename + ".shadow";
        GameManager.Instance.ResumeGame();
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            SaveData data = formatter.Deserialize(stream) as SaveData;
            stream.Close();

            return data;
        } 
        else 
        {
            Debug.LogError("Save file not found in " + path);
            return null;
        }
    }
}