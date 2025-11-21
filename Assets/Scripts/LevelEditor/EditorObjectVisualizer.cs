using UnityEngine;

public class EditorObjectVisualizer : MonoBehaviour
{
    [Header("Editor Visualization")]
    public Sprite editorSprite;
    public Color editorColor = new Color(0, 1, 1, 0.7f);
    public Vector2 spriteSize = new Vector2(1, 1);

    private SpriteRenderer spriteRenderer;
    private bool isInEditMode = true;

    void Awake()
    {
        // Проверяем, находимся ли мы в режиме редактирования
        isInEditMode = FindObjectOfType<LevelEditor>() != null;

        if (isInEditMode)
        {
            CreateVisualization();
        }
    }

    void Start()
    {
        // Дополнительная проверка на случай, если Awake не сработал
        if (spriteRenderer == null && FindObjectOfType<LevelEditor>() != null)
        {
            CreateVisualization();
        }
    }

    private void CreateVisualization()
    {
        // Добавляем SpriteRenderer для редактора
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

        // Устанавливаем спрайт
        if (editorSprite != null)
        {
            spriteRenderer.sprite = editorSprite;
        }
        else
        {
            CreateDefaultSprite();
        }

        spriteRenderer.color = editorColor;
        spriteRenderer.sortingOrder = -1; // Позади других объектов

        // Масштабируем под нужный размер
        if (spriteRenderer.sprite != null)
        {
            Vector2 nativeSize = spriteRenderer.sprite.bounds.size;
            if (nativeSize.x > 0 && nativeSize.y > 0)
            {
                transform.localScale = new Vector3(
                    spriteSize.x / nativeSize.x,
                    spriteSize.y / nativeSize.y,
                    1
                );
            }
        }
    }

    private void CreateDefaultSprite()
    {
        int textureSize = 64;
        Texture2D tex = new Texture2D(textureSize, textureSize);

        // Заполняем прозрачным
        Color[] clearPixels = new Color[textureSize * textureSize];
        for (int i = 0; i < clearPixels.Length; i++)
        {
            clearPixels[i] = Color.clear;
        }
        tex.SetPixels(clearPixels);

        // Рисуем контур
        for (int i = 0; i < textureSize; i++)
        {
            // Внешний контур
            if (i < 4 || i >= textureSize - 4)
            {
                for (int j = 0; j < textureSize; j++)
                {
                    tex.SetPixel(i, j, editorColor);
                    tex.SetPixel(j, i, editorColor);
                }
            }

            // Диагональный крест
            if (Mathf.Abs(i - textureSize / 2) < 2)
            {
                for (int j = 0; j < textureSize; j++)
                {
                    if (Mathf.Abs(j - textureSize / 2) < 2)
                    {
                        tex.SetPixel(i, j, editorColor);
                    }
                }
            }
        }

        tex.Apply();
        tex.filterMode = FilterMode.Point;

        editorSprite = Sprite.Create(tex, new Rect(0, 0, textureSize, textureSize), Vector2.one * 0.5f, textureSize);
        spriteRenderer.sprite = editorSprite;
    }

    // Удаляем визуализацию при запуске игры
    void OnEnable()
    {
        if (!Application.isEditor || (Application.isPlaying && !isInEditMode))
        {
            if (spriteRenderer != null)
            {
                Destroy(spriteRenderer);
            }
            Destroy(this);
        }
    }
}