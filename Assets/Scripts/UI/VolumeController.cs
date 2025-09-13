using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using System;

public class VolumeController : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string mixerGroup = "Master";

    [Header("UI Elements")]
    [SerializeField] private Slider volumeSlider;

    [Header("Settings")]
    [SerializeField] private float defaultVolume = 0.8f;
    [SerializeField] private string volumeKey = "MasterVolume";

    private void Start()
    {
        // Загружаем сохранённое значение громкости или используем значение по умолчанию
        float savedVolume = PlayerPrefs.GetFloat(volumeKey, defaultVolume);

        // Устанавливаем значение слайдера
        volumeSlider.value = savedVolume;

        // Применяем громкость к микшеру
        SetVolume(savedVolume);

        // Добавляем обработчик изменения значения слайдера
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
    }

    private void OnVolumeChanged(float volume)
    {
        // При изменении слайдера обновляем громкость
        SetVolume(volume);

        // Сохраняем значение
        SaveVolume(volume);
    }

    private void SetVolume(float volume)
    {
        // Преобразуем линейное значение (0-1) в логарифмическое (dB)
        // Микшер использует логарифмическую шкалу, где 0.0001 = -80dB, 1 = 0dB
        float dB = volume > 0.0001f ? 20f * Mathf.Log10(volume) : -80f;

        // Устанавливаем громкость в микшере
        audioMixer.SetFloat(mixerGroup, dB);
    }

    private void SaveVolume(float volume)
    {
        // Сохраняем значение громкости
        PlayerPrefs.SetFloat(volumeKey, volume);
        PlayerPrefs.Save();
    }

    // Метод для сброса громкости к значению по умолчанию
    public void ResetToDefault()
    {
        volumeSlider.value = defaultVolume;
        SetVolume(defaultVolume);
        SaveVolume(defaultVolume);
    }

    private void OnDestroy()
    {
        // Убираем обработчик при уничтожении объекта
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
        }
    }
}