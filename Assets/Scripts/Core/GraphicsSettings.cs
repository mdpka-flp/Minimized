using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Rendering;
using System.Linq;

public class GraphicsSettings : MonoBehaviour
{
    [Header("Quality Settings")]
    public TMP_Dropdown qualityDropdown;

    [Header("Resolution Settings")]
    public TMP_Dropdown resolutionDropdown;
    private Resolution[] resolutions;
    private List<Resolution> filteredResolutions;

    [Header("Screen Mode Settings")]
    public TMP_Dropdown screenModeDropdown;

    [Header("Post Processing")]
    public TMP_Dropdown postProcessingDropdown;
    public Volume globalVolume;

    void Start()
    {
        InitializeQualityDropdown();
        InitializeResolutionDropdown();
        InitializeScreenModeDropdown();
        InitializePostProcessingDropdown();
    }

    // Инициализация выбора качества графики
    void InitializeQualityDropdown()
    {
        qualityDropdown.ClearOptions();

        List<string> options = new List<string>();
        string[] qualityNames = QualitySettings.names;

        foreach (string name in qualityNames)
        {
            options.Add(name);
        }

        qualityDropdown.AddOptions(options);
        qualityDropdown.value = QualitySettings.GetQualityLevel();
        qualityDropdown.RefreshShownValue();
    }

    // Инициализация выбора разрешения
    void InitializeResolutionDropdown()
    {
        resolutions = Screen.resolutions;
        filteredResolutions = new List<Resolution>();

        // Фильтрация дубликатов
        foreach (Resolution res in resolutions)
        {
            if (!filteredResolutions.Exists(r => r.width == res.width && r.height == res.height))
            {
                filteredResolutions.Add(res);
            }
        }

        // Сортируем разрешения от большего к меньшему
        filteredResolutions = filteredResolutions
            .OrderByDescending(r => r.width)
            .ThenByDescending(r => r.height)
            .ToList();

        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < filteredResolutions.Count; i++)
        {
            string option = $"{filteredResolutions[i].width} x {filteredResolutions[i].height}";
            options.Add(option);

            if (filteredResolutions[i].width == Screen.currentResolution.width &&
                filteredResolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    // Инициализация выбора режима экрана
    void InitializeScreenModeDropdown()
    {
        screenModeDropdown.ClearOptions();
        screenModeDropdown.AddOptions(new List<string> { "On", "Off", "Windowed" });

        switch (Screen.fullScreenMode)
        {
            case FullScreenMode.ExclusiveFullScreen:
                screenModeDropdown.value = 0;
                break;
            case FullScreenMode.Windowed:
                screenModeDropdown.value = 1;
                break;
            case FullScreenMode.FullScreenWindow:
                screenModeDropdown.value = 2;
                break;
        }
        screenModeDropdown.RefreshShownValue();
    }

    // Инициализация выбора постобработки
    void InitializePostProcessingDropdown()
    {
        postProcessingDropdown.ClearOptions();
        postProcessingDropdown.AddOptions(new List<string> { "On", "Off" });

        if (globalVolume != null)
        {
            // Проверяем, включен ли Volume
            postProcessingDropdown.value = globalVolume.enabled ? 0 : 1;
        }
        else
        {
            postProcessingDropdown.value = 0;
            Debug.LogWarning("Global Volume не назначен!");
        }
        postProcessingDropdown.RefreshShownValue();
    }

    // === ОБРАБОТЧИКИ ИЗМЕНЕНИЙ ===

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    public void SetResolution(int index)
    {
        Resolution resolution = filteredResolutions[index];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void SetScreenMode(int index)
    {
        switch (index)
        {
            case 0: // Fullscreen
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                break;
            case 1: // Windowed
                Screen.fullScreenMode = FullScreenMode.Windowed;
                break;
            case 2: // Borderless
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                break;
        }
    }

    public void SetPostProcessing(int index)
    {
        if (globalVolume != null)
        {
            // Просто включаем/выключаем весь Volume
            globalVolume.enabled = (index == 0);
        }
    }
}