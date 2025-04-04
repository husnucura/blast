using System.IO;
using UnityEngine;
public class LevelLoader
{
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
}
