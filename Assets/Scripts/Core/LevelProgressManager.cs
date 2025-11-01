// LevelProgressManager.cs
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelProgressManager : MonoBehaviour
{
    private const string FILE_NAME = "unlocked_levels.txt";
    private static string FilePath => Path.Combine(Application.persistentDataPath, FILE_NAME);

    private void Awake()
    {
        // Убедимся, что объект не уничтожается
        DontDestroyOnLoad(gameObject);

        // Подписываемся на событие загрузки сцены
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // Отписываемся, чтобы избежать утечек
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Проверяем, является ли сцена уровнем
        string sceneName = scene.name;

        if (sceneName.StartsWith("Lvl", System.StringComparison.OrdinalIgnoreCase))
        {
            string numberPart = sceneName.Substring(3);
            if (int.TryParse(numberPart, out int levelNum) && levelNum > 0)
            {
                HashSet<int> unlocked = LoadUnlockedLevels();
                if (unlocked.Add(levelNum)) // true, если добавлено впервые
                {
                    SaveUnlockedLevels(unlocked);
                    Debug.Log($"Уровень {levelNum} автоматически разблокирован при входе в сцену '{sceneName}'.");
                }
            }
        }
    }

    /// <summary>
    /// Загружает список разблокированных уровней из файла.
    /// Уровень 1 всегда включён.
    /// </summary>
    public static HashSet<int> LoadUnlockedLevels()
    {
        HashSet<int> levels = new HashSet<int>();

        if (File.Exists(FilePath))
        {
            string[] lines = File.ReadAllLines(FilePath);
            foreach (string line in lines)
            {
                if (int.TryParse(line.Trim(), out int level))
                {
                    levels.Add(level);
                }
            }
        }

        levels.Add(1); // Уровень 1 всегда доступен
        return levels;
    }

    /// <summary>
    /// Сохраняет набор разблокированных уровней в файл.
    /// Уровень 1 всегда сохраняется.
    /// </summary>
    private static void SaveUnlockedLevels(HashSet<int> levels)
    {
        levels.Add(1);

        List<string> lines = new List<string>();
        foreach (int level in levels)
        {
            lines.Add(level.ToString());
        }
        File.WriteAllLines(FilePath, lines);
    }
}