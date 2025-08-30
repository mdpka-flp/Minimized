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

    private InputDeviceSwitcher inputSwitcher;
    private bool wasUsingGamepadLastFrame;

    [Header("Scale Settings")]
    public float scaleSpeed = 5f;
    public float massSpeed = 5f;
    public float minScale = 1f;
    public float maxScale = 10f;
    public float minMass = 1f;
    public float maxMass = 55f;
    [Tooltip("Геймпад: уменьшение масштаба")]
    public float gamepadScaleStep = 0.1f;
    [Tooltip("Геймпад: скорость изменения масштаба")]
    public float gamepadScaleSpeed = 2f;

    private Vector3 targetScale;
    private float targetMass;
    private float scaleMultiplier = 1.05f;
    private float massMultiplier = 2.2f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        cam = Camera.main;

        inputSwitcher = FindObjectOfType<InputDeviceSwitcher>();
        if (inputSwitcher != null)
        {
            inputSwitcher.OnInputModeChanged += OnInputModeChanged;
            wasUsingGamepadLastFrame = inputSwitcher.IsUsingGamepad;
        }

        if (collisionMode == CollisionMode.AlwaysDisable)
        {
            SetCollisionWithPlayer(false);
        }

        targetScale = transform.localScale;
        targetMass = rb.mass;

        ClampScaleAndMass();
        InitializeColor();
    }

    void OnDestroy()
    {
        if (inputSwitcher != null)
        {
            inputSwitcher.OnInputModeChanged -= OnInputModeChanged;
        }
    }

    void Update()
    {
        if (inputSwitcher != null)
        {
            bool isUsingGamepadNow = inputSwitcher.IsUsingGamepad;

            if (isUsingGamepadNow != wasUsingGamepadLastFrame && isBeingHeld)
            {
                StartCoroutine(DelayedRelease());
            }

            wasUsingGamepadLastFrame = isUsingGamepadNow;
        }

        if (isBeingHeld)
        {
            float scroll = 0f;

            // Обработка мыши (только при использовании мыши)
            if (inputSwitcher == null || !inputSwitcher.IsUsingGamepad)
            {
                scroll = Input.GetAxis("Mouse ScrollWheel");
            }
            // Обработка геймпада (только при использовании геймпада)
            else if (inputSwitcher.IsUsingGamepad)
            {
                // LB - уменьшение
                if (Input.GetButton("LeftBumper"))
                {
                    scroll -= gamepadScaleStep * gamepadScaleSpeed * Time.deltaTime;
                }
                // RB - увеличение
                if (Input.GetButton("RightBumper"))
                {
                    scroll += gamepadScaleStep * gamepadScaleSpeed * Time.deltaTime;
                }
            }

            // Применение изменений
            if (Mathf.Abs(scroll) > 0.01f)
            {
                float scaleFactor = Mathf.Pow(scaleMultiplier, scroll * 10f);
                Vector3 newScale = targetScale * scaleFactor;
                float sizeRatio = newScale.x / transform.localScale.x;

                targetScale = newScale;
                targetMass *= Mathf.Pow(sizeRatio, 2.5f);

                ClampScaleAndMass();
            }
        }

        InitializeColor();
    }

    void FixedUpdate()
    {
        if (isBeingHeld && followTarget != null &&
            inputSwitcher != null && inputSwitcher.IsUsingGamepad)
        {
            Vector2 direction = (followTarget.position - transform.position);
            rb.linearVelocity = direction * followSpeed;
        }

        // Увеличиваем размер
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            scaleSpeed * Time.fixedDeltaTime
        );

        // Увеличмваем массу
        rb.mass = Mathf.Lerp(
            rb.mass,
            targetMass,
            massSpeed * Time.fixedDeltaTime
        );
    }

    // Ограничение минимальной и максимальной массы
    private void ClampScaleAndMass()
    {
        float clampedScale = Mathf.Clamp(targetScale.x, minScale, maxScale);
        targetScale = new Vector3(clampedScale, clampedScale, clampedScale);

        float sizeRatio = clampedScale / transform.localScale.x;
        targetMass = Mathf.Clamp(
            rb.mass * Mathf.Pow(sizeRatio, 2.5f),
            minMass,
            maxMass
        );
    }

    // По клику мышью на объект
    void OnMouseDown()
    {
        if (inputSwitcher != null && inputSwitcher.IsUsingGamepad) return;
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
    }

    // При передвижении мыши меняем позицию jointна позицию мышки
    void OnMouseDrag()
    {
        if (inputSwitcher != null && inputSwitcher.IsUsingGamepad) return;

        if (mouseJoint != null)
        {
            Vector2 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
            mouseJoint.target = mouseWorld;
        }
    }

    // Когда отпустили ЛКМ
    void OnMouseUp()
    {
        if (inputSwitcher != null && inputSwitcher.IsUsingGamepad) return;

        ReleaseObject();
    }

    // Подбор объекта с геймпада
    public void Grab(Transform target)
    {
        if (inputSwitcher != null && !inputSwitcher.IsUsingGamepad) return;
        if (target == null || rb == null || mouseJoint != null) return;

        followTarget = target;
        isBeingHeld = true;
        rb.gravityScale = 0f;

        if (collisionMode == CollisionMode.DisableWhenHeld)
        {
            SetCollisionWithPlayer(false);
        }
    }

    // Отпускаем объект с геймпада
    public void Release()
    {
        if (inputSwitcher != null && !inputSwitcher.IsUsingGamepad) return;

        ReleaseObject();
    }

    // Отпускаем объект если изменилось устройство ввода
    private void OnInputModeChanged(bool isGamepadMode)
    {
        if (isBeingHeld)
        {
            StartCoroutine(DelayedRelease());
        }
    }

    private IEnumerator DelayedRelease()
    {
        yield return new WaitForSeconds(0.1f);
        ReleaseObject();
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
}