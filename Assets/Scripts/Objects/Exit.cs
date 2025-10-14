using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class Exit : MonoBehaviour
{
    [Header("Телепортация")]
    public bool loadNewScene = true;
    public string nextSceneName = "Level2";
    public Transform teleportTarget;

    [Header("Визуальные эффекты")]
    public Image fadePanel;
    public float fadeDuration = 0.5f;
    public float attractDuration = 0.3f;

    [Header("Эффект уменьшения")]
    [Tooltip("Минимальный масштаб игрока во время телепорта")]
    public Vector3 minScale = new Vector3(0.2f, 0.2f, 0.2f);
    [Tooltip("Время уменьшения (лучше = attractDuration)")]
    public float shrinkDuration = 0.3f;

    [Header("Состояние портала")]
    public bool isOpen = false;
    public SpriteRenderer portalSprite;

    private bool isTeleporting = false;
    private Collider2D triggerCollider;

    [Tooltip("Если 0 — работает в старом режиме: любая кнопка открывает/закрывает напрямую. Если >1 — требуется активация всех кнопок.")]
    public int requiredButtonCount = 0;

    private int activeButtonCount = 0;

    void Start()
    {
        triggerCollider = GetComponent<Collider2D>();
        if (triggerCollider != null)
            triggerCollider.enabled = false;

        UpdatePortalVisuals();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && isOpen && !isTeleporting)
        {
            StartCoroutine(TeleportSequence(other.transform));
        }
    }

    public void OpenPortal()
    {
        if (requiredButtonCount == 0)
        {
            // Старый режим: сразу открываем
            OpenPortalInternal();
        }
        else
        {
            // Новый режим: увеличиваем счётчик
            activeButtonCount = Mathf.Min(activeButtonCount + 1, requiredButtonCount);
            CheckPortalState();
        }
    }

    public void ClosePortal()
    {
        if (requiredButtonCount == 0)
        {
            // Старый режим: сразу закрываем
            ClosePortalInternal();
        }
        else
        {
            // Новый режим: уменьшаем счётчик
            activeButtonCount = Mathf.Max(activeButtonCount - 1, 0);
            CheckPortalState();
        }
    }

    private void CheckPortalState()
    {
        bool shouldOpen = (activeButtonCount >= requiredButtonCount);
        if (shouldOpen && !isOpen)
        {
            OpenPortalInternal();
        }
        else if (!shouldOpen && isOpen)
        {
            ClosePortalInternal();
        }
    }

    private void OpenPortalInternal()
    {
        isOpen = true;
        if (triggerCollider != null) triggerCollider.enabled = true;
        UpdatePortalVisuals();
        Debug.Log("Portal opened!");
    }

    private void ClosePortalInternal()
    {
        isOpen = false;
        if (triggerCollider != null) triggerCollider.enabled = false;
        UpdatePortalVisuals();
        Debug.Log("Portal closed!");
    }

    private void UpdatePortalVisuals()
    {
        if (portalSprite != null)
        {
            Color c = portalSprite.color;
            c.a = isOpen ? 1f : 0.3f;
            portalSprite.color = c;
        }
    }

    private IEnumerator TeleportSequence(Transform player)
    {
        isTeleporting = true;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false;

        Vector3 portalCenter = transform.position;
        Vector3 startPos = player.position;
        Vector3 startScale = player.localScale;

        float elapsedAttract = 0f;
        float elapsedShrink = 0f;

        // Главный цикл: одновременно притягиваем И уменьшаем
        float totalTime = Mathf.Max(attractDuration, shrinkDuration);
        while (elapsedAttract < totalTime)
        {
            // Притягивание к порталу
            if (elapsedAttract < attractDuration)
            {
                player.position = Vector3.Lerp(startPos, portalCenter, elapsedAttract / attractDuration);
            }

            // Уменьшение масштаба
            if (elapsedShrink < shrinkDuration)
            {
                elapsedShrink += Time.deltaTime;
                float t = elapsedShrink / shrinkDuration;
                player.localScale = Vector3.Lerp(startScale, minScale, t);
            }

            elapsedAttract += Time.deltaTime;
            yield return null;
        }

        // Убедимся, что игрок точно в центре и в минимальном масштабе
        player.position = portalCenter;
        player.localScale = minScale;

        // >>> ИСПРАВЛЕНИЕ: Fade только при загрузке новой сцены <<<
        if (loadNewScene)
        {
            yield return StartCoroutine(FadeToBlack(fadeDuration));
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            // Без фейда — сразу телепортируем и восстанавливаем
            player.position = teleportTarget.position;
            player.localScale = startScale;

            if (rb != null) rb.simulated = true;
            isTeleporting = false;
        }
    }

    private IEnumerator FadeToBlack(float duration)
    {
        if (fadePanel == null)
        {
            Debug.LogError("fadePanel is not assigned in Exit.cs!");
            yield break;
        }

        fadePanel.gameObject.SetActive(true);
        float elapsed = 0f;
        Color color = fadePanel.color;
        while (elapsed < duration)
        {
            color.a = Mathf.Lerp(0f, 1f, elapsed / duration);
            fadePanel.color = color;
            elapsed += Time.deltaTime;
            yield return null;
        }
        fadePanel.color = new Color(color.r, color.g, color.b, 1f);
    }
}