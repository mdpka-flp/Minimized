//using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public Rigidbody2D rb;
    private Collider2D playerCollider;
    private float Horizontal;
    float triggerValue;

    [Header("Player Movement Settings")]
    [Range(0, 4f)] public float NormalSpeed = 1f;
    [Range(0, 4f)] public float SprintSpeed = 2f;
    [Range(0, 13f)] public float JumpForce = 6f;

    public float speed;
    private float jumpForce;

    [Header("Jump & Wall Detection")]
    public Transform groundCheckPoint;
    public float groundCheckDistance = 0.1f;
    public float groundCheckRadius = 0.2f;

    [Header("Shear & Stratch")]
    public SpriteRenderer spriteRenderer;
    private Material playerMat;
    public float shearSpeed = 10f; // скорость сглаживания
    private float currentShear = 0f; // текущее значение (сглаженное)
    public float stretchSpeed = 10f;
    private float currentStretch = 1f;

    [Header("Wall Slide")]
    private bool isWallSliding;

    public bool isGrounded;
    public Collider2D lastGroundCollider;
    public GameManager gameManager;

    public bool isDead = false;

    private bool IsTouchingWall()
    {
        float direction = Mathf.Sign(Horizontal);
        float extraWidth = 0.05f; // небольшое смещение, чтобы не залипать

        // Делаем BoxCast равный размеру коллайдера игрока
        RaycastHit2D hit = Physics2D.BoxCast(
            playerCollider.bounds.center,
            playerCollider.bounds.size,
            0f,
            Vector2.right * direction,
            extraWidth,
            LayerMask.GetMask("Ground") // убедись, что стены на этом слое
        );

        return hit.collider != null && !hit.collider.isTrigger;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerMat = spriteRenderer.material;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void Update()
    {
        Movement();
        Shear();
        Stretch();
        GroundHits();
        Jump();
    }

    public bool IsBelowPlayer(Collider2D hit)
    {
        float hitTop = hit.bounds.max.y; // Верхняя граница коллайдера земли
        float playerBottom = playerCollider.bounds.min.y; // Нижняя граница игрока

        float tolerance = 0.05f; // Допустимое расстояние
        return (playerBottom - hitTop) > -tolerance; // Проверка положения
    }

    private void FixedUpdate()
    {
        float targetX = Horizontal * speed * 10f;

        // Если в воздухе и прижат к стене — обнуляем горизонтальную скорость
        if (!isGrounded && Mathf.Abs(Horizontal) > 0.1f && IsTouchingWall())
        {
            targetX = 0f;
        }

        rb.linearVelocity = new Vector2(targetX, rb.linearVelocity.y);
    }


    /*
    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.green;
            // Рисуем куб вместо сферы
            Gizmos.DrawWireCube(
                groundCheckPoint.position,
                new Vector2(1, 1)
            );
        }
    }*/

    private void Movement()
    {
        Horizontal = Input.GetAxis("Horizontal");

        speed = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            ? SprintSpeed
            : NormalSpeed;

        jumpForce = JumpForce;
    }

    private void Jump()
    {
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Joystick1Button0)) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            gameManager.RestartLevel();
        }
    }

    private void GroundHits()
    {
        isGrounded = false;
        lastGroundCollider = null;

        // Создаём квадрат для проверки стоит ли игрок на земле
        Collider2D[] groundHits = Physics2D.OverlapBoxAll(
            groundCheckPoint.position,
            new Vector2(1, 1),
            0f
        );

        foreach (Collider2D hit in groundHits)
        {
            if (hit.isTrigger || hit == playerCollider) continue;

            if (IsBelowPlayer(hit))
            {
                isGrounded = true;
                lastGroundCollider = hit;
                break;
            }
        }

        if (isGrounded && lastGroundCollider != null)
        {
            Draggable draggable = lastGroundCollider.GetComponent<Draggable>();
            if (draggable != null && draggable.isBeingHeld)
            {
                isGrounded = false;
            }
        }
    }

    private void Shear()
    {
        float targetShear;

        // Если скорость игрока близка к нулю, не показываем анимацию shear
        if (Mathf.Abs(rb.linearVelocity.x) < 0.1f)
        {
            targetShear = 0f;
        }
        else
        {
            targetShear = Mathf.Clamp(Horizontal * 0.1f, -0.1f, 0.1f);
        }

        currentShear = Mathf.Lerp(currentShear, targetShear, Time.deltaTime * shearSpeed);
        playerMat.SetFloat("_Shear", currentShear);
    }

    // Растягивание по вертикали при прыжке
    private void Stretch()
    {
        float stretchTarget = 1f;

        // если игрок прыгает вверх
        if (rb.linearVelocity.y > 0.1f)
        {
            stretchTarget = 1.15f; // растягиваем
            stretchSpeed = 10;
        }
        // если игрок падает
        else if (rb.linearVelocity.y < -0.1f)
        {
            stretchTarget = 0.85f; // сжимаем
            stretchSpeed = 5;
        }
        // иначе игрок на земле
        else
        {
            stretchTarget = 1f;
            stretchSpeed = 10;
        }

        // плавное приближение к целевому значению
        currentStretch = Mathf.Lerp(currentStretch, stretchTarget, Time.deltaTime * stretchSpeed);
        playerMat.SetFloat("_Stretch", currentStretch);
    }
}