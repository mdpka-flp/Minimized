using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
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

    private bool isGrounded;
    private Collider2D lastGroundCollider;
    public GameManager gameManager;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
    }

    void Update()
    {
        Horizontal = Input.GetAxis("Horizontal");
        triggerValue = Input.GetAxis("Triggers");

        speed = (triggerValue > 0.1f || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            ? SprintSpeed
            : NormalSpeed;

        jumpForce = JumpForce;

        isGrounded = false;
        lastGroundCollider = null;

        // Создаём сферу для проверки стоит ли игрок на земле
        Collider2D[] groundHits = Physics2D.OverlapCircleAll(
            groundCheckPoint.position,
            groundCheckRadius
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

        // Прыжок
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Joystick1Button0)) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            gameManager.RestartLevel();
        }
    }

    private bool IsBelowPlayer(Collider2D hit)
    {
        float hitTop = hit.bounds.max.y; // Верхняя граница коллайдера земли
        float playerBottom = playerCollider.bounds.min.y; // Нижняя граница игрока

        float tolerance = 0.05f; // Допустимое расстояние
        return (playerBottom - hitTop) > -tolerance; // Проверка положения
    }

    private void FixedUpdate()
    {
        Vector2 targetVelocity = new Vector2(Horizontal * speed * 10f, rb.linearVelocity.y);
        rb.linearVelocity = targetVelocity;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }
    }
}