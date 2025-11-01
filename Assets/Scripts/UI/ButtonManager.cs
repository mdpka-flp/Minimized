using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization.Settings;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ButtonManager : MonoBehaviour
{
    [Header("Level Generation")]
    public GameObject activeLevelButtonPrefab;   // Рабочая кнопка
    public GameObject lockedLevelButtonPrefab;   // Кнопка с замком
    public Transform contentParent;      // Сюда кнопки генерировать
    public GameObject LevelsPanel;
    public GameObject SettingsPanel;
    public GameObject ExitPanel;
    public GameObject LanguagePanel;
    public TextMeshProUGUI langButton;

    private int currentLocaleIndex = 0;

    // Существующие методы
    public void Play()
    {
        LevelsPanel.SetActive(true);
        GenerateLevelButtons();
    }

    public void Settings()
    {
        SettingsPanel.SetActive(true);
    }

    public void Exit()
    {
        ExitPanel.SetActive(true);
    }

    public void Leave()
    {
        Application.Quit();
        #if UNITY_EDITOR
        EditorApplication.isPlaying = false;
        #endif
    }

    public void BackToMainMenu()
    {
        LevelsPanel.SetActive(false);
        SettingsPanel.SetActive(false);
        ExitPanel.SetActive(false);
        LanguagePanel.SetActive(false);
        ClearLevelButtons();
    }

    private void GenerateLevelButtons()
    {
        // Очистка старых кнопок
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        HashSet<int> unlockedLevels = LevelProgressManager.LoadUnlockedLevels();

        // Собираем все подходящие уровни из Build Settings
        List<(int levelNumber, string sceneName)> validLevels = new List<(int, string)>();

        int sceneCount = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < sceneCount; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(path);

            if (sceneName.StartsWith("Lvl", System.StringComparison.OrdinalIgnoreCase))
            {
                string numberPart = sceneName.Substring(3);
                if (int.TryParse(numberPart, out int levelNum) && levelNum > 0)
                {
                    validLevels.Add((levelNum, sceneName));
                }
            }
        }

        // Сортируем по номеру уровня
        validLevels.Sort((a, b) => a.levelNumber.CompareTo(b.levelNumber));

        // Параметры размещения — теперь с учётом 1000px ширины
        const int levelsInRow = 4;
        const float buttonWidth = 150f;
        const float buttonHeight = 150f;
        const float offsetX = 250f; // Шаг по X — чтобы 4 кнопки заняли 750px
        const float offsetY = 200f; // Шаг по Y
        const float startX = -875f; // Центрирование: 3×250=750 → отступ слева 125px → -375
        const float startY = 180f;  // Начальная Y позиция — чуть ниже, чтобы не уезжали вверх

        // Создаём кнопки
        for (int i = 0; i < validLevels.Count; i++)
        {
            var (levelNum, sceneName) = validLevels[i];
            bool isUnlocked = unlockedLevels.Contains(levelNum);
            GameObject prefabToUse = isUnlocked ? activeLevelButtonPrefab : lockedLevelButtonPrefab;
            GameObject buttonObj = Instantiate(prefabToUse, contentParent);

            // Устанавливаем текст
            var textComponent = buttonObj.GetComponentInChildren<Text>();
            if (textComponent != null)
            {
                textComponent.text = "Lvl " + levelNum;
            }
            else
            {
                var tmp = buttonObj.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.text = "Lvl " + levelNum;
                }
            }

            // Настраиваем кликабельность
            var button = buttonObj.GetComponent<Button>();
            if (button != null)
            {
                if (isUnlocked)
                {
                    button.interactable = true;
                    string capturedScene = sceneName;
                    button.onClick.AddListener(() => LoadLevel(capturedScene));
                }
                else
                {
                    button.interactable = false;
                }
            }

            // Позиционируем через anchoredPosition
            int row = i / levelsInRow;
            int col = i % levelsInRow;
            float posX = startX + col * offsetX;
            float posY = startY - row * offsetY;

            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            if (buttonRect != null)
            {
                buttonRect.anchoredPosition = new Vector2(posX, posY);
                buttonRect.sizeDelta = new Vector2(buttonWidth, buttonHeight);
                buttonRect.localScale = Vector3.one;
            }
        }

        // Обновляем размер контента для Scroll View
        UpdateContentSize(validLevels.Count, levelsInRow, offsetX, offsetY);
    }

    // Метод для обновления размера контента Scroll View
    private void UpdateContentSize(int totalButtons, int levelsInRow, float offsetX, float offsetY)
    {
        if (contentParent == null) return;

        int rows = Mathf.CeilToInt((float)totalButtons / levelsInRow);
        float contentWidth = levelsInRow * offsetX;
        float contentHeight = rows * offsetY;

        RectTransform contentRect = contentParent.GetComponent<RectTransform>();
        if (contentRect != null)
        {
            contentRect.sizeDelta = new Vector2(contentWidth, contentHeight);
        }
    }


    private bool IsSceneInBuild(string sceneName)
    {
        int count = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < count; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string scene = System.IO.Path.GetFileNameWithoutExtension(path);
            if (scene == sceneName)
                return true;
        }
        return false;
    }

    private void ClearLevelButtons()
    {
        // Собираем все дочерние объекты в список, чтобы не модифицировать коллекцию во время итерации
        List<GameObject> children = new List<GameObject>();
        foreach (Transform child in contentParent)
        {
            children.Add(child.gameObject);
        }

        // Уничтожаем в конце кадра — чтобы избежать MissingReferenceException
        foreach (var child in children)
        {
            if (child != null)
            {
                Destroy(child);
            }
        }
    }

    private void LoadLevel(string levelName)
    {
        SceneManager.LoadScene(levelName);
    }

    public void LanguageSettings()
    {
        LanguagePanel.SetActive(true);
    }

    public void SwitchToEnglish()
    {
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[0];
    }

    public void SwitchToRussian()
    {
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[1];
    }

    public void SwitchLanguage()
    {
        var locales = LocalizationSettings.AvailableLocales.Locales;
        if (locales == null || locales.Count == 0)
            return;

        // Переключаем на следующую локаль (циклически)
        currentLocaleIndex = (currentLocaleIndex + 1) % locales.Count;
        LocalizationSettings.SelectedLocale = locales[currentLocaleIndex];

        // Обновляем текст кнопки
        if (langButton != null)
        {
            langButton.text = currentLocaleIndex == 0 ? "English" : "Русский";
        }
    }

    public void mdpka()
    {
        Application.OpenURL("https://t.me/mdpkaaa");
    }
    public void SkyFly()
    {
        
    }
    public void malis()
    {
        Application.OpenURL("https://t.me/snow_shaark");
    }

    private void Start()
    {
        var locales = LocalizationSettings.AvailableLocales.Locales;
        if (locales != null && locales.Count > 0)
        {
            currentLocaleIndex = locales.IndexOf(LocalizationSettings.SelectedLocale);
            if (currentLocaleIndex == -1)
                currentLocaleIndex = 0; // fallback на первый язык

            // Обновляем текст кнопки при запуске
            if (langButton != null)
            {
                langButton.text = currentLocaleIndex == 0 ? "English" : "Русский";
            }
        }
        else
        {
            // На случай, если локали ещё не загружены (редко, но бывает)
            if (langButton != null)
                langButton.text = "English";
        }
        GenerateLevelButtons();
    }
}