using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class LevelEditor : MonoBehaviour
{
    [Tooltip("Prefab to instantiate")]
    public GameObject objectToSpawn;

    [Tooltip("Camera used to convert mouse position to world position")]
    public Camera mainCamera;

    [Tooltip("Optional: Separate prefab for preview (if null, uses objectToSpawn)")]
    public GameObject previewPrefab;

    [Tooltip("Sprite to use for eraser preview")]
    public Sprite eraserPreviewSprite;

    [Tooltip("Layer on which objects can be interacted with")]
    public LayerMask objectLayer;

    [Tooltip("Layer that blocks spawning (objects on this layer prevent spawning)")]
    public LayerMask blockingLayer;

    private GameObject previewObject;
    private SpriteRenderer previewSpriteRenderer;

    private bool isEraserMode = false;

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        CreatePreviewObject();
    }

    void Update()
    {
        UpdatePreviewPosition();

        if (Input.GetMouseButton(0))
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

    public void ChangeObjectToSpawn(GameObject newObject)
    {
        objectToSpawn = newObject;
        SetPreviewAppearance();
        SetEraserMode(false);
    }

    private void CreatePreviewObject()
    {
        if (previewPrefab != null)
        {
            previewObject = Instantiate(previewPrefab);
        }
        else if (objectToSpawn != null)
        {
            previewObject = Instantiate(objectToSpawn);
        }
        else
        {
            Debug.LogWarning("No prefab assigned for preview or spawning.");
            return;
        }

        previewSpriteRenderer = previewObject.GetComponent<SpriteRenderer>();
        if (previewSpriteRenderer == null)
        {
            Debug.LogWarning("Preview object does not have a SpriteRenderer component.");
        }

        SetPreviewAppearance();

        foreach (var comp in previewObject.GetComponents<MonoBehaviour>())
        {
            comp.enabled = false;
        }
    }

    private void SetPreviewAppearance()
    {
        previewSpriteRenderer.sprite = objectToSpawn.GetComponent<SpriteRenderer>().sprite;
        if (previewSpriteRenderer == null)
        {
            Debug.LogWarning("Preview object does not have a SpriteRenderer component.");
        }
        else
        {
            Color c = previewSpriteRenderer.color;
            c.a = 0.5f;
            previewSpriteRenderer.color = c;
        }
    }

    private void UpdatePreviewPosition()
    {
        if (previewObject == null || mainCamera == null)
            return;

        Vector3 mouseScreenPos = Input.mousePosition;
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);
        worldPos.z = 0f;

        Vector3 snappedPos = new Vector3(
            Mathf.Round(worldPos.x),
            Mathf.Round(worldPos.y),
            Mathf.Round(worldPos.z)
        );

        previewObject.transform.position = snappedPos;
    }

    private void SpawnPrefabAtMousePosition()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (objectToSpawn == null || mainCamera == null)
        {
            Debug.LogWarning("Prefab or MainCamera is not assigned.");
            return;
        }

        Vector3 spawnPos = previewObject != null ? previewObject.transform.position : Vector3.zero;

        Collider2D[] blockingColliders = Physics2D.OverlapPointAll(spawnPos, blockingLayer);
        if (blockingColliders.Length > 0)
        {
            Debug.Log("Spawn blocked by object on blocking layer.");
            return;
        }

        Collider2D[] collidersAtPos = Physics2D.OverlapPointAll(spawnPos, objectLayer);
        if (collidersAtPos.Length > 0)
        {
            return;
        }

        Instantiate(objectToSpawn, spawnPos, Quaternion.identity);
    }

    private void EraseObjectAtMousePosition()
    {
        if (mainCamera == null)
        {
            Debug.LogWarning("MainCamera is not assigned.");
            return;
        }

        Vector3 erasePos = previewObject != null ? previewObject.transform.position : Vector3.zero;

        Collider2D[] colliders = Physics2D.OverlapPointAll(erasePos, objectLayer);

        if (colliders.Length == 0)
        {
            return;
        }
        else
        {
            foreach (var col in colliders)
            {
                Destroy(col.gameObject);
            }
        }
    }

    public void ChangePreviewSprite(Sprite newSprite)
    {
        if (previewSpriteRenderer != null)
        {
            previewSpriteRenderer.sprite = newSprite;
        }
        else
        {
            Debug.LogWarning("Preview SpriteRenderer is not assigned.");
        }
    }

    public void SetEraserMode(bool enabled)
    {
        isEraserMode = enabled;

        if (previewSpriteRenderer == null)
            return;

        if (isEraserMode)
        {
            if (eraserPreviewSprite != null)
            {
                previewSpriteRenderer.sprite = eraserPreviewSprite;
            }
            else
            {
                Debug.LogWarning("Eraser preview sprite is not assigned.");
            }
        }
        else
        {
            Sprite originalSprite = null;

            if (objectToSpawn != null)
            {
                var sr = objectToSpawn.GetComponent<SpriteRenderer>();
                if (sr != null) originalSprite = sr.sprite;
            }

            if (originalSprite != null)
            {
                previewSpriteRenderer.sprite = originalSprite;
            }
            else
            {
                Debug.LogWarning("Original preview sprite not found to restore.");
            }
        }

    }
}