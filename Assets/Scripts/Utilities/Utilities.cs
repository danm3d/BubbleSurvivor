using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.UI;

public static class Utilities
{

    public static T LoadClass<T>(string filePath)
    {
        string fileName = Application.persistentDataPath + filePath;
        if (File.Exists(fileName))
        {
            BinaryFormatter bf = new BinaryFormatter();
            try
            {
                using (FileStream fileStr = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    return (T)bf.Deserialize(fileStr);
                }
            } catch
            {
                return default(T);
            }
        }
        return default(T);
    }
	
    public static void SaveClass<T>(string filePath, T saveClass)
    {
        if (saveClass != null)
        {
            string fileName = Application.persistentDataPath + filePath;
            BinaryFormatter bf = new BinaryFormatter();
            Directory.CreateDirectory(Application.persistentDataPath + @"/BubbleSurvivor");
            using (FileStream fileStr = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                bf.Serialize(fileStr, saveClass);
            }
        }
    }

}
