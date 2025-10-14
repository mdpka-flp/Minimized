// MassLabel.cs
using UnityEngine;
using TMPro;
using System.Collections;

public class MassLabel : MonoBehaviour
{
    [SerializeField] private TMP_Text textComponent;
    private Rigidbody2D targetRigidbody;
    [SerializeField] private float fadeDuration = 0.2f; // врем€ по€влени€/исчезновени€ в секундах

    private Coroutine fadeCoroutine;

    public void Initialize(Rigidbody2D rb)
    {
        if (rb == null)
        {
            Debug.LogError("MassLabel: Cannot initialize with null Rigidbody2D!");
            Destroy(gameObject);
            return;
        }

        targetRigidbody = rb;
        textComponent.color = new Color(textComponent.color.r, textComponent.color.g, textComponent.color.b, 0f); // начать с прозрачного
        fadeCoroutine = StartCoroutine(FadeTo(1f));
    }

    // ¬ызываетс€ из Draggable.cs перед уничтожением
    public void FadeOutAndDestroy()
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeTo(0f, () =>
        {
            Destroy(gameObject);
        }));
    }

    IEnumerator FadeTo(float targetAlpha, System.Action onComplete = null)
    {
        Color startColor = textComponent.color;
        Color targetColor = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            textComponent.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }

        textComponent.color = targetColor;
        onComplete?.Invoke();
    }

    void LateUpdate()
    {
        if (targetRigidbody == null || targetRigidbody.transform == null)
        {
            Destroy(gameObject);
            return;
        }

        Transform target = targetRigidbody.transform;
        float scaleHeight = target.localScale.y;
        Vector3 labelPosition = target.position;
        labelPosition.y -= (scaleHeight / 2f) + 0.3f;
        transform.position = labelPosition;

        if (Camera.main != null)
        {
            transform.LookAt(transform.position + Camera.main.transform.forward);
        }

        // ќбновл€ем текст даже во врем€ анимации
        if (textComponent != null)
        {
            textComponent.text = targetRigidbody.mass.ToString("F1");
        }
    }
}