using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro; // если используешь TextMeshPro
using UnityEngine.Localization.Settings;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ButtonManager : MonoBehaviour
{
    [Header("Level Generation")]
    public GameObject levelButtonPrefab; // Префаб кнопки
    public Transform contentParent;      // Контейнер внутри ScrollView (обычно Content)
    public int maxLevelCount = 100;      // Максимум уровней для проверки
    public GameObject LevelsPanel;
    public GameObject SettingsPanel;
    public GameObject ExitPanel;
    public GameObject LanguagePanel;

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
        int columns = 4;
        float startX = -370f;
        float startY = 115f;
        float stepX = 250f;
        float stepY = 230f;

        int row = 0;
        int col = 0;

        for (int i = 1; i <= maxLevelCount; i++)
        {
            string levelName = "Lvl" + i;

            if (IsSceneInBuild(levelName))
            {
                GameObject newButton = Instantiate(levelButtonPrefab, contentParent);

                // Установим позицию в локальных координатах
                RectTransform rt = newButton.GetComponent<RectTransform>();
                if (rt != null)
                {
                    float posX = startX + stepX * col;
                    float posY = startY - stepY * row;
                    rt.anchoredPosition = new Vector2(posX, posY);
                }

                // Установим текст
                TMP_Text buttonText = newButton.GetComponentInChildren<TMP_Text>();
                if (buttonText != null)
                    buttonText.text = levelName;

                // Кнопка загружает нужный уровень
                Button btn = newButton.GetComponent<Button>();
                if (btn != null)
                {
                    string capturedLevelName = levelName;
                    btn.onClick.AddListener(() => LoadLevel(capturedLevelName));
                }

                // Продвигаем колонку и строку
                col++;
                if (col >= columns)
                {
                    col = 0;
                    row++;
                }
            }
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
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
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

    public void SwitchLanguage(int index)
    {
        switch (index)
        {
            case 0:
                SwitchToEnglish();
                break;
            case 1:
                SwitchToRussian();
                break;
        }
    }
}
