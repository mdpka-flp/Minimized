using UnityEngine;
using System.Collections;

public class Door : MonoBehaviour
{
    public enum direct
    {
        horizontal,
        vertical
    }
    public enum mode
    {
        move,
        scale
    }

    [Header("Door settings")]
    public direct direction = direct.vertical;
    public mode doorMode = mode.move;
    public bool isOpen;

    [Header("Scale mode")]
    public float closedScale = 1f;
    public float openedScale = 0f;

    [Header("Move mode")]
    public float pos = 1f;

    [Header("Animation")]
    public float duration = 0.5f;

    private Coroutine currentAnimation = null;
    private Vector3 initialPosition;
    private float initialScale;

    void Start()
    {
        initialPosition = transform.position;
        initialScale = GetAxisScale();

        // Устанавливаем начальное состояние без анимации
        if (isOpen)
        {
            ApplyOpenedState();
        }
        else
        {
            ApplyClosedState();
        }
    }

    public void OpenDoor()
    {
        StopCurrentAnimation();
        currentAnimation = StartCoroutine(AnimateToOpened());
    }

    public void CloseDoor()
    {
        StopCurrentAnimation();
        currentAnimation = StartCoroutine(AnimateToClosed());
    }

    private void StopCurrentAnimation()
    {
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
            currentAnimation = null;
        }
    }

    private IEnumerator AnimateToOpened()
    {
        Vector3 startPos = transform.position;
        float startScale = GetAxisScale();

        Vector3 targetPos = initialPosition + GetDirectionVector() * pos;
        float targetScale = openedScale;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float easedT = 1f - (1f - t) * (1f - t); // ease-out

            if (doorMode == mode.move)
                transform.position = Vector3.Lerp(startPos, targetPos, easedT);
            else if (doorMode == mode.scale)
                SetAxisScale(Mathf.Lerp(startScale, targetScale, easedT));

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Убедимся, что достигли конечного состояния
        if (doorMode == mode.move)
            transform.position = targetPos;
        else
            SetAxisScale(targetScale);

        isOpen = true;
        currentAnimation = null;
    }

    private IEnumerator AnimateToClosed()
    {
        Vector3 startPos = transform.position;
        float startScale = GetAxisScale();

        Vector3 targetPos = initialPosition;
        float targetScale = closedScale;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float easedT = 1f - (1f - t) * (1f - t); // ease-out

            if (doorMode == mode.move)
                transform.position = Vector3.Lerp(startPos, targetPos, easedT);
            else if (doorMode == mode.scale)
                SetAxisScale(Mathf.Lerp(startScale, targetScale, easedT));

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (doorMode == mode.move)
            transform.position = targetPos;
        else
            SetAxisScale(targetScale);

        isOpen = false;
        currentAnimation = null;
    }

    // Применяем конечное состояние мгновенно (для Start)
    private void ApplyOpenedState()
    {
        if (doorMode == mode.move)
            transform.position = initialPosition + GetDirectionVector() * pos;
        else
            SetAxisScale(openedScale);
        isOpen = true;
    }

    private void ApplyClosedState()
    {
        if (doorMode == mode.move)
            transform.position = initialPosition;
        else
            SetAxisScale(closedScale);
        isOpen = false;
    }

    private Vector3 GetDirectionVector()
    {
        return direction == direct.horizontal ? Vector3.right : Vector3.up;
    }

    private float GetAxisScale()
    {
        return direction == direct.horizontal ? transform.localScale.x : transform.localScale.y;
    }

    private void SetAxisScale(float scale)
    {
        Vector3 newScale = transform.localScale;
        if (direction == direct.horizontal)
            newScale.x = scale;
        else
            newScale.y = scale;
        transform.localScale = newScale;
    }
}