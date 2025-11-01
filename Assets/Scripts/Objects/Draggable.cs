using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Draggable : MonoBehaviour
{
    public enum CollisionMode
    {
        DisableWhenHeld,
        NeverDisable,
        AlwaysDisable
    }
    public enum ObjColor
    {
        Red,
        Green,
        Blue
    }

    private Rigidbody2D rb;
    private Collider2D col;
    private Transform followTarget;
    private Camera cam;
    private TargetJoint2D mouseJoint;

    [Header("Drag Settings")]
    public float followSpeed = 15f;
    public bool isBeingHeld = false;

    [Header("Collision Settings")]
    public CollisionMode collisionMode = CollisionMode.DisableWhenHeld;
    public LayerMask ignoreCollisionWith;

    [Header("Color Settings")]
    public ObjColor itsColor = ObjColor.Green;
    public SpriteRenderer Object;
    public Color Red = new Color(1f, 0.53f, 0.53f);
    public Color Green = new Color(0.69f, 1f, 0.65f);
    public Color Blue = new Color(0.45f, 0.47f, 1f);

    [Header("Scale Settings")]
    public float scaleSpeed = 5f;
    public float massSpeed = 5f;
    public float minScale = 1f;
    public float maxScale = 2f;
    public float minMass = 5f;
    public float maxMass = 15f;
    private bool isUsingProportionalScale = false; // флаг: перешли ли мы на пропорциональный режим
    private float currentScaleFactor;
    private float targetScaleFactor;
    [Tooltip("How much slower scaling becomes when Ctrl is held")]
    public float fineTuneMultiplier = 0.2f;

    [Header("Mass Label")]
    public GameObject massLabelPrefab;
    private GameObject spawnedMassLabel;

    //private Vector3 targetScale;
    //private float targetMass;
    //private float scaleMultiplier = 1.05f;
    //private float massMultiplier = 2.2f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        cam = Camera.main;

        if (collisionMode == CollisionMode.AlwaysDisable)
        {
            SetCollisionWithPlayer(false);
        }

        float initialScale = transform.localScale.x;
        float initialMass = rb.mass;

        // Ограничиваем, но не привязываем к scaleFactor
        initialScale = Mathf.Clamp(initialScale, minScale, maxScale);
        initialMass = Mathf.Clamp(initialMass, minMass, maxMass);

        transform.localScale = Vector3.one * initialScale;
        rb.mass = initialMass;

        InitializeColor();
    }

    void Update()
    {
        if (isBeingHeld)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                SwitchToProportionalMode();

                float scrollSensitivity = scaleSpeed * 0.2f;

                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                {
                    scrollSensitivity *= fineTuneMultiplier;
                }

                targetScaleFactor += scroll * scrollSensitivity;
                targetScaleFactor = Mathf.Clamp01(targetScaleFactor);
            }
        }

        InitializeColor();
    }

    void FixedUpdate()
    {
        if (isUsingProportionalScale)
        {
            // Плавно интерполируем scaleFactor
            currentScaleFactor = Mathf.Lerp(
                currentScaleFactor,
                targetScaleFactor,
                scaleSpeed * Time.fixedDeltaTime
            );

            // Применяем пропорциональные размер и массу
            float scale = Mathf.Lerp(minScale, maxScale, currentScaleFactor);
            float mass = Mathf.Lerp(minMass, maxMass, currentScaleFactor);

            transform.localScale = Vector3.one * scale;
            rb.mass = mass;
        }

        // Если НЕ в пропорциональном режиме — ничего не делаем (оставляем как есть)
    }

    private void ApplyScaleFromFactor(float factor)
    {
        float clampedFactor = Mathf.Clamp01(factor);
        float currentScale = Mathf.Lerp(minScale, maxScale, clampedFactor);
        float currentMass = Mathf.Lerp(minMass, maxMass, clampedFactor);

        transform.localScale = Vector3.one * currentScale;
        rb.mass = currentMass;
    }

    private void SwitchToProportionalMode()
    {
        if (isUsingProportionalScale) return;

        // Вычисляем scaleFactor на основе ТЕКУЩЕГО размера
        // (можно и на основе массы — но размер визуальнее)
        float currentScale = transform.localScale.x;
        currentScaleFactor = Mathf.InverseLerp(minScale, maxScale, currentScale);
        targetScaleFactor = currentScaleFactor;

        isUsingProportionalScale = true;
    }

    // По клику мышью на объект
    void OnMouseDown()
    {
        if (isBeingHeld) return;

        Vector2 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);

        mouseJoint = gameObject.AddComponent<TargetJoint2D>();
        mouseJoint.autoConfigureTarget = false;
        mouseJoint.dampingRatio = 1f;
        mouseJoint.frequency = 5f;
        mouseJoint.maxForce = 1000f;
        mouseJoint.anchor = mouseJoint.transform.InverseTransformPoint(mouseWorld);
        mouseJoint.target = mouseWorld;

        rb.gravityScale = 0f;
        isBeingHeld = true;

        if (collisionMode == CollisionMode.DisableWhenHeld)
        {
            SetCollisionWithPlayer(false);
        }

        if (massLabelPrefab != null && spawnedMassLabel == null)
        {
            spawnedMassLabel = Instantiate(massLabelPrefab, transform.position, Quaternion.identity);
            spawnedMassLabel.GetComponent<MassLabel>().Initialize(rb);
        }
    }

    // При передвижении мыши меняем позицию jointна позицию мышки
    void OnMouseDrag()
    {
        if (mouseJoint != null)
        {
            Vector2 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
            mouseJoint.target = mouseWorld;
        }
    }

    // Когда отпустили ЛКМ
    void OnMouseUp()
    {
        ReleaseObject();

        if (spawnedMassLabel != null)
        {
            MassLabel label = spawnedMassLabel.GetComponent<MassLabel>();
            if (label != null)
            {
                label.FadeOutAndDestroy();
            }
            else
            {
                Destroy(spawnedMassLabel); // на случай, если что-то пошло не так
            }
            spawnedMassLabel = null;
        }
    }

    // Отпускаем объект мышкой
    private void ReleaseObject()
    {
        if (mouseJoint != null)
        {
            Destroy(mouseJoint);
            mouseJoint = null;
        }

        if (followTarget != null)
        {
            followTarget = null;
        }

        if (!isBeingHeld) return;

        isBeingHeld = false;
        rb.gravityScale = 1f;

        if (collisionMode == CollisionMode.DisableWhenHeld)
        {
            SetCollisionWithPlayer(true);
        }

        if (spawnedMassLabel != null)
        {
            Destroy(spawnedMassLabel);
            spawnedMassLabel = null;
        }
    }

    private void SetCollisionWithPlayer(bool enable)
    {
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Player"))
        {
            Collider2D playerCol = obj.GetComponent<Collider2D>();
            if (playerCol != null)
            {
                Physics2D.IgnoreCollision(col, playerCol, !enable);
            }
        }
    }

    private void InitializeColor()
    {
        if (itsColor == ObjColor.Red)
        {
            Object.color = Red;
        }
        else if (itsColor == ObjColor.Green)
        {
            Object.color = Green;
        }
        else if (itsColor == ObjColor.Blue)
        {
            Object.color = Blue;
        }
    }

    private void OnValidate()
    {
        InitializeColor();
    }
}