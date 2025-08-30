using UnityEngine;
using System.Collections;

public class ShatterManager : MonoBehaviour
{
    public GameManager gameManager;

    public GameObject cubePiecePrefab;
    public GameObject playerPiecePrefab;

    [Header("Осколки")]
    public int minPieces = 5;
    public int maxPieces = 12;
    public float minSize = 0.2f;
    public float maxSize = 0.5f;
    public float force = 200f;
    public float lifetime = 1f;

    // Генерация осколков куба
    public void BreakCube(Vector3 position, Color color)
    {
        int pieces = Random.Range(minPieces, maxPieces);
        for (int i = 0; i < pieces; i++)
        {
            SpawnPiece(cubePiecePrefab, position, color);
        }
    }

    // Генерация осколков игрока
    public void BreakPlayer(Vector3 position)
    {
        int pieces = Random.Range(minPieces + 3, maxPieces + 5); // больше кусочков
        for (int i = 0; i < pieces; i++)
        {
            SpawnPiece(playerPiecePrefab, position, Color.white);
        }

        StartCoroutine(RestartAfterDelay(lifetime + 0.1f));
    }

    private void SpawnPiece(GameObject prefab, Vector3 position, Color color)
    {
        GameObject piece = Instantiate(prefab, position, Quaternion.identity);

        // Случайный размер
        float sizeX = Random.Range(minSize, maxSize);
        float sizeY = Random.Range(minSize, maxSize);
        piece.transform.localScale = new Vector3(sizeX, sizeY, 1);

        // Цвет
        SpriteRenderer sr = piece.GetComponent<SpriteRenderer>();
        sr.color = color;

        // Rigidbody и разлет
        Rigidbody2D rb = piece.GetComponent<Rigidbody2D>();
        float angle = Random.Range(0f, 360f);
        float speed = Random.Range(force * 0.5f, force);
        Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
        rb.AddForce(direction * speed);
        rb.angularVelocity = Random.Range(-360f, 360f);

        // Плавное исчезновение
        StartCoroutine(FadeOut(piece, sr, lifetime));
    }

    private IEnumerator FadeOut(GameObject piece, SpriteRenderer sr, float duration)
    {
        float elapsed = 0f;
        Color originalColor = sr.color;
        Vector3 originalScale = piece.transform.localScale;

        // Можно добавить случайную задержку, чтобы кусочки исчезали не одновременно
        float delay = Random.Range(0f, 0.2f);
        yield return new WaitForSeconds(delay);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Альфа Fade
            sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1 - t);

            // Scale Fade (кусочек уменьшается)
            piece.transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);

            yield return null;
        }

        Destroy(piece);
    }

    private IEnumerator RestartAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (gameManager != null)
            gameManager.RestartLevel();
    }
}
