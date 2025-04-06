using System.IO;
using UnityEngine;

public class LevelManager
{
    private const string CurrentLevelKey = "CurrentLevel";

    public static LevelData LoadLevel(int levelNumber)
    {
        string levelFileName = $"level_{levelNumber:00}.json";
        string filePath = Path.Combine(Application.dataPath, "Levels", levelFileName);

        if (!File.Exists(filePath))
        {
            Debug.LogError($"Level file '{levelFileName}' not found at path: {filePath}");
            return null;
        }

        string json = File.ReadAllText(filePath);
        LevelData levelData = JsonUtility.FromJson<LevelData>(json);
        return levelData;
    }

    public static void SaveCurrentLevelNumber(int levelNumber)
    {
        PlayerPrefs.SetInt(CurrentLevelKey, levelNumber);
        PlayerPrefs.Save();
    }

    public static int LoadCurrentLevelNumber()
    {
       int curLevel = PlayerPrefs.GetInt(CurrentLevelKey, 1);
       if(curLevel == 1)
        SaveCurrentLevelNumber(1);
        return curLevel;
    }

    public static void IncrementLevel()
    {
        int currentLevel = LoadCurrentLevelNumber();
        int nextLevel = currentLevel + 1;
        SaveCurrentLevelNumber(nextLevel);
    }

    public static bool LevelExists(int levelNumber)
    {
        string levelFileName = $"level_{levelNumber:00}.json";
        string filePath = Path.Combine(Application.dataPath, "Levels", levelFileName);
        return File.Exists(filePath);
    }

    public static void ResetLevel()
    {
        SaveCurrentLevelNumber(1);
    }
}
