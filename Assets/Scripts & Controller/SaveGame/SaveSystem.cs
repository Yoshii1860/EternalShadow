using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem 
{
    public static void SavePlayer(Player player, Transform weaponPool, Transform objectPool, Transform enemyPool)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/player.shadow";
        FileStream stream = new FileStream(path, FileMode.Create);

        SaveData data = new SaveData(player, weaponPool, objectPool, enemyPool);

        formatter.Serialize(stream, data);
        stream.Close();
    }

    public static SaveData LoadPlayer()
    {
        string path = Application.persistentDataPath + "/player.shadow";
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