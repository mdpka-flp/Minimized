using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class LevelEditor : MonoBehaviour
{
    [Tooltip("Prefab to instantiate")]
    public GameObject objectToSpawn;

    [Tooltip("Camera used for editing")]
    public Camera editorCamera;

    [Tooltip("Sprite to use for eraser preview")]
    public Sprite eraserPreviewSprite;

    [Tooltip("Layers for different object types")]
    public LayerMask draggableLayer;
    public LayerMask wallLayer;
    public LayerMask objectLayer;

    [Tooltip("Layer that blocks spawning")]
    public LayerMask blockingLayer;

    [Header("Camera Controls")]
    public float dragSpeed = 0.5f;
    public float zoomSpeed = 1f;
    public float minZoom = 1f;
    public float maxZoom = 10f;

    [Header("Preview Settings")]
    public SpriteRenderer cursorPreview;

    private bool isEraserMode = false;
    private Vector3 lastMousePosition;
    private bool isDraggingCamera = false;

    // Список для отслеживания всех созданных в редакторе объектов
    private List<GameObject> editorObjects = new List<GameObject>();

    void Start()
    {
        if (editorCamera == null)
        {
            editorCamera = Camera.main;
        }

        UpdateCursorPreview();
    }

    void Update()
    {
        HandleCameraControls();
        UpdateCursorPosition();

        if (Input.GetMouseButton(0) && !IsPointerOverUI())
        {
            if (isEraserMode)
            {
                EraseObjectAtMousePosition();
            }
            else
            {
                SpawnPrefabAtMousePosition();
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            SetEraserMode(!isEraserMode);
        }
    }

    private void HandleCameraControls()
    {
        // Перемещение камеры при зажатом колёсике
        if (Input.GetMouseButtonDown(2))
        {
            isDraggingCamera = true;
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(2) && isDraggingCamera)
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            Vector3 move = new Vector3(-delta.x, -delta.y, 0) * dragSpeed * editorCamera.orthographicSize / 10f;
            editorCamera.transform.Translate(move);
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(2))
        {
            isDraggingCamera = false;
        }

        // Масштабирование колёсиком мыши
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            float newSize = editorCamera.orthographicSize - scroll * zoomSpeed;
            editorCamera.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
        }
    }

    public void ChangeObjectToSpawn(GameObject newObject)
    {
        objectToSpawn = newObject;
        SetEraserMode(false);
    }

    private void UpdateCursorPosition()
    {
        if (cursorPreview == null || editorCamera == null || isDraggingCamera)
            return;

        Vector3 worldPos = editorCamera.ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = 0f;

        Vector3 snappedPos = new Vector3(
            Mathf.Round(worldPos.x),
            Mathf.Round(worldPos.y),
            Mathf.Round(worldPos.z)
        );

        cursorPreview.transform.position = snappedPos;
    }

    private void UpdateCursorPreview()
    {
        if (cursorPreview == null) return;

        if (isEraserMode)
        {
            cursorPreview.sprite = eraserPreviewSprite;
            cursorPreview.color = new Color(0.7f, 0.7f, 0.7f, 0.6f); // Серый для ластика
        }
        else
        {
            // Для режима размещения используем простой квадрат
            cursorPreview.sprite = CreateSimpleSquareSprite();
            cursorPreview.color = new Color(0.7f, 0.7f, 0.7f, 0.4f); // Серый для размещения
        }
    }

    private Sprite CreateSimpleSquareSprite()
    {
        // Создаем простой квадратный спрайт
        int size = 32;
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];

        // Заполняем прозрачным
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }

        // Рисуем контур
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                if (x < 2 || x >= size - 2 || y < 2 || y >= size - 2)
                {
                    texture.SetPixel(x, y, Color.white);
                }
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32);
    }

    private void SpawnPrefabAtMousePosition()
    {
        if (objectToSpawn == null || editorCamera == null)
        {
            Debug.LogWarning("Prefab or EditorCamera is not assigned.");
            return;
        }

        Vector3 spawnPos = GetMouseSnappedPosition();

        // Проверка блокирующего слоя
        if (Physics2D.OverlapPoint(spawnPos, blockingLayer))
        {
            return;
        }

        // Проверка всех редактируемых слоёв
        if (IsPositionOccupied(spawnPos))
        {
            return;
        }

        GameObject spawnedObject = Instantiate(objectToSpawn, spawnPos, Quaternion.identity);

        // Добавляем в список редакторских объектов
        editorObjects.Add(spawnedObject);

        // Отключаем физику и другие компоненты для объектов в редакторе
        DisablePhysicsAndScripts(spawnedObject);
    }

    private void EraseObjectAtMousePosition()
    {
        if (editorCamera == null) return;

        Vector3 erasePos = GetMouseSnappedPosition();
        DestroyObjectsAtPosition(erasePos);
    }

    private Vector3 GetMouseSnappedPosition()
    {
        Vector3 worldPos = editorCamera.ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = 0f;
        return new Vector3(
            Mathf.Round(worldPos.x),
            Mathf.Round(worldPos.y),
            Mathf.Round(worldPos.z)
        );
    }

    private bool IsPositionOccupied(Vector3 position)
    {
        // Проверяем коллайдеры
        if (Physics2D.OverlapPoint(position, draggableLayer | wallLayer | objectLayer))
            return true;

        // Дополнительная проверка для объектов без коллайдеров
        return CheckForObjectsWithoutColliders(position);
    }

    private bool CheckForObjectsWithoutColliders(Vector3 position)
    {
        float checkRadius = 0.3f;

        // Проверяем объекты из нашего списка редакторских объектов
        foreach (GameObject obj in editorObjects)
        {
            if (obj == null) continue;

            if (Vector3.Distance(obj.transform.position, position) < checkRadius)
            {
                return true;
            }
        }

        return false;
    }

    private void DestroyObjectsAtPosition(Vector3 position)
    {
        float checkRadius = 0.3f;

        // Создаем временный список для удаления
        List<GameObject> toRemove = new List<GameObject>();

        // Уничтожаем объекты с коллайдерами
        Collider2D[] colliders = Physics2D.OverlapPointAll(position, draggableLayer | wallLayer | objectLayer);
        foreach (var col in colliders)
        {
            toRemove.Add(col.gameObject);
            Destroy(col.gameObject);
        }

        // Уничтожаем объекты без коллайдеров из нашего списка
        foreach (GameObject obj in editorObjects)
        {
            if (obj == null) continue;

            if (Vector3.Distance(obj.transform.position, position) < checkRadius)
            {
                toRemove.Add(obj);
                Destroy(obj);
            }
        }

        // Удаляем уничтоженные объекты из списка
        foreach (GameObject obj in toRemove)
        {
            editorObjects.Remove(obj);
        }
    }

    private void DisablePhysicsAndScripts(GameObject obj)
    {
        // Отключаем Rigidbody2D если есть
        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = false;
        }

        // Отключаем скрипты перетаскивания если есть
        MonoBehaviour[] scripts = obj.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script.GetType().Name.ToLower().Contains("drag"))
            {
                script.enabled = false;
            }
        }
    }

    public void SetEraserMode(bool enabled)
    {
        isEraserMode = enabled;
        UpdateCursorPreview();
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

    // Очистка списка при уничтожении
    void OnDestroy()
    {
        editorObjects.Clear();
    }
}