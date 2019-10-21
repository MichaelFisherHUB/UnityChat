using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Extensions;

public class LocalStorageManager
{
    private const string RANDOM_DATA_FOLDER = "RandomData";
    private const string USED_RANDOM_INTS = "UsedInts.json";

    [SerializeField]
    private static List<int> usedInRandom;

    public static int GetRandomUnrepitingInt()
    {
        #region Check for already used random ints

        if (usedInRandom == null)
        {
            string usedIntsJSON = ReadAllFromFile(RANDOM_DATA_FOLDER, USED_RANDOM_INTS);
            usedInRandom = new List<int>();
            if (!string.IsNullOrEmpty(usedIntsJSON))
            {
                usedInRandom.FromJson(usedIntsJSON);
            }
        }
        #endregion

        #region Random

        int randomUnrepitingInt = 0;
        try
        {
            do
            {
                randomUnrepitingInt = Random.Range(int.MinValue, int.MaxValue);
            }
            while (usedInRandom.Contains(randomUnrepitingInt));
        }
        catch
        {
            Debug.LogError("Womething went wrong!! Cant create unused int");
            return 1010101;
        }

        usedInRandom.Add(randomUnrepitingInt);

        string JSONstr = usedInRandom.ToJson();
        WriteToFile(RANDOM_DATA_FOLDER, USED_RANDOM_INTS, JSONstr);
        #endregion

        return randomUnrepitingInt;
    }

    #region Local data managment

    private static void WriteToFile(string folderName, string file, string content)
    {
        CreateIfNotExist(folderName, file);
        string fullPath = Path.Combine(Path.Combine(Application.persistentDataPath, folderName), file);
        File.WriteAllText(fullPath, content);
    }

    private static string ReadAllFromFile(string folderName, string file)
    {
        CreateIfNotExist(folderName, file);
        string fullPath = Path.Combine(Path.Combine(Application.persistentDataPath, folderName), file);
        return File.ReadAllText(fullPath);
    }

    private static void CreateIfNotExist(string folderName, string file)
    {
        string fullFolderPath = Path.Combine(Application.persistentDataPath, folderName);
        if (!Directory.Exists(fullFolderPath))
        {
            Directory.CreateDirectory(fullFolderPath);
        }

        string fullFilePath = Path.Combine(fullFolderPath, file);
        if (!File.Exists(fullFilePath))
        {
            File.Create(fullFilePath);
        }
    }
    #endregion
}
