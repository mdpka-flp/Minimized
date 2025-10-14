using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelDisplay : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Animation Settings")]
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float visibleDuration = 2f;

    private void Start()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        string levelNumber = "";
        foreach (char c in sceneName)
        {
            if (char.IsDigit(c)) levelNumber += c;
        }

        levelText.text = $"Current lvl: {levelNumber}";

        StartCoroutine(ShowLevelText());
    }

    private IEnumerator ShowLevelText()
    {
        // Плавное появление
        yield return StartCoroutine(Fade(0f, 1f, fadeDuration));

        // Небольшая пауза (надпись видна)
        yield return new WaitForSeconds(visibleDuration);

        // Плавное исчезновение
        yield return StartCoroutine(Fade(1f, 0f, fadeDuration));
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, time / duration);
            yield return null;
        }
        canvasGroup.alpha = to;
    }
}
